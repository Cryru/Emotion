using Emotion.Common.Serialization;
using Emotion.Utility;
using System.Net;

#nullable enable

namespace Emotion.Network.ServerSide;

[DontSerialize]
public class ServerUser
{
    public static ObjectPool<ServerUser> Shared = new ObjectPool<ServerUser>((r) => r.Reset(), 50);

    public IPEndPoint? UserIp;
    public int SendMessageIndex = 1;
    public int ReceiveMessageIndex = 1;
    public int Id = -1;

    public ServerRoom? InRoom;

    public void Reset()
    {
        UserIp = null;
        SendMessageIndex = 1;
        ReceiveMessageIndex = 1;
        Id = -1;
        InRoom = null;
    }

    public override string ToString()
    {
        return $"User {Id} - {UserIp}";
    }
}
