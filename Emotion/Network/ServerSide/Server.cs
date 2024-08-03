using Emotion.Network.Base;
using Emotion.Utility;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

#nullable enable

namespace Emotion.Network.ServerSide;

public class Server : NetworkCommunicator
{
    public List<ServerUser> ConnectedUsers = new List<ServerUser>();
    public ConcurrentDictionary<IPEndPoint, ServerUser> IPToUser = new();

    protected Server()
    {

    }

    public static T CreateServer<T>(int hostingPort) where T : Server, new()
    {
        var server = new T();

        server._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        server._socket.Bind(new IPEndPoint(IPAddress.Any, hostingPort));

        server.Ip = "127.0.0.1";
        server.Port = hostingPort;

        server.LogTag = "Server";

        server.SetupArgs();

        return server;
    }

    protected override void ProcessMessageInternal(NetworkMessage msg)
    {
        ByteReader? reader = msg.GetContentReader();
        AssertNotNull(reader);
        if (reader.Length < 1) return;

        NetworkMessageType msgType = (NetworkMessageType)reader.ReadInt8();
        switch (msgType)
        {
            case NetworkMessageType.RequestConnect:
                Msg_RequestConnect(msg);
                break;
             default:
                AssertNotNull(msg.Sender);
                if (msg.Sender != null && IPToUser.TryGetValue(msg.Sender, out ServerUser? user))
                    ServerProcessMessage(user, msg, reader);
                break;
        }
    }

    public void Msg_RequestConnect(NetworkMessage msg)
    {
        AssertNotNull(msg.Sender);

        if (IPToUser.TryGetValue(msg.Sender, out ServerUser? user))
        {
            user.SendMessage(this, NetworkMessageType.Connected);
        }
        else
        {
            Engine.Log.Info($"User connected - {msg.Sender}", LogTag);

            ServerUser newUser = ServerUser.Shared.Get();
            newUser.MyIP = msg.Sender;
            IPToUser.TryAdd(newUser.MyIP, newUser);

            ConnectedUsers.Add(newUser);
        }
    }

    protected virtual void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        // nop
    }
}
