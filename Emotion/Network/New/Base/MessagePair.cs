#nullable enable

using System.Net;

namespace Emotion.Network.New.Base;

[DontSerialize]
public struct MessagePair
{
    public IPEndPoint Recipient;
    public NetworkMessageData Message;
}