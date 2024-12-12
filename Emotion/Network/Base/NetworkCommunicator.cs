using Emotion.Network.ServerSide;
using Emotion.Standard.XML;
using Emotion.Utility;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

#nullable enable

namespace Emotion.Network.Base;

public class NetworkCommunicator
{
    public ServerStatus Status;

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

    public void SendMessage(ReadOnlySpan<byte> data, IPEndPoint to, int msgOrderIndex)
    {
        NetworkMessage newMessage = NetworkMessage.Shared.Get();
        newMessage.EncodeMessageInLocalBuffer(to, data, msgOrderIndex);
        newMessage.Index = msgOrderIndex;
        _sendQueue.Enqueue(newMessage);
    }

    public void SendMessage<T>(IPEndPoint to, NetworkMessageType msgType, T msgInfo, int msgOrderIndex)
    {
        Span<byte> data = stackalloc byte[NetworkMessage.MaxMessageContent];
        data[0] = (byte)msgType;

        string? metaAsString = XMLFormat.To(msgInfo);
        int byteCount = Encoding.UTF8.GetByteCount(metaAsString);
        MemoryMarshal.Write(data.Slice(1), byteCount);

        int written = Encoding.UTF8.GetBytes(metaAsString, data.Slice(1 + sizeof(int)));
        data = data.Slice(0, written + 1 + sizeof(int));

        AssertNotNull(to);
        SendMessage(data, to, msgOrderIndex);
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

    protected virtual void UpdateInternal()
    {

    }

    private void PumpMessages()
    {
        while (_sendQueue.TryDequeue(out NetworkMessage? msg))
        {
            msg.Content.CopyTo(_sendBuffer);
            _sendEventArgs.SetBuffer(_sendBuffer, 0, msg.Content.Length);
            _sendEventArgs.RemoteEndPoint = msg.Sender;

            //Engine.Log.Info($"Sent message to {msg.Sender}", LogTag);

            bool willRaiseEvent = _socket.SendToAsync(_sendEventArgs);
            if (!willRaiseEvent)
                MessageSent(_socket, _sendEventArgs);

            NetworkMessage.Shared.Return(msg);
        }

        while (_processQueue.TryDequeue(out NetworkMessage? msg))
        {
            msg.Process();
            if (msg.Valid)
            {
                //var str = Encoding.UTF8.GetString(msg.Content.Span);
                //Engine.Log.Info($"Got message {str} from {msg.Sender}", LogTag);
                
                ProcessMessageInternal(msg);
            }

            if (msg.AutoFree)
                NetworkMessage.Shared.Return(msg);
        }
    }

    protected virtual void ProcessMessageInternal(NetworkMessage msg)
    {
        // nop
    }

    public static int WriteStringToMessage(Span<byte> spanDataCur, string str)
    {
        int byteCount = Encoding.ASCII.GetByteCount(str);
        BinaryPrimitives.WriteInt32LittleEndian(spanDataCur, byteCount);
        int methodNameBytesWritten = Encoding.ASCII.GetBytes(str, spanDataCur.Slice(sizeof(int)));
        return methodNameBytesWritten + sizeof(int);
    }
}
