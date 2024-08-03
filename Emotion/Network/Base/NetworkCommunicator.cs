using Emotion.Network.ServerSide;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
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
        int bytesWritten = NetworkMessage.EncodeMessage(data, _sendBuffer, msgOrderIndex);
        _sendEventArgs.SetBuffer(_sendBuffer, 0, bytesWritten);
        _sendEventArgs.RemoteEndPoint = to;

        bool willRaiseEvent = _socket.SendToAsync(_sendEventArgs);
        if (!willRaiseEvent)
            MessageSent(_socket, _sendEventArgs);
    }

    public void Update()
    {
        switch (Status)
        {
            case ServerStatus.None:
                {
                    bool willRaiseEvent = _socket.ReceiveFromAsync(_receiveEventArgs);
                    if (!willRaiseEvent)
                        MessageReceived(null, _receiveEventArgs);
                    else
                        Status = ServerStatus.Listening;
                }
                break;
            case ServerStatus.Listening:
                break;
        }
    }

    public void PumpMessages()
    {
        while (_processQueue.TryDequeue(out NetworkMessage? msg))
        {
            msg.Process();
            if (msg.Valid)
            {
                var str = Encoding.UTF8.GetString(msg.Content.Span);
                Engine.Log.Info($"Got message {str} from {msg.Sender}", LogTag);

                ProcessMessageInternal(msg);
            }
            NetworkMessage.Shared.Return(msg);
        }
    }

    protected virtual void ProcessMessageInternal(NetworkMessage msg)
    {
        // nop
    }
}
