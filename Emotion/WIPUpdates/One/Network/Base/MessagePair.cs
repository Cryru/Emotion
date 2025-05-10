using Emotion.Common.Serialization;
using System.Net;

namespace Emotion.WIPUpdates.One.Network.Base;

[DontSerialize]
public struct MessagePair
{
    public IPEndPoint Recipient;
    public NetworkMessageData Message;
}