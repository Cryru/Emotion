using Emotion.Network.Base;
using Emotion.Utility;
using System.Text;

#nullable enable

namespace Emotion.Network.ClientSide.BasicMessageBroker;

public class MsgBrokerClient : Client
{
    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        string messageTypeString = Encoding.ASCII.GetString(msg.Content.Span);
        Engine.Log.Trace($"Message {messageTypeString}", LogTag);
    }
}
