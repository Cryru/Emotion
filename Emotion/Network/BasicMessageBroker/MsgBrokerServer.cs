using Emotion.Network.Base;
using Emotion.Network.ServerSide;
using Emotion.Utility;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public class MsgBrokerServer : Server
{
    protected override void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        ServerRoom? room = sender.InRoom;
        if (room == null) return;

        for (int i = 0; i < room.UsersInside.Count; i++)
        {
            ServerUser userInRoom = room.UsersInside[i];
            if (userInRoom == sender) continue;
            userInRoom.SendMessage(this, msg.Content.Span);
        }
    }
}
