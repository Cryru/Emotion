#nullable enable

using Emotion.Network.ServerSide;

namespace Emotion.Network.TimeSyncMessageBroker;

public class AuthoritativeServerTimeSync : MsgBrokerServerTimeSync
{
    protected T MakeServerManagedRoom<T>() where T : IServerRoomGameplay, new()
    {
        ServerRoom newRoom = ServerRoom.Shared.Get();
        newRoom.Id = ActiveRooms.Count + 1;
        newRoom.Host = null;
        newRoom.Active = true;

        T gameplayRoom = new T();
        gameplayRoom.BindToServer(this, newRoom);
        newRoom.ServerGameplay = gameplayRoom;

        ActiveRooms.Add(newRoom);

        Engine.Log.Info($"New room created: {newRoom.Id}", LogTag);
        RoomCreated(newRoom);

        return gameplayRoom;
    }
}