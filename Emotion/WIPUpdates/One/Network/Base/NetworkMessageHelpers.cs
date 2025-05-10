using Emotion.Common.Serialization;
using Emotion.Network.Base;
using Emotion.Standard.XML;
using System.Buffers.Binary;
using System.Text;

#nullable enable

namespace Emotion.WIPUpdates.One.Network.Base;

[DontSerialize]
public unsafe struct NetworkMessageData
{
    public const int MagicNumber = (((byte) 'E') << 24) | (((byte)'M') << 16) | (((byte)'O') << 8) | ((byte)'N');
    public const int MaxMessageSize = 1000 * 10; // 10KB
    public const int SizeWithoutContent = sizeof(byte) * 3 + sizeof(int) + sizeof(int) + sizeof(uint) + sizeof(int);
    public const int MaxContentSize = NetworkMessageData.MaxMessageSize - SizeWithoutContent;
    public const int HashOffset = sizeof(byte) * 3;

    public int Magic; // Should match MagicNumber
    public int Hash;
    public int MessageIndex;
    public uint Type;

    public int ContentLength;
    public fixed byte Content[NetworkMessageData.MaxContentSize]; // Not the whole thing will be copied into the buffer :)
}

[DontSerialize]
public unsafe struct NetworkMessageTimeContentHeader
{
    public const int SizeWithoutMethodData = sizeof(int) * 2 + 32;

    public int GameTime;
    public int MethodNameLength;
    public fixed byte MethodName[32];
    public fixed byte MethodData[NetworkMessageData.MaxMessageSize - NetworkMessageData.SizeWithoutContent - SizeWithoutMethodData];
}

public static class NetworkMessageHelpers
{
    public static NetworkMessageData CreateMessage(uint type)
    {
        var msg = new NetworkMessageData();
        msg.Type = type;
        return msg;
    }

    public static NetworkMessageData CreateMessage<T>(uint type, string method, T data) where T : struct
    {
        return new NetworkMessageData();
    }

    public static NetworkMessageData CreateMessage<T>(string method, T data) where T : struct
    {
        return CreateMessage((uint) NetworkMessageType.GenericGameplay, method, data);
    }

    public unsafe static NetworkMessageData CreateMessage<T>(uint type, T data) where T : struct
    {
        var msg = new NetworkMessageData();
        msg.Type = type;

        int bytesWritten = 0;
        var spanData = new Span<byte>(msg.Content, NetworkMessageData.MaxContentSize);

        var xmlContent = XMLFormat.To(data);
        int xmlByteCount = Encoding.ASCII.GetByteCount(xmlContent);

        BinaryPrimitives.WriteInt32LittleEndian(spanData, xmlByteCount);
        bytesWritten += sizeof(int);

        int methodNameBytesWritten = Encoding.ASCII.GetBytes(xmlContent, spanData.Slice(bytesWritten));
        bytesWritten += methodNameBytesWritten;

        msg.ContentLength = bytesWritten;

        return msg;
    }

    #region Time Sync

    public static NetworkMessageData CreateMessageWithTime(NetworkMessageType type, int time)
    {
        return new NetworkMessageData();
    }

    public static NetworkMessageData CreateMessageWithTime<T>(NetworkMessageType type, int time, string method, T data) where T : struct
    {
        return new NetworkMessageData();
    }

    public static NetworkMessageData CreateMessageWithTime<T>(int time, string method, T data) where T : struct
    {
        return CreateMessageWithTime(NetworkMessageType.GenericGameplayWithTime, time, method, data);
    }

    #endregion
}