#nullable enable

using Emotion;
using System.Net;

namespace Emotion.Network.Base;

[DontSerialize]
public struct MessagePair
{
    public IPEndPoint Recipient;
    public NetworkMessage Message;
}