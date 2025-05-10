#nullable enable

using Emotion;
using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using Emotion.Network.ServerSide.Gameplay;
using Emotion.Utility;

namespace Emotion.Network.ServerAuthoritative;

public class AuthoritativeServer : Server
{
    public AuthoritativeServer()
    {
        UsersCanManageRooms = false;
    }

    protected override void OnUserConnected(ServerUser user)
    {
        // nop
        // implement room logic here since users cant create rooms
    }

    protected override void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        ServerRoom? room = sender.InRoom;
        if (room == null || room.ServerGameplay == null) return;
        room.ServerGameplay.OnMessageReceived(sender, msg);
    }

    protected ServerRoom MakeServerManagedRoom<T>() where T : ServerTimeSyncRoomGameplay, new()
    {
        ServerRoom newRoom = ServerRoom.Shared.Get();
        newRoom.Id = ActiveRooms.Count + 1;
        newRoom.Host = null;
        newRoom.Active = true;

        T gameplayRoom = new T();
        gameplayRoom.BindToServer(this, newRoom);

        ActiveRooms.Add(newRoom);

        Engine.Log.Info($"New room created: {newRoom.Id}", LogTag);
        OnRoomCreated(newRoom);

        return newRoom;
    }
}