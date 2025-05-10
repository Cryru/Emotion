using Emotion.WIPUpdates.One.Network.ServerSide;

namespace Emotion.WIPUpdates.One.Network.Authoritative;

public class AuthoritativeServer<TRoomType> : ServerBase<TRoomType>
    where TRoomType : ServerGameRoom, new()
{
    public AuthoritativeServer(int hostingPort) : base(hostingPort)
    {
    }
}
