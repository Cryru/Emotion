using Emotion.Game.Time.Routines;
using Emotion.Network.Base;
using Emotion.Network.BasicMessageBroker;
using Emotion.Network.ClientSide;
using Emotion.Utility;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.IO;
using System.Text;

#nullable enable

namespace Emotion.Network.TimeSyncMessageBroker;

public class MsgBrokerClientTimeSync : MsgBrokerClient
{
    private bool _firstTimeSyncMsg = true;

    public MsgBrokerClientTimeSync()
    {
        RegisterFunction<bool>("AdvanceTime", (va) => { Assert(va); });
    }

    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        int gameTime = reader.ReadInt32();
        if (_firstTimeSyncMsg)
        {
            GameTime.GameTimeAdvanceLimit = gameTime;
            GameTime.ResetGameTime(gameTime);
            _firstTimeSyncMsg = false;
        }
        else
        {
            GameTime.GameTimeAdvanceLimit = MathF.Max(GameTime.GameTimeAdvanceLimit, gameTime);
        }

        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        int methodNameHash = methodNameBytes.GetStableHashCode();
        if (_functions.TryGetValue(methodNameHash, out MsgBrokerFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);
            func.InvokeAtGameTime(gameTime, metaDataBytes);
        }
    }

    public void SendTimeSyncHash(int hash)
    {
        if (Engine.CoroutineManagerGameTime.Current == null)
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
        if (Engine.CoroutineManagerGameTime.Current == null)
        {
            Assert(false, "Can't send time sync hashes outside the Engine.CoroutineManagerGameTime runner!");
            return;
        }

        int bytesWritten = 0;
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        spanData[0] = (byte)NetworkMessageType.TimeSyncHashDebug;

        bytesWritten += sizeof(byte);
        bytesWritten += WriteStringToMessage(spanData.Slice(bytesWritten), hashString);
        SendMessageToServer(spanData.Slice(0, bytesWritten));
    }
}
