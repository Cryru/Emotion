#nullable enable

using Emotion;
using System.Net;

namespace Emotion.Network.ServerSide;

[DontSerialize]
public class ServerPlayer
{
    public IPEndPoint? UserIp;
    public int SendMessageIndex = 1;
    public int ReceiveMessageIndex = 0;
    public int Id = -1;

    public ServerRoom? InRoom;

    public override string ToString()
    {
        return $"User {Id} - {UserIp}";
    }
}