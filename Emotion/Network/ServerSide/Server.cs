using Emotion.Network.Base;
using Emotion.Utility;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

#nullable enable

namespace Emotion.Network.ServerSide;

public class Server : NetworkCommunicator
{
    public List<ServerUser> ConnectedUsers = new List<ServerUser>();
    public List<ServerRoom> ActiveRooms = new List<ServerRoom>();

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

        AssertNotNull(msg.Sender);
        ServerUser? user = null;
        if (msg.Sender != null)
            IPToUser.TryGetValue(msg.Sender, out user);

        if (user != null)
        {
            // Old message
            if (user.ReceiveMessageIndex > msg.Index)
            {
                Engine.Log.Trace($"Received old message from {user.Id}. Discarding!", LogTag);
                return;
            }
            user.ReceiveMessageIndex = msg.Index;
        }

        NetworkMessageType msgType = (NetworkMessageType)reader.ReadInt8();
        msg.MessageType = msgType;

        switch (msgType)
        {
            case NetworkMessageType.RequestConnect:
                Msg_RequestConnect(msg);
                break;
            case NetworkMessageType.HostRoom when user != null:
                Msg_HostRoom(user, msg, reader);
                break;
            case NetworkMessageType.GetRoomInfo when user != null:
                Msg_GetRoomInfo(user);
                break;
            case NetworkMessageType.GetRooms when user != null:
                Msg_GetRooms(user, msg, reader);
                break;
            case NetworkMessageType.JoinRoom when user != null:
                Msg_JoinRoom(user, msg, reader);
                break;
            default:
                if (user != null) // Connected only.
                    ServerProcessMessage(user, msg, reader);
                break;
        }
    }

    protected void Msg_RequestConnect(NetworkMessage msg)
    {
        AssertNotNull(msg.Sender);

        if (!IPToUser.TryGetValue(msg.Sender, out ServerUser? user))
        {
            Engine.Log.Info($"User connected - {msg.Sender}", LogTag);

            user = ServerUser.Shared.Get();
            user.MyIP = msg.Sender;
            user.Id = ConnectedUsers.Count + 1;
            IPToUser.TryAdd(user.MyIP, user);
            ConnectedUsers.Add(user);
        }

        SendMessage(user, NetworkMessageType.Connected, user.Id);
    }

    protected void Msg_HostRoom(ServerUser user, NetworkMessage msg, ByteReader reader)
    {
        if (user.InRoom != null)
        {
            ServerRoom room = user.InRoom;
            room.UserLeave(this, user);
            if (!room.Active)
            {
                ActiveRooms.Remove(room);
                ServerRoom.Shared.Return(room);
            }
        }

        ServerRoom newRoom = ServerRoom.Shared.Get();
        newRoom.Id = ActiveRooms.Count + 1;
        newRoom.Host = user;
        newRoom.Active = true;
        newRoom.UserJoin(this, user);
        ActiveRooms.Add(newRoom);

        Engine.Log.Info($"New room created: {newRoom.Id}", LogTag);
        RoomCreated(newRoom);
    }

    protected void Msg_GetRoomInfo(ServerUser user)
    {
        if (user.InRoom == null)
        {
            SendMessage(user, NetworkMessageType.NotInRoom);
            return;
        }

        ServerRoom room = user.InRoom;
        SendMessage(user, NetworkMessageType.RoomInfo, room.GetRoomInfo());
    }

    protected void Msg_GetRooms(ServerUser user, NetworkMessage msg, ByteReader reader)
    {
        List<ServerRoomInfo> roomInfo = new List<ServerRoomInfo>(10);
        for (int i = 0; i < ActiveRooms.Count; i++)
        {
            var room = ActiveRooms[i];
            Assert(room.Active);
            roomInfo.Add(room.GetRoomInfo());
            if (roomInfo.Count > 10) break;
        }

        SendMessage(user, NetworkMessageType.RoomList, roomInfo);
    }

    protected void Msg_JoinRoom(ServerUser user, NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out int roomId)) return;

        if (user.InRoom != null)
        {
            ServerRoom room = user.InRoom;
            room.UserLeave(this, user);
            if (!room.Active)
            {
                ActiveRooms.Remove(room);
                ServerRoom.Shared.Return(room);
            }
        }

        for (int i = 0; i < ActiveRooms.Count; i++)
        {
            var room = ActiveRooms[i];
            if (room.Id == roomId)
            {
                room.UserJoin(this, user);
                break;
            }
        }
    }

    public void SendMessage(ServerUser user, ReadOnlySpan<byte> data)
    {
        AssertNotNull(user.MyIP);
        SendMessage(data, user.MyIP, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    public void SendMessage(ServerUser user, NetworkMessageType shorthand)
    {
        Span<byte> data = stackalloc byte[1];
        data[0] = (byte)shorthand;

        AssertNotNull(user.MyIP);
        SendMessage(data, user.MyIP, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    public void SendMessage<T>(ServerUser user, NetworkMessageType msgType, T msgInfo)
    {
        AssertNotNull(user.MyIP);
        SendMessage(user.MyIP, msgType, msgInfo, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    protected virtual void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        // nop
    }

    protected virtual void RoomCreated(ServerRoom room)
    {
        // nop
    }
}
