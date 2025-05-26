using Emotion.Game.Time.Routines;
using Emotion.Network.ServerSide;
using Emotion.WIPUpdates.One.Network.Base;
using System.Collections.Concurrent;
using System.Net;

namespace Emotion.WIPUpdates.One.Network.ServerSide;

#nullable enable

public abstract class ServerBase : NetworkAgentBase
{
    protected ServerBase(int hostingPort, CoroutineManager coroutineManager) : base(hostingPort, coroutineManager)
    {
    }

    public abstract unsafe void SendMessageToUser(ServerPlayer user, NetworkMessageData msg);
}

public abstract class ServerBase<TRoomType> : ServerBase
    where TRoomType : ServerGameRoom, new()
{
    public bool UsersCanManageRooms { get; init; } = true;

    public List<ServerPlayer<TRoomType>> ConnectedUsers = new();

    public ConcurrentDictionary<IPEndPoint, ServerPlayer<TRoomType>> IPToUser = new();

    protected ServerBase(int hostingPort) : base(hostingPort, Engine.CoroutineManager)
    {
    }

    protected void RegisterFunction<TMsg>(uint typ, Action<ServerPlayer, TMsg> func) where TMsg : struct
    {
        if (GetFunctionFromMessageType(typ) != null) return;
        var netFunc = new NetworkFunction<TMsg>(typ, func);
        _functions.Add(typ, netFunc);
    }

    protected void RegisterRoomFunction<TMsg>(uint typ, Action<TRoomType, ServerPlayer, TMsg> func) where TMsg : struct
    {
        if (GetFunctionFromMessageType(typ) != null) return;

        Action<ServerPlayer, TMsg> wrapperFunc = (user, msg) =>
        {
            var userAsProperType = user as ServerPlayer<TRoomType>;
            if (userAsProperType == null)
            {
                Assert(false, "Player is of another type? huh?");
                return;
            }
            if (userAsProperType.InRoom == null) return;

            func(userAsProperType.InRoom, user, msg);
        };
        var netFunc = new NetworkFunction<TMsg>(typ, wrapperFunc);
        _functions.Add(typ, netFunc);
    }

    protected override void ProcessMessage(IPEndPoint from, NetworkMessageData msg)
    {
        IPToUser.TryGetValue(from, out ServerPlayer<TRoomType>? user);

        uint messageType = msg.Type;
        switch (messageType)
        {
            case (uint)Emotion.Network.Base.NetworkMessageType.RequestConnect:
                if (user == null)
                    Msg_RequestConnect(from, msg);
                else
                    SendMessageToIP(from, NetworkMessageHelpers.CreateMessage((uint)Emotion.Network.Base.NetworkMessageType.Error_AlreadyConnected));
                break;
        }

        if (user == null)
        {
            Engine.Log.Trace($"Message from unknown sender - {from}", LogTag);
            return;
        }

        if (user.ReceiveMessageIndex > msg.MessageIndex)
        {
            Engine.Log.Trace($"Received old message from {user.Id}. Discarding!", LogTag);
            return;
        }
        user.ReceiveMessageIndex = msg.MessageIndex;

        CallNetFunc(user, msg);
    }

    public override unsafe void SendMessageToUser(ServerPlayer user, NetworkMessageData msg)
    {
        if (user.UserIp == null) return;

        SendMessageToIP(user.UserIp, msg, user.SendMessageIndex);
        user.SendMessageIndex++;
    }

    #region Handlers

    private void Msg_RequestConnect(IPEndPoint ip, NetworkMessageData msg)
    {
        // todo: connection challenge and encryption establish

        Engine.Log.Info($"User connected - {ip}", LogTag);

        var user = new ServerPlayer<TRoomType>();
        user.UserIp = ip;
        user.Id = ConnectedUsers.Count + 1;
        IPToUser.TryAdd(user.UserIp, user);
        ConnectedUsers.Add(user);

        var connectMessage = NetworkMessageHelpers.CreateMessage((uint)Emotion.Network.Base.NetworkMessageType.Connected, user.Id);
        SendMessageToUser(user, connectMessage);
        OnUserConnected(user);
    }

    #endregion

    #region Rooms

    public List<TRoomType> CurrentRooms = new List<TRoomType>();

    private static uint _nextFreeRoomId = 1;

    public TRoomType MakeNewRoom()
    {
        TRoomType newRoom = new TRoomType
        {
            Server = this,
            Id = _nextFreeRoomId
        };
        _nextFreeRoomId++;
        CurrentRooms.Add(newRoom);
        Engine.Log.Info($"New room created: {newRoom.Id}", LogTag);

        return newRoom;
    }

    public void AddPlayerToRoom(ServerPlayer<TRoomType> user, TRoomType newRoom)
    {
        if (!newRoom.Active)
            return;

        RemovePlayerFromRoom(user); // Remove from old room
        newRoom.UserJoin(user);
        user.InRoom = newRoom;
    }

    public void RemovePlayerFromRoom(ServerPlayer<TRoomType> user)
    {
        if (user.InRoom == null) return;

        TRoomType room = user.InRoom;
        room.UserLeave(user);
        user.InRoom = null;

        // If the host just left - dissolve the room.
        if (user == room.Host)
        {
            for (int i = 0; i < room.UsersInside.Count; i++)
            {
                ServerPlayer otherUser = room.UsersInside[i];
                RemovePlayerFromRoom((ServerPlayer<TRoomType>)otherUser);
            }
            room.Deactivate();
            CurrentRooms.Remove(room);
        }
    }

    #endregion

    #region API

    protected virtual void OnUserConnected(ServerPlayer<TRoomType> user)
    {

    }

    #endregion
}
