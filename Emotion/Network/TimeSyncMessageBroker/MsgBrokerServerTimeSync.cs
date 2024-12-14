using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ServerSide;
using Emotion.Standard.XML;
using Emotion.Utility;
using System.Buffers.Binary;

#nullable enable

namespace Emotion.Network.TimeSyncMessageBroker;

public class MsgBrokerServerTimeSync : MsgBrokerServer
{
    public CoroutineManager CoroutineManager = Engine.CoroutineManager;

    protected override void ServerProcessMessage(ServerUser sender, NetworkMessage msg, ByteReader reader)
    {
        ServerRoom? room = sender.InRoom;
        if (room == null) return;
        if (room.ServerData is not TimeSyncedServerRoom timeSyncRoom) return;

        msg.AutoFree = false;
        timeSyncRoom.AddMessageForNextTick(msg);
    }

    protected override void RoomCreated(ServerRoom room)
    {
        TimeSyncedServerRoom timeSyncRoomData = new TimeSyncedServerRoom(this, room);
        room.ServerData = timeSyncRoomData;

        timeSyncRoomData.StartGameTime(CoroutineManager);
        base.RoomCreated(room);
    }

    public void BroadcastAdvanceTimeMessage(List<ServerUser> users, int time)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        spanData[0] = (byte)NetworkMessageType.Generic;
        bytesWritten += sizeof(byte);

        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(bytesWritten), time);
        bytesWritten += sizeof(int);

        bytesWritten += WriteStringToMessage(spanData.Slice(bytesWritten), "AdvanceTime");
        bytesWritten += WriteStringToMessage(spanData.Slice(bytesWritten), XMLFormat.To(true));

        Span<byte> sendData = spanData.Slice(0, bytesWritten);

        // Send the message to all the users (this reuses the span!)
        for (int i = 0; i < users.Count; i++)
        {
            ServerUser user = users[i];
            SendMessage(user, sendData);
        }
    }

    public void BroadcastMessageWithTime(List<ServerUser> users, int time, NetworkMessage msg)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        spanData[0] = (byte)NetworkMessageType.Generic;
        bytesWritten += sizeof(byte);

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
            SendMessage(user, sendData);
        }
    }
}