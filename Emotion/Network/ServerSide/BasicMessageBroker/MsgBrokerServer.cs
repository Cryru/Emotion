using Emotion.Network.Base;
using Emotion.Utility;

#nullable enable

namespace Emotion.Network.ServerSide.BasicMessageBroker;

public class MsgBrokerServer : Server
{
    protected override void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        for (int i = 0; i < ConnectedUsers.Count; i++)
        {
            var user = ConnectedUsers[i];
            if (user == sender) continue;
            user.SendMessage(this, msg.Content.Span);
        }
    }
}
