using Emotion.Common.Serialization;
using System.Net;

namespace Emotion.Networking.Base;

[DontSerialize]
public struct MessagePair
{
    public IPEndPoint Recipient;
    public NetworkMessageData Message;
}