using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Standard.XML;
using Emotion.Utility;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

#nullable enable

namespace Emotion.Network.ServerSide;

public class Server : NetworkCommunicator
{
    public CoroutineManager CoroutineManager = Engine.CoroutineManager;

    public bool UsersCanManageRooms { get; init; } = true;

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
            case NetworkMessageType.HostRoom when user != null && UsersCanManageRooms:
                Msg_HostRoom(user, msg, reader);
                break;
            case NetworkMessageType.GetRoomInfo when user != null && UsersCanManageRooms:
                Msg_GetRoomInfo(user);
                break;
            case NetworkMessageType.GetRooms when user != null && UsersCanManageRooms:
                Msg_GetRooms(user, msg, reader);
                break;
            case NetworkMessageType.JoinRoom when user != null && UsersCanManageRooms:
                Msg_JoinRoom(user, msg, reader);
                break;
            default:
                if (user != null) // Connected only.
                    ServerProcessMessage(user, msg, reader);
                break;
        }
    }

    #region Message Sending Helpers

    public void SendMessageRawToUser(ServerUser user, ReadOnlySpan<byte> data)
    {
        AssertNotNull(user.UserIp);
        SendMessageToIPRaw(user.UserIp, data, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    public void SendMessageToUser(ServerUser user, NetworkMessageType shorthand)
    {
        Span<byte> data = stackalloc byte[1];
        data[0] = (byte)shorthand;
        SendMessageRawToUser(user, data);
    }

    public void SendMessageToUser<TMsg>(ServerUser user, NetworkMessageType msgType, TMsg msgInfo)
    {
        AssertNotNull(user.UserIp);
        SendMessageToIP(user.UserIp, msgType, msgInfo, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    public void SendMessageToUsersWithTime<TMsg>(IEnumerable<ServerUser> users, int time, string method, TMsg metadata) where TMsg : struct
    {
        string xml = XMLFormat.To<TMsg>(metadata);
        SendMessageToUsersWithTime(users, time, method, xml);
    }

    public void SendMessageToUsersWithTime(IEnumerable<ServerUser> users, int time, string method, string metadata)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        // Message type
        spanData[0] = (byte)NetworkMessageType.GenericGameplayWithTime;
        bytesWritten += sizeof(byte);

        // Time
        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(bytesWritten), time);
        bytesWritten += sizeof(int);

        // Check if message is too long (ASCII encoding of strings assumed)
        if ((method.Length + metadata.Length) > spanData.Length - bytesWritten)
        {
            Assert(false);
            return;
        }
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), method);
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), metadata);

        // Send the message to all the users (this reuses the span!)
        Span<byte> sendData = spanData.Slice(0, bytesWritten);
        foreach (ServerUser user in users)
        {
            SendMessageRawToUser(user, sendData);
        }
    }

    public void SendMessageToUsers<TMsg>(IEnumerable<ServerUser> users, string method, TMsg metadata) where TMsg : struct
    {
        string xml = XMLFormat.To<TMsg>(metadata);

        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        // Message type
        spanData[0] = (byte)NetworkMessageType.GenericGameplay;
        bytesWritten += sizeof(byte);

        // Check if message is too long (ASCII encoding of strings assumed)
        if ((method.Length + xml.Length) > spanData.Length - bytesWritten)
        {
            Assert(false);
            return;
        }
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), method);
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), xml);

        // Send the message to all the users (this reuses the span!)
        Span<byte> sendData = spanData.Slice(0, bytesWritten);
        foreach (ServerUser user in users)
        {
            SendMessageRawToUser(user, sendData);
        }
    }

    public void BroadcastAdvanceTimeMessage(ServerRoom room, int time)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        spanData[0] = (byte)NetworkMessageType.GenericGameplayWithTime; //NIY_AdvanceTime
        bytesWritten += sizeof(byte);

        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(bytesWritten), time);
        bytesWritten += sizeof(int);

        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), "AdvanceTime");
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), XMLFormat.To(true));

        Span<byte> sendData = spanData.Slice(0, bytesWritten);

        // Send the message to all the users (this reuses the span!)
        List<ServerUser> users = room.UsersInside;
        foreach (ServerUser user in users)
        {
            SendMessageRawToUser(user, sendData);
        }
    }

    #endregion

    protected virtual void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        // nop
    }

    #region Connection

    protected void Msg_RequestConnect(NetworkMessage msg)
    {
        AssertNotNull(msg.Sender);

        // todo: connection challenge and encryption establish

        if (!IPToUser.TryGetValue(msg.Sender, out ServerUser? user))
        {
            Engine.Log.Info($"User connected - {msg.Sender}", LogTag);

            user = ServerUser.Shared.Get();
            user.UserIp = msg.Sender;
            user.Id = ConnectedUsers.Count + 1;
            IPToUser.TryAdd(user.UserIp, user);
            ConnectedUsers.Add(user);
            SendMessageToUser(user, NetworkMessageType.Connected, user.Id);
            OnUserConnected(user);
        }
        else
        {
            Span<byte> data = stackalloc byte[1];
            data[0] = (byte)NetworkMessageType.Error_AlreadyConnected;
            SendMessageToIPRaw(msg.Sender, data, 1);
        }
    }

    protected virtual void OnUserConnected(ServerUser user)
    {

    }

    #endregion

    #region Room Management

    protected void Msg_HostRoom(ServerUser user, NetworkMessage msg, ByteReader reader)
    {
        if (user.InRoom != null)
        {
            ServerRoom room = user.InRoom;
            room.UserLeave(this, user);
            if (!room.Active)
            {
                room.ServerGameplay = null;
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
        OnRoomCreated(newRoom);
    }

    protected virtual void OnRoomCreated(ServerRoom room)
    {
        // nop
    }

    protected void Msg_GetRoomInfo(ServerUser user)
    {
        if (user.InRoom == null)
        {
            SendMessageToUser(user, NetworkMessageType.Error_NotInRoom);
            return;
        }

        ServerRoom room = user.InRoom;
        SendMessageToUser(user, NetworkMessageType.RoomInfo, room.GetRoomInfo());
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

        SendMessageToUser(user, NetworkMessageType.RoomList, roomInfo);
    }

    protected void Msg_JoinRoom(ServerUser user, NetworkMessage msg, ByteReader reader)
    {
        if (!NetworkMessage.TryReadXMLDataFromMessage(reader, out int roomId)) return;

        ServerRoom? roomToJoin = null;
        for (int i = 0; i < ActiveRooms.Count; i++)
        {
            ServerRoom room = ActiveRooms[i];
            if (room.Id == roomId)
            {
                roomToJoin = room;
                break;
            }
        }
        if (roomToJoin == null) return;
        UserJoinRoom(user, roomToJoin);
    }

    protected void UserJoinRoom(ServerUser user, ServerRoom newRoom)
    {
        if (user.InRoom != null)
        {
            ServerRoom room = user.InRoom;
            room.UserLeave(this, user);
            if (!room.Active)
            {
                room.ServerGameplay = null;
                ActiveRooms.Remove(room);
                ServerRoom.Shared.Return(room);
            }
        }

        newRoom.UserJoin(this, user);
    }

    #endregion
}
