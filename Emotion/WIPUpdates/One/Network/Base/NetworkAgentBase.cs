using Emotion.Game.Time.Routines;
using Emotion.Network.ServerSide;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using Emotion.WIPUpdates.One.Network.ServerSide;

#nullable enable

namespace Emotion.WIPUpdates.One.Network.Base;

public abstract class NetworkAgentBase
{
    public static NetworkAgentBase? ActiveAgent;

    #region Metrics

    public int MetricBytesUploadedSec { get; protected set; }

    public int MetricBytesDownloadedSec { get; protected set; }

    public int MetricMessagesSec { get; protected set; }

    private int _currentSecond = 0;

    #endregion

    protected IPAddress Ip { get; init; }

    protected int Port { get; init; }

    protected IPEndPoint EndPoint { get; init; }

    public CoroutineManager CoroutineManager { get; init; }

    public string LogTag { get; init; }

    public ServerStatus Status { get; protected set; }

    protected Socket _socket;
    private Coroutine _updateRoutine = Coroutine.CompletedRoutine;

    private byte[] _sendBuffer = new byte[NetworkMessageData.MaxMessageSize];
    private SocketAsyncEventArgs _sendEventArgs;

    private MessagePair[] _sendingQueue = new MessagePair[1000];
    private int _sendQueueIdx = 0;
    private Lock _sendLock = new Lock();

    private byte[] _receiveBuffer = new byte[NetworkMessageData.MaxMessageSize];
    private SocketAsyncEventArgs _receiveEventArgs;

    private MessagePair[] _receivingQueue = new MessagePair[1000];
    private int _receiveQueueIdx = 0;
    private Lock _receiveLock = new Lock();

    private bool _bufferMessageReceiving;

    protected NetworkAgentBase(int hostingPort, CoroutineManager coroutineManager)
    {
        Ip = IPAddress.Loopback;
        Port = hostingPort;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        CoroutineManager = coroutineManager;

        EndPoint = new IPEndPoint(IPAddress.Any, hostingPort);
        _socket.Bind(EndPoint);

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
        _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageReceived);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSent);

        _updateRoutine = coroutineManager.StartCoroutine(UpdateRoutine());

        LogTag = "Server";
        ActiveAgent = this;
    }

    protected NetworkAgentBase(string serverAndPort, CoroutineManager coroutineManager)
    {
        IPEndPoint serverEndpoint = IPEndPoint.Parse(serverAndPort);

        Ip = serverEndpoint.Address;
        Port = serverEndpoint.Port;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        CoroutineManager = coroutineManager;

        EndPoint = serverEndpoint;
        _socket.Connect(serverEndpoint);

        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
        _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageReceived);

        _sendEventArgs = new SocketAsyncEventArgs();
        _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSent);

        _updateRoutine = coroutineManager.StartCoroutine(UpdateRoutine());

        LogTag = "Client";
        ActiveAgent = this;
    }

    public void Dispose()
    {
        _updateRoutine.RequestStop();
        _socket.Dispose();
    }

    #region Functions

    protected Dictionary<uint, NetworkFunction> _functions = new();

    public void RegisterFunction<TMsg>(uint typ, Action<TMsg> func) where TMsg : struct
    {
        if (GetFunctionFromMessageType(typ) != null) return;
        var netFunc = new NetworkFunction<TMsg>(typ, func);
        _functions.Add(typ, netFunc);
    }

    protected NetworkFunction? GetFunctionFromMessageType(uint typ)
    {
        _functions.TryGetValue(typ, out NetworkFunction? func);
        return func;
    }
    protected void CallNetFunc(ServerPlayer? sender, NetworkMessageData msg)
    {
        uint msgType = msg.Type;
        NetworkFunction? netFunc = GetFunctionFromMessageType(msgType);
        if (netFunc == null)
        {
            Engine.Log.Warning($"Unknown message type {msgType}", LogTag, true);
            return;
        }

        bool success = netFunc.TryInvoke(sender, msg);
        if (!success)
        {
            Engine.Log.Warning($"Error executing function of message type {msgType}", LogTag);
            return;
        }
    }

    #endregion

    private IEnumerator UpdateRoutine()
    {
        while (Engine.Status == EngineStatus.Running)
        {
            // Reset metrics
            int timeNowSecond = (int)(Engine.TotalTime / 1000);
            if (timeNowSecond != _currentSecond)
            {
                MetricBytesUploadedSec = 0;
                MetricBytesDownloadedSec = 0;
                MetricMessagesSec = 0;
                _currentSecond = timeNowSecond;
            }

            // Process message receiving from the socket
            while (Status == ServerStatus.None)
            {
                bool willRaiseEvent = _socket.ReceiveFromAsync(_receiveEventArgs);
                if (!willRaiseEvent)
                    MessageReceived(null, _receiveEventArgs);
                else
                    Status = ServerStatus.Listening;
            }

            // Pump messages, sending and processing.
            PumpMessages();

            yield return null;
        }
    }

    private unsafe void PumpMessages()
    {
        lock (_sendLock)
        {
            for (int i = 0; i < _sendQueueIdx; i++)
            {
                ref MessagePair msgPair = ref _sendingQueue[i];
                ref NetworkMessageData msg = ref msgPair.Message;

                int messageLength = NetworkMessageData.SizeWithoutContent + msgPair.Message.ContentLength;
                if (messageLength >= NetworkMessageData.MaxMessageSize)
                {
                    Engine.Log.Warning("Tried to send a message with a length larger than the maximum", LogTag);
                    continue;
                }

                // Convert the message to bytes
                Span<NetworkMessageData> msgAsSpan = new Span<NetworkMessageData>(ref msg);
                Span<byte> msgAsBytes = MemoryMarshal.Cast<NetworkMessageData, byte>(msgAsSpan);
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

                MetricBytesUploadedSec += messageLength;
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
                ref NetworkMessageData msg = ref msgPair.Message;

                // Already processed, happens when buffering is turned on via a message,
                // and then turned off.
                if (msg.Hash == MESSAGE_PROCESSED)
                    continue;

                // Validate message
                if (msg.Magic != NetworkMessageData.MagicNumber) // Invalid magic
                {
                    Engine.Log.Trace("Received message with invalid message magic", LogTag);
                    continue;
                }

                if (msg.ContentLength > NetworkMessageData.MaxContentSize) // Out of bounds
                {
                    Engine.Log.Trace("Received message with out of bounds content length", LogTag);
                    continue;
                }

                // Get message as bytes to check hash
                int hash = msg.Hash;
                msg.Hash = 0;
                Span<NetworkMessageData> msgAsSpan = new Span<NetworkMessageData>(ref msg);
                Span<byte> msgAsBytes = MemoryMarshal.Cast<NetworkMessageData, byte>(msgAsSpan);

                int messageLength = NetworkMessageData.SizeWithoutContent + msgPair.Message.ContentLength;
                msgAsBytes = msgAsBytes.Slice(0, messageLength);

                int hashOfMessage = msgAsBytes.GetStableHashCode();
                if (hashOfMessage != hash) // Hash doesn't match
                {
                    Engine.Log.Trace("Received message with invalid hash", LogTag);
                    continue;
                }

                ProcessMessage(msgPair.Recipient, msg);
                msg.Hash = MESSAGE_PROCESSED;
            }
            _receiveQueueIdx = 0;
        }
    }

    protected void MessageReceived(object? sender, SocketAsyncEventArgs args)
    {
        if (_receiveQueueIdx >= _receivingQueue.Length)
        {
            Engine.Log.Warning("Tried to receive a message, but the receive queue is full.", LogTag);
            return;
        }

        byte[]? buffer = args.Buffer;
        if (buffer == null) return;
        if (args.SocketError != SocketError.Success) return;
        if (args.RemoteEndPoint is not IPEndPoint senderIp) return;

        int bytesTransferred = args.BytesTransferred;
        MetricBytesDownloadedSec += bytesTransferred;
        MetricMessagesSec++;

        Status = ServerStatus.ParsingMessage;

        NetworkMessageData msg = new NetworkMessageData();
        Span<NetworkMessageData> msgAsSpan = new Span<NetworkMessageData>(ref msg);
        Span<byte> msgAsBytes = MemoryMarshal.Cast<NetworkMessageData, byte>(msgAsSpan);
        buffer.AsSpan().Slice(0, bytesTransferred).CopyTo(msgAsBytes);

        lock (_receiveLock)
        {
            ref MessagePair receivePair = ref _receivingQueue[_receiveQueueIdx];
            receivePair.Message = msg;
            receivePair.Recipient = senderIp;
            _receiveQueueIdx++;
        }

        Status = ServerStatus.None;
    }

    protected void MessageSent(object? sender, SocketAsyncEventArgs args)
    {

    }

    protected virtual void ProcessMessage(IPEndPoint from, NetworkMessageData msg)
    {

    }

    public unsafe void SendMessageToIP(IPEndPoint ip, NetworkMessageData msg, int messageIdx = 1)
    {
        msg.Magic = NetworkMessageData.MagicNumber;
        msg.MessageIndex = messageIdx;

        lock (_sendLock)
        {
            if (_sendQueueIdx >= _sendingQueue.Length)
            {
                Assert(false);
                return;
            }

            ref MessagePair sendPair = ref _sendingQueue[_sendQueueIdx];
            sendPair.Message = msg;
            sendPair.Recipient = ip;
            _sendQueueIdx++;
        }
    }

    public void BufferNetworkMessages(bool buffer)
    {
        _bufferMessageReceiving = buffer;
    }
}