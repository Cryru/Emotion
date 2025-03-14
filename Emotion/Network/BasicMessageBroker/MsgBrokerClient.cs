﻿using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Utility;
using System.Buffers.Binary;
using System.IO;
using System.Text;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public class MsgBrokerClient : Client
{
    protected Dictionary<int, MsgBrokerFunction> _functions = new Dictionary<int, MsgBrokerFunction>();

    public void RegisterFunction<T>(string name, Action<T> func)
    {
        MsgBrokerFunction<T> funcDef = new MsgBrokerFunction<T>(name, func);
        _functions.Add(name.GetStableHashCodeASCII(), funcDef);
    }

    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        var methodNameHash = methodNameBytes.GetStableHashCode();

        if (_functions.TryGetValue(methodNameHash, out MsgBrokerFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);

            // Engine.Log.Trace($"MsgBroker Invoking {methodName} with {metaData}", LogTag);

            func.Invoke(metaDataBytes);
        }
    }

    public virtual void SendBrokerMsg(string method, string metadata)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];
        int bytesWritten = 0;

        spanData[0] = (byte)NetworkMessageType.Generic;
        bytesWritten += sizeof(byte);

        bytesWritten += WriteStringToMessage(spanData.Slice(bytesWritten), method);
        bytesWritten += WriteStringToMessage(spanData.Slice(bytesWritten), metadata);
        SendMessageToServer(spanData.Slice(0, bytesWritten));
    }
}
