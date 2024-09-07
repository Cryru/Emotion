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
    public CoroutineManager GameTimeRunner = new CoroutineManager(false);
    public int CurrentGameTime = 0;

    private int _advanceGameTimeTo = 0;

    public MsgBrokerClientTimeSync()
    {
        RegisterFunction<bool>("AdvanceTime", (va) => { Assert(va); });
    }

    protected override void UpdateInternal()
    {
        base.UpdateInternal();

        if (_advanceGameTimeTo != CurrentGameTime)
        {
            int diff = _advanceGameTimeTo - CurrentGameTime;
            GameTimeRunner.Update(diff);
            CurrentGameTime = _advanceGameTimeTo;
        }
    }

    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        int gameTime = reader.ReadInt32();
        if (gameTime > _advanceGameTimeTo)
            _advanceGameTimeTo = gameTime;

        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        var methodName = Encoding.ASCII.GetString(methodNameBytes);

        if (_functions.TryGetValue(methodName, out MsgBrokerFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);
            var metaData = Encoding.UTF8.GetString(metaDataBytes);

            // Engine.Log.Trace($"MsgBroker Invoking {methodName} with {metaData}", LogTag);
            GameTimeRunner.StartCoroutine(RunMessageFunctionAtTime(gameTime, func, metaData));
        }
    }

    protected IEnumerator RunMessageFunctionAtTime(int time, MsgBrokerFunction brokerFunc, string metaData)
    {
        int timeDiff = time - CurrentGameTime;
        yield return timeDiff;
        CurrentGameTime = time;

        brokerFunc.Invoke(metaData);
    }

    public void SendTimeSyncHash(int hash)
    {
        if (GameTimeRunner.Current == null)
        {
            Assert(false, "Can't send time synced messages outside the GameTimeRunner!");
            return;
        }

        Span<byte> spanData = stackalloc byte[sizeof(byte) + sizeof(int)];
        spanData[0] = (byte)NetworkMessageType.TimeSyncHash;
        BinaryPrimitives.WriteInt32LittleEndian(spanData.Slice(1), hash);
        SendMessageToServer(spanData);
    }

    public override void SendBrokerMsg(string method, string metadata)
    {
        if (GameTimeRunner.Current == null)
        {
            Assert(false, "Can't send time synced messages outside the GameTimeRunner!");
            return;
        }

        base.SendBrokerMsg(method, metadata);
    }
}
