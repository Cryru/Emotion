using Emotion.Utility;
using System.IO;
using System.Net;
using System.Security.Cryptography;

#nullable enable

namespace Emotion.Network.Base;

public class NetworkMessage
{
    // [3] - EMO
    // [4] - Reserved
    // [4] - message index
    // [4] - int32 content length
    // [...] - content
    // [32] - hash

    public IPEndPoint? Sender;
    public bool Valid;
    public int Index;
    public ReadOnlyMemory<byte> Content = ReadOnlyMemory<byte>.Empty;

    public const int MaxMessageSize = 1000 * 10; // 10KB
    public const int MaxMessageContent = MaxMessageSize - 3 - 4 - 4 - 4 - 32;
    public const int MessageContentOffset = 3 + 4 + 4 + 4;

    public static ObjectPool<NetworkMessage> Shared = new ObjectPool<NetworkMessage>((r) => r.Reset(), 100);

    private byte[] _memory = new byte[MaxMessageSize];
    private ByteReader _reader;

    public NetworkMessage()
    {
        _reader = new ByteReader(_memory);
    }

    public void CopyToLocalBuffer(IPEndPoint sender, byte[] buffer)
    {
        Sender = sender;
        Assert(buffer.Length == _memory.Length);
        buffer.CopyTo(_memory, 0);
    }

    public void Process()
    {
        _reader.Seek(0, SeekOrigin.Begin);
        var magicNumber = _reader.ReadBytes(3);
        bool magicMatches = magicNumber[0] == 'E' && magicNumber[1] == 'M' && magicNumber[2] == 'O';
        if (!magicMatches)
        {
            Valid = false;
            return;
        }

        _reader.ReadInt(); // Reserved

        Index = _reader.ReadInt();
        if (Index <= 0)
        {
            Valid = false;
            Index = -1;
            return;
        }

        int contentLength = _reader.ReadInt();
        if (contentLength == 0 || contentLength > MaxMessageContent)
        {
            Valid = false;
            return;
        }

        ReadOnlySpan<byte> msg = _reader.ReadBytes(contentLength);

        Span<byte> contentHash = stackalloc byte[32];
        if (!MD5.TryHashData(msg, contentHash, out int bytesWritten))
        {
            Valid = false;
            return;
        }

        ReadOnlySpan<byte> hash = _reader.ReadBytes(32);
        if (!hash.SequenceEqual(contentHash))
        {
            Valid = false;
            return;
        }

        Valid = true;
        Content = new Memory<byte>(_memory, MessageContentOffset, contentLength);
    }

    public static int EncodeMessage(ReadOnlySpan<byte> data, byte[] dst, int msgIndex)
    {
        using MemoryStream str = new MemoryStream(dst);
        using BinaryWriter writer = new BinaryWriter(str);

        writer.Write((byte)'E');
        writer.Write((byte)'M');
        writer.Write((byte)'O');

        writer.Write(0);
        writer.Write(msgIndex);
        writer.Write(data.Length);

        writer.Write(data);

        Span<byte> contentHash = stackalloc byte[32];
        MD5.TryHashData(data, contentHash, out int bytesWritten);
        writer.Write(contentHash);

        writer.Flush();
        return (int)str.Position;
    }

    public void Reset()
    {
        Sender = null;
        Valid = false;
        Index = -1;
    }

    public ByteReader? GetContentReader()
    {
        if (!Valid) return null;
        _reader.Seek(MessageContentOffset, SeekOrigin.Begin);
        return _reader;
    }
}
