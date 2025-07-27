using Emotion.Networking.ServerSide;

namespace Emotion.Networking.Authoritative;

public class AuthoritativeServer<TRoomType> : ServerBase<TRoomType>
    where TRoomType : ServerGameRoom, new()
{
    public AuthoritativeServer(int hostingPort) : base(hostingPort)
    {
    }
}
