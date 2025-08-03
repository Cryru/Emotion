#nullable enable

using Emotion.Network.Base;

namespace Emotion.Network.ServerSide.Gameplay;

public abstract class ServerRoomGameplay
{
    public Server Server = null!;
    public ServerRoom Room = null!;

    public virtual void BindToServer(Server server, ServerRoom room)
    {
        Server = server;
        Room = room;
        room.ServerGameplay = this;
    }

    public virtual void OnMessageReceived(ServerUser sender, NetworkMessage msg)
    {
        // nop
    }

    public virtual void OnUserJoined(ServerUser user)
    {
        // nop
    }
}