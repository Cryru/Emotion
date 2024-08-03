using Emotion.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WinApi.User32;

namespace Emotion.Network;

public class NetworkCommunicator
{
    public ServerStatus Status;

    public string Ip;
    public int Port;

    public string LogTag;

    public const int BufferSize = 1024 * 1024;

    private Socket _socket;
    private byte[] _receiveBuffer = new byte[BufferSize];
    private byte[] _sendBuffer = new byte[BufferSize];

    private SocketAsyncEventArgs _receiveEventArgs;
    private SocketAsyncEventArgs _sendEventArgs;

    public static Client CreateClient(string serverIp, int serverPort)
    {
        var client = new Client();

        IPEndPoint serverEndpoint = IPEndPoint.Parse(serverIp + ":" + serverPort);
        client._serverEndPoint = serverEndpoint;

        client._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        client._socket.Connect(serverEndpoint);

        client.Ip = serverIp;
        client.Port = serverPort;

        client.LogTag = "Client";

        client.SetupArgs();

        return client;
    }

    public static Server CreateServer(int hostingPort)
    {
        var server = new Server();

        server._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        server._socket.Bind(new IPEndPoint(IPAddress.Any, hostingPort));

        server.Ip = "127.0.0.1";
        server.Port = hostingPort;

        server.LogTag = "Server";

        server.SetupArgs();

        return server;
    }

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
        var str = System.Text.Encoding.UTF8.GetString(buffer);
        if(!str.StartsWith("pepega"))
        {
            if (args.RemoteEndPoint is IPEndPoint clientIp)
            {
                SendMessage("pepega", clientIp);
            }

            
        }

        Engine.Log.Info($"Got message {str} from {args.RemoteEndPoint}", LogTag);
        

        Status = ServerStatus.None;
    }

    protected virtual void MessageSent(object? sender, SocketAsyncEventArgs args)
    {

    }

    public void SendMessage(string msg, IPEndPoint to)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(msg);

        _sendEventArgs.SetBuffer(messageBytes, 0, messageBytes.Length);
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
}
