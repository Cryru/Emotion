using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Utility;
using System.Buffers.Binary;

#nullable enable

namespace Emotion.Network.ClientSide;

public class TimeSyncClient : Client
{
    public CoroutineManagerGameTime CoroutineManager = Engine.CoroutineManagerGameTime;

    private bool _firstTimeSyncMsg = true;

    public TimeSyncClient()
    {
        RegisterFunction<bool>("AdvanceTime", (va) => { Assert(va); });
    }

    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        if (msg.MessageType == NetworkMessageType.GenericGameplay)
        {
            base.ClientProcessMessage(msg, reader);
            return;
        }

        if (msg.MessageType != NetworkMessageType.GenericGameplayWithTime) return;

        int gameTime = reader.ReadInt32();
        if (_firstTimeSyncMsg)
        {
            CoroutineManager.GameTimeAdvanceLimit = gameTime;
            CoroutineManager.ResetGameTime(gameTime);
            _firstTimeSyncMsg = false;
        }
        else
        {
            CoroutineManager.GameTimeAdvanceLimit = MathF.Max(CoroutineManager.GameTimeAdvanceLimit, gameTime);
        }

        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        int methodNameHash = methodNameBytes.GetStableHashCode();
        if (_functions.TryGetValue(methodNameHash, out NetworkFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);
            func.InvokeAtGameTime(CoroutineManager, gameTime, metaDataBytes);
        }
    }

    public void SendTimeSyncHash(int hash)
    {
        if (CoroutineManager.Current == null)
        {
            Assert(false, "Can't send time sync hashes outside the Engine.CoroutineManagerGameTime runner!");
            return;
        }

        Span<byte> spanData = stackalloc byte[sizeof(byte) + sizeof(int)];
        spanData[0] = (byte)NetworkMessageType.TimeSyncHash;
        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(1), hash);
        SendMessageToServer(spanData);
    }

    [Conditional("DEBUG")]
    public void SendTimeSyncHashDebug(string hashString)
    {
        if (CoroutineManager.Current == null)
        {
            Assert(false, "Can't send time sync hashes outside the Engine.CoroutineManagerGameTime runner!");
            return;
        }

        int bytesWritten = 0;
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        spanData[0] = (byte)NetworkMessageType.TimeSyncHashDebug;

        bytesWritten += sizeof(byte);
        bytesWritten += NetworkMessage.WriteStringToMessage(spanData.Slice(bytesWritten), hashString);
        SendMessageToServer(spanData.Slice(0, bytesWritten));
    }
}
