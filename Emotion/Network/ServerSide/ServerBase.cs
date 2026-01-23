#nullable enable

using Emotion;
using Emotion.Network.Base;
using Emotion.Network.Base.Invocation;
using Emotion.Network.New.Base;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Emotion.Network.ServerSide;

public class ServerBase : NetworkAgentBase
{
    public List<ServerPlayer> ConnectedUsers = new();
    public ConcurrentDictionary<IPEndPoint, ServerPlayer> IPToUser = new();

    public ServerBase(int hostingPort) : base()
    {
        Ip = IPAddress.Loopback;
        Port = hostingPort;
        EndPoint = new IPEndPoint(IPAddress.Any, hostingPort);

        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(EndPoint);

        LogTag = "Server";
    }

    protected override void ProcessMessage(IPEndPoint from, ref NetworkMessage msg)
    {
        IPToUser.TryGetValue(from, out ServerPlayer? sender);

        // Manually handle connection establishing messages here.
        uint messageType = msg.Type;
        switch (messageType)
        {
            case (uint)NetworkMessageType.RequestConnect:
                if (sender == null)
                    Msg_RequestConnect(from, ref msg);
                else
                    SendMessageToIPWithoutData(from, NetworkMessageType.Error_AlreadyConnected);
                break;
        }

        if (sender == null)
        {
            Engine.Log.Trace($"Message from unknown sender - {from}", LogTag);
            return;
        }

        if (sender.ReceiveMessageIndex > msg.MessageIndex)
        {
            Engine.Log.Trace($"Received old message from {sender.Id}. Discarding!", LogTag);
            return;
        }
        sender.ReceiveMessageIndex = msg.MessageIndex;

        NetworkFunctionBase<ServerBase, ServerPlayer>? netFunc = _netFuncs.GetFunctionFromMessageType(messageType);
        if (netFunc != null)
        {
            if (!netFunc.TryInvoke(this, sender, msg))
            {
                Engine.Log.Warning($"Error executing function of message type {messageType}", LogTag);
            }
        }
        else if (sender.InRoom == null || !sender.InRoom.ProcessMessage(this, sender, msg))
        {
            Engine.Log.Warning($"Neither server nor room could handle {messageType}", LogTag, true);
        }
    }

    #region Sender Helpers

    public void SendMessageToPlayer<TData>(ServerPlayer player, in TData data)
        where TData : unmanaged, INetworkMessageStruct
    {
        IPEndPoint? ip = player.UserIp;
        if (ip == null) return;

        SendMessageToIP(ip, data, player.SendMessageIndex);
        player.SendMessageIndex++;
    }

    public void SendMessageToPlayer<TEnum, TData>(ServerPlayer player, TEnum messageType, in TData data)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        IPEndPoint? ip = player.UserIp;
        if (ip == null) return;

        SendMessageToIP(ip, messageType, data, player.SendMessageIndex);
        player.SendMessageIndex++;
    }

    public void SendMessageToPlayerWithoutData<TEnum>(ServerPlayer player, TEnum messageType)
        where TEnum : unmanaged
    {
        IPEndPoint? ip = player.UserIp;
        if (ip == null) return;

        SendMessageToIPWithoutData(ip, messageType, player.SendMessageIndex);
        player.SendMessageIndex++;
    }

    #endregion

    #region Connection Establish

    private void Msg_RequestConnect(IPEndPoint ip, ref NetworkMessage msg)
    {
        // todo: connection challenge and encryption establish

        Engine.Log.Info($"User connected - {ip}", LogTag);

        var user = new ServerPlayer();
        user.UserIp = ip;
        user.Id = ConnectedUsers.Count + 1;
        IPToUser.TryAdd(user.UserIp, user);
        ConnectedUsers.Add(user);

        SendMessageToPlayer(user, NetworkMessageType.Connected, user.Id);
        OnUserConnected(user);
    }

    #endregion

    #region Rooms

    public bool UsersCanManageRooms { get; init; } = true;

    protected List<ServerRoom> _rooms { get; } = new();
    private static uint _nextFreeRoomId = 1;

    protected virtual ServerRoom CreateNewRoomImplementation(ServerPlayer? host, uint newRoomId)
    {
        return new ServerRoom(this, host, newRoomId);
    }

    private void CreateNewRoom(ServerPlayer? host)
    {
        ServerRoom newRoom = CreateNewRoomImplementation(host, _nextFreeRoomId);
        _rooms.Add(newRoom);
        Engine.Log.Info($"New room created: {newRoom.Id}", LogTag);
        _nextFreeRoomId++;

        if (host != null)
        {
            bool success = AddPlayerToRoom(host, newRoom);
            Assert(success && host.InRoom == newRoom);
        }
    }

    public bool AddPlayerToRoom(ServerPlayer user, ServerRoom roomToJoin)
    {
        if (!roomToJoin.CanBeJoined(user))
            return false;

        RemovePlayerFromRoom(user); // Remove from old room
        roomToJoin.UserJoin(user);
        user.InRoom = roomToJoin;

        return true;
    }

    public void RemovePlayerFromRoom(ServerPlayer player)
    {
        if (player.InRoom == null) return;

        ServerRoom room = player.InRoom;
        room.UserLeave(player);
        player.InRoom = null;

        // If the host just left - dissolve the room.
        if (player == room.Host)
        {
            for (int i = 0; i < room.UsersInside.Count; i++)
            {
                ServerPlayer otherUser = room.UsersInside[i];
                RemovePlayerFromRoom(otherUser);
            }
            room.Dispose();
            _rooms.Remove(room);
        }
    }

    #endregion

    #region Events

    protected virtual void OnUserConnected(ServerPlayer user)
    {

    }

    #endregion

    #region Network Functions

    protected static NetworkFunctionInvoker<ServerBase, ServerPlayer> _netFuncs = new();

    static ServerBase()
    {
        RegisterFunctions(_netFuncs);
    }

    private static void RegisterFunctions(NetworkFunctionInvoker<ServerBase, ServerPlayer> invoker)
    {
        invoker.Register(NetworkMessageType.HostRoom, Msg_HostRoom);
        invoker.Register(NetworkMessageType.GetRoomInfo, Msg_GetRoomInfo);
        invoker.Register(NetworkMessageType.GetRooms, Msg_GetRooms);
        invoker.Register<NetworkMessageType, int>(NetworkMessageType.JoinRoom, Msg_JoinRoom);
    }

    private static void Msg_HostRoom(ServerBase self, ServerPlayer sender)
    {
        if (!self.UsersCanManageRooms) return;

        self.RemovePlayerFromRoom(sender);
        self.CreateNewRoom(sender);
    }

    private static void Msg_GetRoomInfo(ServerBase self, ServerPlayer sender)
    {
        if (!self.UsersCanManageRooms) return;

        ServerRoom? room = sender.InRoom;
        if (room == null)
        {
            self.SendMessageToPlayerWithoutData(sender, NetworkMessageType.Error_NotInRoom);
            return;
        }

        ServerRoomInfo roomInfo = room.GetRoomInfo();
        self.SendMessageToPlayer(sender, NetworkMessageType.RoomInfo, roomInfo);
    }

    private unsafe static void Msg_GetRooms(ServerBase self, ServerPlayer sender)
    {
        if (!self.UsersCanManageRooms) return;

        List<ServerRoom> rooms = self._rooms;
        int roomsToSend = Math.Min(rooms.Count, ServerGameInfoList.MAX_ROOMS_IN_LIST);

        ServerGameInfoList roomList = new ServerGameInfoList()
        {
            RoomCount = roomsToSend
        };

        // todo: paging
        for (int i = 0; i < roomsToSend; i++)
        {
            ServerRoomInfo roomInfo = rooms[i].GetRoomInfo();
            roomList.RoomIds[i] = roomInfo.RoomId;
            roomList.HostIds[i] = roomInfo.HostId;
            roomList.PlayerCounts[i] = roomInfo.PlayerCount;
        }

        self.SendMessageToPlayer(sender, NetworkMessageType.RoomList, roomList);
    }

    private static void Msg_JoinRoom(ServerBase self, ServerPlayer sender, int roomId)
    {
        if (!self.UsersCanManageRooms) return;

        ServerRoom? roomFound = null;
        for (int i = 0; i < self._rooms.Count; i++)
        {
            ServerRoom room = self._rooms[i];
            if (room.Id == roomId)
            {
                roomFound = room;
                break;
            }
        }

        if (roomFound == null)
        {
            self.SendMessageToPlayerWithoutData(sender, NetworkMessageType.Error_RoomNotFound);
            return;
        }

        bool success = self.AddPlayerToRoom(sender, roomFound);
        if (!success)
        {
            self.SendMessageToPlayerWithoutData(sender, NetworkMessageType.Error_RoomNotFound);
            return;
        }
    }

    #endregion
}
