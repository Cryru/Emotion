using Emotion.Network.New.ServerSide;

namespace Emotion.Network.New.Authoritative;

public class AuthoritativeServer<TRoomType> : ServerBase<TRoomType>
    where TRoomType : ServerGameRoom, new()
{
    public AuthoritativeServer(int hostingPort) : base(hostingPort)
    {
    }
}
