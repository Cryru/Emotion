#nullable enable

using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ServerSide;
using System.Buffers.Binary;

namespace Emotion.Network.TimeSyncMessageBroker;

public class MsgBrokerTimeSyncServer : MsgBrokerServer
{
    protected override void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        ServerRoom? room = sender.InRoom;
        if (room == null || room.ServerGameplay == null) return;
        room.ServerGameplay.OnMessageReceived(sender, msg);
    }

    protected override void OnRoomCreated(ServerRoom room)
    {
        var timeSyncRoomData = new MsgBrokerTimeSyncGameplay();
        timeSyncRoomData.BindToServer(this, room);

        base.OnRoomCreated(room);
    }

    public void ResendMessageWithTime(List<ServerUser> users, int time, NetworkMessage msg)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        // Message type
        spanData[0] = (byte)NetworkMessageType.GenericGameplayWithTime;
        bytesWritten += sizeof(byte);

        // Time
        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(bytesWritten), time);
        bytesWritten += sizeof(int);

        // Copy generic broker message to all users.
        ByteReader reader = msg.GetContentReader()!;
        reader.ReadByte(); // Type
        ReadOnlySpan<byte> data = reader.ReadBytes(msg.Content.Length - 1);
        // Message received is so long that adding the time code went over the max message content - wtf
        if (data.Length > spanData.Length - bytesWritten)
        {
            Assert(false);
            return;
        }
        data.CopyTo(spanData.Slice(bytesWritten));
        bytesWritten += data.Length;

        Span<byte> sendData = spanData.Slice(0, bytesWritten);

        // Send the message to all the users (this reuses the span!)
        for (int i = 0; i < users.Count; i++)
        {
            ServerUser user = users[i];
            SendMessageRawToUser(user, sendData);
        }
    }
}