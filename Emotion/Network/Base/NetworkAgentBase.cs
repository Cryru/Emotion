#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Network.New.Base;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Emotion.Network.Base;

public enum NetworkAgentStatus
{
    None,
    Listening,
    ParsingMessage
}

public abstract class NetworkAgentBase
{
    #region Metrics

    public int MetricBytesUploadedSec { get; protected set; }

    public int MetricBytesDownloadedSec { get; protected set; }

    public string MetricText { get => $"U:{Helpers.FormatByteAmountAsString(MetricBytesUploadedSec)}\\D:{Helpers.FormatByteAmountAsString(MetricBytesDownloadedSec)}"; }

    public int MetricMessagesSec { get; protected set; }

    private int _currentSecond = 0;
    private int _bytesUploadedThisSecond = 0;
    private int _bytesDownloadedThisSecond = 0;

    #endregion

    protected IPAddress Ip { get; init; }

    protected int Port { get; init; }

    public IPEndPoint EndPoint { get; init; }

    public string LogTag { get; init; }

    public NetworkAgentStatus Status { get; protected set; }

    protected Socket? _socket;
    private Coroutine _updateRoutine = Coroutine.CompletedRoutine;

    private byte[] _sendBuffer = new byte[NetworkMessage.MaxMessageSize];
    private SocketAsyncEventArgs _sendEventArgs;

    private MessagePair[] _sendingQueue = new MessagePair[1000];
    private int _sendQueueIdx = 0;
    private Lock _sendLock = new Lock();

    private byte[] _receiveBuffer = new byte[NetworkMessage.MaxMessageSize];
    private SocketAsyncEventArgs _receiveEventArgs;

    private MessagePair[] _receivingQueue = new MessagePair[1000];
    private int _receiveQueueIdx = 0;
    private Lock _receiveLock = new Lock();

    private bool _bufferMessageReceiving;

    protected NetworkAgentBase(IPEndPoint endPoint, string logTag) : this(endPoint.Address, endPoint.Port, endPoint, logTag)
    {
    }

    protected NetworkAgentBase(IPAddress ip, int port, IPEndPoint endPoint, string logTag)
    {
        Ip = ip;
        Port = port;
        EndPoint = endPoint;
        LogTag = logTag;

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
        _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageReceived);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSent);

        _updateRoutine = Engine.CoroutineManager.StartCoroutine(UpdateNetworkRoutine());
    }

    public void Dispose()
    {
        _updateRoutine.RequestStop();
        _socket?.Dispose();
        _socket = null;
    }

    private IEnumerator UpdateNetworkRoutine()
    {
        while (Engine.Status == EngineState.Running)
        {
            // Reset metrics
            int timeNowSecond = (int)(Engine.TotalTime / 1000);
            if (timeNowSecond != _currentSecond)
            {
                MetricBytesUploadedSec = _bytesUploadedThisSecond;
                MetricBytesDownloadedSec = _bytesDownloadedThisSecond;
                _bytesUploadedThisSecond = 0;
                _bytesDownloadedThisSecond = 0;
                MetricMessagesSec = 0;
                _currentSecond = timeNowSecond;
            }

            // Pump messages, sending and processing.
            if (_socket != null)
                UpdateNetwork_PumpMessages();

            yield return null;
        }
    }

    private unsafe void UpdateNetwork_PumpMessages()
    {
        AssertNotNull(_socket);

        // Process message receiving from the socket
        while (Status == NetworkAgentStatus.None)
        {
            bool willRaiseEvent = _socket.ReceiveFromAsync(_receiveEventArgs);
            if (!willRaiseEvent)
                MessageReceived(null, _receiveEventArgs);
            else
                Status = NetworkAgentStatus.Listening;
        }

        lock (_sendLock)
        {
            for (int i = 0; i < _sendQueueIdx; i++)
            {
                ref MessagePair msgPair = ref _sendingQueue[i];
                ref NetworkMessage msg = ref msgPair.Message;

                int messageLength = NetworkMessage.SizeWithoutContent + msgPair.Message.ContentLength;
                if (messageLength > NetworkMessage.MaxMessageSize)
                {
                    Engine.Log.Warning("Tried to send a message with a length larger than the maximum", LogTag);
                    continue;
                }

                // Convert the message to bytes
                Span<NetworkMessage> msgAsSpan = new Span<NetworkMessage>(ref msg);
                Span<byte> msgAsBytes = MemoryMarshal.AsBytes<NetworkMessage>(msgAsSpan);
                msgAsBytes = msgAsBytes.Slice(0, messageLength);

                // Hash the message
                msg.Hash = 0;
                int hash = msgAsBytes.GetStableHashCode();
                msg.Hash = hash;

                // Setup sending
                msgAsBytes.CopyTo(_sendBuffer);
                _sendEventArgs.SetBuffer(_sendBuffer, 0, messageLength);
                _sendEventArgs.RemoteEndPoint = msgPair.Recipient;

                bool willRaiseEvent = _socket.SendToAsync(_sendEventArgs);
                if (!willRaiseEvent)
                    MessageSent(_socket, _sendEventArgs);

                _bytesUploadedThisSecond += messageLength;
            }
            _sendQueueIdx = 0;
        }

        const int MESSAGE_PROCESSED = 1337;
        lock (_receiveLock)
        {
            for (int i = 0; i < _receiveQueueIdx; i++)
            {
                if (_bufferMessageReceiving) return;

                ref MessagePair msgPair = ref _receivingQueue[i];
                ref NetworkMessage msg = ref msgPair.Message;

                // Already processed, happens when buffering is
                // turned on via a message callback and then turned off.
                if (msg.Hash == MESSAGE_PROCESSED)
                    continue;

                // Validate message
                if (msg.Magic != NetworkMessage.MagicNumber) // Invalid magic
                {
                    Engine.Log.Trace("Received message with invalid message magic", LogTag);
                    continue;
                }

                if (msg.ContentLength > NetworkMessage.MaxContentSize) // Out of bounds
                {
                    Engine.Log.Trace("Received message with out of bounds content length", LogTag);
                    continue;
                }

                // Get message as bytes to check hash
                int hash = msg.Hash;
                msg.Hash = 0;
                Span<NetworkMessage> msgAsSpan = new Span<NetworkMessage>(ref msg);
                Span<byte> msgAsBytes = MemoryMarshal.Cast<NetworkMessage, byte>(msgAsSpan);

                int messageLength = NetworkMessage.SizeWithoutContent + msgPair.Message.ContentLength;
                msgAsBytes = msgAsBytes.Slice(0, messageLength);

                int hashOfMessage = msgAsBytes.GetStableHashCode();
                if (hashOfMessage != hash) // Hash doesn't match
                {
                    Engine.Log.Trace("Received message with invalid hash", LogTag);
                    continue;
                }

                ProcessMessage(msgPair.Recipient, ref msg);
                msg.Hash = MESSAGE_PROCESSED;
            }
            _receiveQueueIdx = 0;
        }
    }

    protected unsafe void MessageReceived(object? sender, SocketAsyncEventArgs args)
    {
        if (_receiveQueueIdx >= _receivingQueue.Length)
        {
            Engine.Log.Warning("Tried to receive a message, but the receive queue is full.", LogTag);
            return;
        }

        byte[]? buffer = args.Buffer;
        if (buffer == null) return;
        if (args.SocketError != SocketError.Success)
        {
            Engine.Log.Warning($"Socket error {args.SocketError} in MessageReceived", LogTag, true);
            return;
        }

        if (args.RemoteEndPoint is not IPEndPoint senderIp) return;

        int bytesTransferred = args.BytesTransferred;
        if (bytesTransferred > NetworkMessage.MaxMessageSize)
        {
            Engine.Log.Warning("Socket sent us too many bytes for a single message.", LogTag);
            return;
        }

        Span<byte> receivedBytes = buffer.AsSpan().Slice(0, bytesTransferred);

        _bytesDownloadedThisSecond += bytesTransferred;
        MetricMessagesSec++;

        Assert(Status == NetworkAgentStatus.None || Status == NetworkAgentStatus.Listening);
        Status = NetworkAgentStatus.ParsingMessage;

        lock (_receiveLock)
        {
            ref MessagePair receivePair = ref _receivingQueue[_receiveQueueIdx];
            fixed (NetworkMessage* msgPtr = &receivePair.Message)
            {
                var dstMsg = new Span<byte>(msgPtr, bytesTransferred);
                receivedBytes.CopyTo(dstMsg);
            }
            receivePair.Recipient = senderIp;
            _receiveQueueIdx++;
        }

        Status = NetworkAgentStatus.None;
    }

    protected void MessageSent(object? sender, SocketAsyncEventArgs args)
    {

    }

    protected virtual void ProcessMessage(IPEndPoint from, ref NetworkMessage msg)
    {

    }

    #region Message Creators

    public static NetworkMessage CreateMessageWithoutData<TEnum>(TEnum messageType)
        where TEnum : unmanaged
    {
        Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(uint));

        var msg = new NetworkMessage
        {
            Type = Unsafe.As<TEnum, uint>(ref messageType),
            Magic = NetworkMessage.MagicNumber
        };
        return msg;
    }

    public unsafe static NetworkMessage CreateMessage<TEnum, TData>(TEnum messageType, in TData data)
       where TEnum : unmanaged
       where TData : unmanaged
    {
        Assert(sizeof(TData) <= NetworkMessage.MaxContentSize);
        AssertSequentialLayout<TData>();
        ReadOnlySpan<TData> dataAsSpan = new ReadOnlySpan<TData>(in data);
        ReadOnlySpan<byte> dataAsBytes = MemoryMarshal.AsBytes(dataAsSpan);
        return CreateMessage<TEnum>(messageType, dataAsBytes);
    }

    public unsafe static NetworkMessage CreateMessage<TEnum>(TEnum messageType, ReadOnlySpan<byte> dataAsBytes)
        where TEnum : unmanaged
    {
        Assert(dataAsBytes.Length <= NetworkMessage.MaxContentSize);

        var msg = new NetworkMessage
        {
            Type = Unsafe.As<TEnum, uint>(ref messageType),
            Magic = NetworkMessage.MagicNumber
        };

        if (dataAsBytes.Length > NetworkMessage.MaxContentSize)
        {
            // todo: some sort of invalid msg?
            msg.Type = (uint) NetworkMessageType.None;
            msg.ContentLength = 0;
            return msg;
        }

        Span<byte> contentSpan = new Span<byte>(msg.Content, NetworkMessage.MaxContentSize);
        dataAsBytes.CopyTo(contentSpan);
        msg.ContentLength = dataAsBytes.Length;

        return msg;
    }

    #endregion

    #region Sending

    public void SendMessageToIPRaw(IPEndPoint ip, in NetworkMessage msg, int messageIdx = 1)
    {
        lock (_sendLock)
        {
            if (_sendQueueIdx >= _sendingQueue.Length)
            {
                Assert(false);
                return;
            }

            ref MessagePair sendPair = ref _sendingQueue[_sendQueueIdx];
            sendPair.Message = msg; // Copy :/
            sendPair.Message.MessageIndex = messageIdx;
            sendPair.Recipient = ip;
            _sendQueueIdx++;
        }
    }

    public void SendMessageToIP<TData>(IPEndPoint ip, in TData data, int messageIdx = 1)
        where TData : unmanaged, INetworkMessageStruct
    {
        uint messageType = TData.MessageType;
        NetworkMessage msg = CreateMessage(messageType, data);
        SendMessageToIPRaw(ip, msg, messageIdx);
    }

    public void SendMessageToIP<TEnum, TData>(IPEndPoint ip, TEnum messageType, in TData data, int messageIdx = 1)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        NetworkMessage msg = CreateMessage(messageType, data);
        SendMessageToIPRaw(ip, msg, messageIdx);
    }

    public void SendMessageToIPWithoutData<TEnum>(IPEndPoint ip, TEnum messageType, int messageIdx = 1)
        where TEnum : unmanaged
    {
        NetworkMessage msg = CreateMessageWithoutData(messageType);
        SendMessageToIPRaw(ip, msg, messageIdx);
    }

    #endregion

    public void BufferNetworkMessages(bool buffer)
    {
        _bufferMessageReceiving = buffer;
    }

    private static void AssertSequentialLayout<T>()
    {
        Type t = typeof(T);
        if (t.IsPrimitive) return;

        StructLayoutAttribute? attr = t.StructLayoutAttribute;
        if (attr == null) return;

        Assert(attr.Value == LayoutKind.Sequential, $"{t.FullName} must be marked with [StructLayout(LayoutKind.Sequential, Pack = 1)]");
        Assert(attr.Pack == 1, $"{t.FullName} must specify Pack = 1.");
    }
}

[DontSerialize]
public interface INetworkMessageStruct
{
    public static abstract uint MessageType { get; }
}