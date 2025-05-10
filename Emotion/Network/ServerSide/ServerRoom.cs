using Emotion.Common.Serialization;
using Emotion.Network.Base;
using Emotion.Network.ServerSide.Gameplay;
using Emotion.Utility;

#nullable enable

namespace Emotion.Network.ServerSide;

[DontSerialize]
public class ServerRoom
{
    public static ObjectPool<ServerRoom> Shared = new ObjectPool<ServerRoom>((r) => r.Reset(), 10);

    public bool Active = false;
    public int Id = -1;
    public ServerUser? Host;
    public List<ServerUser> UsersInside = new List<ServerUser>();

    public ServerRoomGameplay? ServerGameplay;

    public void Reset()
    {
        Id = -1;
        UsersInside.Clear();
        Host = null;
        ServerGameplay = null;
        Active = true;
    }

    public void UserJoin(Server server, ServerUser user)
    {
        user.InRoom = this;
        UsersInside.Add(user);
        server.SendMessageToUser(user, NetworkMessageType.RoomJoined, GetRoomInfo());

        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerUser otherUser = UsersInside[i];
            if (otherUser != user)
                server.SendMessageToUser(otherUser, NetworkMessageType.UserJoinedRoom, GetRoomInfo());
        }

        Engine.Log.Info($"User {user.Id} joined room {Id}", server.LogTag);
        ServerGameplay?.OnUserJoined(user);
    }

    public void UserLeave(Server server, ServerUser user)
    {
        user.InRoom = null;
        UsersInside.Remove(user);

        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerUser otherUser = UsersInside[i];
            NotifyUserLeft(server, otherUser, user);
        }

        if (user == Host)
        {
            Active = false;
            for (int i = 0; i < UsersInside.Count; i++)
            {
                ServerUser otherUser = UsersInside[i];
                otherUser.InRoom = null;
                NotifyUserKicked(server, otherUser);
            }
            Reset();
        }

        Engine.Log.Info($"User {user.Id} left room {Id}", server.LogTag);
    }

    protected void NotifyUserLeft(Server server, ServerUser userToNotify, ServerUser userWhoLeft)
    {

    }

    protected void NotifyUserKicked(Server server, ServerUser userToNotify)
    {

    }

    public ServerRoomInfo GetRoomInfo()
    {
        int[] otherUserIds = new int[UsersInside.Count];
        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerUser user = UsersInside[i];
            otherUserIds[i] = user.Id;
        }

        ServerRoomInfo info = new ServerRoomInfo()
        {
            Id = Id,
            HostUser = Host == null ? -1: Host.Id,
            UsersInside = otherUserIds
        };
        return info;
    }
}
