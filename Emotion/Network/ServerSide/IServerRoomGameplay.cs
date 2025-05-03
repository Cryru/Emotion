namespace Emotion.Network.ServerSide;

public interface IServerRoomGameplay
{
    public void BindToServer(Server server, ServerRoom room);

    public void UserJoined(ServerUser user);
}