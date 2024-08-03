using Emotion.Network.Base;
using Emotion.Utility;
using System.Net;
using System.Net.Sockets;

#nullable enable

namespace Emotion.Network.ClientSide;

public class Client : NetworkCommunicator
{
    public bool ConnectedToServer { get; protected set; }

    public IPEndPoint? _serverEndPoint { get; protected set; }

    private int _messageIndex = 1;

    public Client()
    {
    }

    public static T CreateClient<T>(string serverIp, int serverPort) where T : Client, new()
    {
        var client = new T();

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

    public void ConnectIfNotConnected()
    {
        if (ConnectedToServer) return;

        Span<byte> reqConnect = stackalloc byte[1];
        reqConnect[0] = (byte)NetworkMessageType.RequestConnect;
        SendMessageToServer(reqConnect);
    }

    public void SendMessageToServer(Span<byte> data)
    {
        SendMessage(data, _serverEndPoint, _messageIndex);
        _messageIndex++;
    }

    protected override void ProcessMessageInternal(NetworkMessage msg)
    {
        var reader = msg.GetContentReader();
        AssertNotNull(reader);
        if (reader.Length < 1) return;

        NetworkMessageType msgType = (NetworkMessageType)reader.ReadInt8();
        switch (msgType)
        {
            case NetworkMessageType.Connected:
                Msg_Connected(msg);
                break;
            default:
                ClientProcessMessage(msg, reader);
                break;
        }
    }

    protected virtual void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        // nop
    }

    public void Msg_Connected(NetworkMessage msg)
    {
        AssertNotNull(msg.Sender);
        ConnectedToServer = true;
        Engine.Log.Info($"Connected to server - {_serverEndPoint}", LogTag);
    }
}
