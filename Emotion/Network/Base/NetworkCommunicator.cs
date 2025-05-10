using Emotion.Common.Serialization;
using Emotion.Network.ServerSide;
using Emotion.Standard.XML;
using Emotion.Utility;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

#nullable enable

namespace Emotion.Network.Base;

[DontSerialize]
public class NetworkCommunicator
{
    public ServerStatus Status;

    public int BytesUploadedSec { get; protected set; }

    public int BytesDownloadedSec { get; protected set; }

    public string Ip { get; protected set; }

    public int Port { get; protected set; }

    public string LogTag { get; protected set; }

    protected Socket _socket;
    private byte[] _receiveBuffer = new byte[NetworkMessage.MaxMessageSize];
    private byte[] _sendBuffer = new byte[NetworkMessage.MaxMessageSize];

    private SocketAsyncEventArgs _receiveEventArgs;
    private SocketAsyncEventArgs _sendEventArgs;

    private ConcurrentQueue<NetworkMessage> _sendQueue = new();
    private ConcurrentQueue<NetworkMessage> _processQueue = new();

    protected void SetupArgs()
    {
        _receiveEventArgs = new SocketAsyncEventArgs();
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
        _receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageReceived);

        _sendEventArgs = new SocketAsyncEventArgs();

        //_sendEventArgs.SetBuffer(_sendBuffer, 0, _sendBuffer.Length);
        _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(MessageSent);
    }

    protected virtual void MessageReceived(object? sender, SocketAsyncEventArgs args)
    {
        Status = ServerStatus.ParsingMessage;

        var buffer = args.Buffer;
        if (args.SocketError == SocketError.Success && buffer != null && args.RemoteEndPoint is IPEndPoint senderIp)
        {
            var msg = NetworkMessage.Shared.Get();
            msg.CopyToLocalBuffer(senderIp, buffer);
            _processQueue.Enqueue(msg);
        }

        Status = ServerStatus.None;
    }

    protected virtual void MessageSent(object? sender, SocketAsyncEventArgs args)
    {

    }

    public void SendMessageToIPRaw(IPEndPoint to, ReadOnlySpan<byte> data, int msgOrderIndex)
    {
        NetworkMessage newMessage = NetworkMessage.Shared.Get();
        newMessage.EncodeMessageInLocalBuffer(to, data, msgOrderIndex);
        newMessage.Index = msgOrderIndex;
        _sendQueue.Enqueue(newMessage);
    }

    public void SendMessageToIP<T>(IPEndPoint to, NetworkMessageType msgType, T msgInfo, int msgOrderIndex)
    {
        int bytesWritten = 0;

        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        spanData[0] = (byte)msgType;
        bytesWritten += sizeof(byte);

        string metaAsString = XMLFormat.To(msgInfo) ?? string.Empty;
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), metaAsString);

        AssertNotNull(to);
        SendMessageToIPRaw(to, spanData.Slice(0, bytesWritten), msgOrderIndex);
    }

    public void Update()
    {
        switch (Status)
        {
            case ServerStatus.None:
                while (Status == ServerStatus.None)
                {
                    bool willRaiseEvent = _socket.ReceiveFromAsync(_receiveEventArgs);
                    if (!willRaiseEvent)
                        MessageReceived(null, _receiveEventArgs);
                    else
                        Status = ServerStatus.Listening;
                }
                break;
            case ServerStatus.Listening:
                UpdateInternal();
                break;
        }
        PumpMessages();
    }

    public void InitAutoUpdate()
    {
        Engine.CoroutineManager.StartCoroutine(AutoUpdateRoutine());
    }

    private IEnumerator AutoUpdateRoutine()
    {
        while (Engine.Status == EngineStatus.Running)
        {
            Update();
            yield return null;
        }
    }

    protected virtual void UpdateInternal()
    {

    }

    private void PumpMessages()
    {
        while (_sendQueue.TryDequeue(out NetworkMessage? msg))
        {
            ReadOnlyMemory<byte> msgContent = msg.Content;
            msgContent.CopyTo(_sendBuffer);
            _sendEventArgs.SetBuffer(_sendBuffer, 0, msgContent.Length);
            _sendEventArgs.RemoteEndPoint = msg.Sender;

            //Engine.Log.Info($"Sent message to {msg.Sender}", LogTag);

            bool willRaiseEvent = _socket.SendToAsync(_sendEventArgs);
            if (!willRaiseEvent)
                MessageSent(_socket, _sendEventArgs);

            BytesUploadedSec += msgContent.Length;

            NetworkMessage.Shared.Return(msg);
        }

        while (!_bufferReceivedMessages && _processQueue.TryDequeue(out NetworkMessage? msg))
        {
            msg.Process();
            if (msg.Valid)
            {
                //var str = Encoding.UTF8.GetString(msg.Content.Span);
                //Engine.Log.Info($"Got message {str} from {msg.Sender}", LogTag);

                BytesDownloadedSec += msg.Content.Length;
                ProcessMessageInternal(msg);
            }

            if (msg.AutoFree)
            {
                Assert(!_sendQueue.Contains(msg));
                NetworkMessage.Shared.Return(msg);
            }
        }
    }

    private bool _bufferReceivedMessages;

    public void BufferNetworkMessages(bool enable)
    {
        _bufferReceivedMessages = enable;
    }

    protected virtual void ProcessMessageInternal(NetworkMessage msg)
    {
        // nop
    }

    public void Dispose()
    {
        //_socket?.Disconnect(true);
        _socket?.Dispose();
    }
}
