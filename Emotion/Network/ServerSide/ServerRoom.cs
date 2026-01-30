#nullable enable

using Emotion.Network.Base;
using Emotion.Network.New.Base;
using System.Runtime.InteropServices;
using NetworkMessage = Emotion.Network.Base.NetworkMessage;

namespace Emotion.Network.ServerSide;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ServerRoomInfo
{
    // Rooms with a lot of players probably shouldn't send out all player ids since then its more of a "mmo" type of deal
    public const int MAX_PLAYERS_IN_ROOM = 10;// (NetworkMessage.MaxContentSize - sizeof(uint) - sizeof(int) - sizeof(int)) / sizeof(int);

    public uint RoomId;
    public int HostId;
    public int PlayerCount;
    public fixed int PlayerIds[MAX_PLAYERS_IN_ROOM];
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct ServerGameInfoList
{
    public const int MAX_ROOMS_IN_LIST = 10;

    public int RoomCount;

    public fixed uint RoomIds[10];
    public fixed int HostIds[10];
    public fixed int PlayerCounts[10];
}

[DontSerialize]
public class ServerRoom
{
    public ServerBase Server { get; init; }
    public ServerPlayer? Host { get; init; } // If null the room is hosted by the server.
    public uint Id { get; init; }

    public bool Disposed = false;
    public List<ServerPlayer> UsersInside = new List<ServerPlayer>();

    public ServerRoom(ServerBase server, ServerPlayer? host, uint roomId)
    {
        Server = server;
        Host = host;
        Id = roomId;
    }

    public virtual void Dispose()
    {
        Disposed = true;
    }

    #region MemberShip

    public virtual bool CanBeJoined(ServerPlayer user)
    {
        return !Disposed;
    }

    public void UserJoin(ServerPlayer user)  
    {
        UsersInside.Add(user);

        ServerRoomInfo roomInfo = GetRoomInfo();
        Server.SendMessageToPlayer(user, NetworkMessageType.RoomJoined, roomInfo);
        SendMessageToAll(NetworkMessageType.UserJoinedRoom, roomInfo, user);

        Engine.Log.Info($"User {user.Id} joined room {Id}", Server!.LogTag);
        OnUserJoined(user);
    }

    public void UserLeave(ServerPlayer user)
    {
        UsersInside.Remove(user);

        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerPlayer otherUser = UsersInside[i];
            NotifyUserLeft(otherUser, user);
        }

        Engine.Log.Info($"User {user.Id} left room {Id}", Server!.LogTag);
        OnUserLeft(user);
    }

    protected void NotifyUserLeft(ServerPlayer userToNotify, ServerPlayer userWhoLeft)
    {

    }

    public virtual unsafe ServerRoomInfo GetRoomInfo()
    {
        ServerRoomInfo info = new ServerRoomInfo()
        {
            RoomId = Id,
            HostId = Host?.Id ?? -1,
            PlayerCount = UsersInside.Count
        };

        int usersToReport = Math.Min(UsersInside.Count, ServerRoomInfo.MAX_PLAYERS_IN_ROOM);
        for (int i = 0; i < usersToReport; i++)
        {
            info.PlayerIds[i] = UsersInside[i].Id;
        }

        return info;
    }

    #endregion

    #region API

    protected virtual void OnUserJoined(ServerPlayer newUser)
    {

    }

    protected virtual void OnUserLeft(ServerPlayer oldUser)
    {

    }

    #endregion

    public void SendMessageToAll<TData>(TData data, ServerPlayer? except = null)
        where TData : unmanaged, INetworkMessageStruct
    {
        uint messageType = TData.MessageType;
        SendMessageToAll(messageType, data, except);
    }

    public void SendMessageToAll<TEnum, TData>(TEnum messageType, TData data, ServerPlayer? except = null)
        where TEnum : unmanaged
        where TData : unmanaged
    {
        NetworkMessage msg = NetworkAgentBase.CreateMessage(messageType, data);
        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerPlayer user = UsersInside[i];
            if (user == except) continue;
            Server.SendMessageToPlayerRaw(user, in msg);
        }
    }

    public void SendMessageToAllWithoutData<TEnum>(TEnum messageType, ServerPlayer? except = null)
        where TEnum : unmanaged
    {
        NetworkMessage msg = NetworkAgentBase.CreateMessageWithoutData(messageType);
        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerPlayer user = UsersInside[i];
            if (user == except) continue;
            Server.SendMessageToPlayerRaw(user, in msg);
        }
    }

    #region Network Functions

    public static ServerNetworkFunctionInvoker<ServerRoom, ServerPlayer> NetworkFunctions { get; } = new();

    public bool ProcessMessage(ServerBase server, ServerPlayer sender, in NetworkMessage msg)
    {
        uint messageType = msg.Type;
        NetworkFunctionBase<ServerRoom, ServerPlayer>? netFunc = NetworkFunctions.GetFunctionFromMessageType(messageType);
        if (netFunc != null)
        {
            if (!netFunc.TryInvoke(this, sender, msg))
            {
                Engine.Log.Warning($"Error executing function of message type {messageType}", nameof(ServerRoom), true);
            }
        }
        else
        {
            Engine.Log.Warning($"Unknown message type {messageType}", nameof(ServerRoom), true);
        }

        return true;
    }

    #endregion
}
