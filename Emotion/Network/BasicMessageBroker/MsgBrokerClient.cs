using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Utility;
using System.Buffers.Binary;
using System.IO;
using System.Text;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public class MsgBrokerClient : Client
{
    protected Dictionary<string, MsgBrokerFunction> _functions = new Dictionary<string, MsgBrokerFunction>();

    public void RegisterFunction<T>(string name, Action<T> func)
    {
        MsgBrokerFunction<T> funcDef = new MsgBrokerFunction<T>(name, func);
        _functions.Add(name, funcDef);
    }

    protected override void ClientProcessMessage(NetworkMessage msg, ByteReader reader)
    {
        int methodNameLength = reader.ReadInt32();
        var methodNameBytes = reader.ReadBytes(methodNameLength);
        var methodName = Encoding.ASCII.GetString(methodNameBytes);

        if (_functions.TryGetValue(methodName, out MsgBrokerFunction? func))
        {
            var metaDataLength = reader.ReadInt32();
            var metaDataBytes = reader.ReadBytes(metaDataLength);
            var metaData = Encoding.UTF8.GetString(metaDataBytes);

            // Engine.Log.Trace($"MsgBroker Invoking {methodName} with {metaData}", LogTag);

            func.Invoke(metaData);
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
