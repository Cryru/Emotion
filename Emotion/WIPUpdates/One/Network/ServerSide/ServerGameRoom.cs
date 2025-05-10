using Emotion.Common.Serialization;
using Emotion.Network.Base;
using Emotion.WIPUpdates.One.Network.Base;

#nullable enable

namespace Emotion.WIPUpdates.One.Network.ServerSide;

[DontSerialize]
public class ServerGameRoom
{
    public ServerBase? Server;

    public bool Active = false;
    public uint Id = 0;
    public ServerPlayer? Host; // If null the room is hosted by the server.
    public List<ServerPlayer> UsersInside = new List<ServerPlayer>();

    public virtual void Activate()
    {
        if (Server == null) return;
        Active = true;
    }

    public virtual void Deactivate()
    {
        Active = false;
    }

    #region MembersHip

    public void UserJoin(ServerPlayer user)  
    {
        UsersInside.Add(user);

        var roomInfo = GetRoomInfo();

        var joinedMsg = NetworkMessageHelpers.CreateMessage((uint)NetworkMessageType.RoomJoined, roomInfo);
        Server!.SendMessageToUser(user, joinedMsg);

        var joinMsg = NetworkMessageHelpers.CreateMessage((uint)NetworkMessageType.UserJoinedRoom, roomInfo);
        SendMessageToAllUsers(joinMsg, user);

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

    public int GetRoomInfo()
    {
        return 1;
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

    public void SendMessageToAllUsers(NetworkMessageData msg, ServerPlayer? except = null)
    {
        AssertNotNull(Server);

        for (int i = 0; i < UsersInside.Count; i++)
        {
            ServerPlayer user = UsersInside[i];
            if (user == except) continue;
            Server.SendMessageToUser(user, msg);
        }
    }
}