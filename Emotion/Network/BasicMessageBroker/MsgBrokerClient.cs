using Emotion.Network.Base;
using Emotion.Network.ClientSide;
using Emotion.Utility;
using System.IO;
using System.Text;

#nullable enable

namespace Emotion.Network.BasicMessageBroker;

public class MsgBrokerClient : Client
{
    private Dictionary<string, MsgBrokerFunction> _functions = new Dictionary<string, MsgBrokerFunction>();

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

            Engine.Log.Trace($"MsgBroker Invoking {methodName} with {metaData}", LogTag);

            func.Invoke(metaData);
        }
    }

    public void SendBrokerMsg(string method, string metadata)
    {
        Span<byte> spanData = stackalloc byte[NetworkMessage.MaxMessageContent];

        using MemoryStream str = new MemoryStream();
        using BinaryWriter wri = new BinaryWriter(str);

        wri.Write((byte)NetworkMessageType.Generic);

        var methodNameBytes = Encoding.ASCII.GetBytes(method);
        wri.Write(methodNameBytes.Length);
        wri.Write(methodNameBytes);

        var methodMetadataBytes = Encoding.UTF8.GetBytes(metadata);
        wri.Write(methodMetadataBytes.Length);
        wri.Write(methodMetadataBytes);

        wri.Flush();
        var arr = str.ToArray();
        SendMessageToServer(arr);
    }
}
