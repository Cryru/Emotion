using Emotion.Common.Serialization;
using System.Net;

#nullable enable

namespace Emotion.WIPUpdates.One.Network.ServerSide;

[DontSerialize]
public abstract class ServerPlayer
{
    public IPEndPoint? UserIp;
    public int SendMessageIndex = 1;
    public int ReceiveMessageIndex = 1;
    public int Id = -1;
}

[DontSerialize]
public class ServerPlayer<TRoomType> : ServerPlayer
    where TRoomType : ServerGameRoom, new()
{
    public TRoomType? InRoom;

    public override string ToString()
    {
        return $"User {Id} - {UserIp}";
    }
}
