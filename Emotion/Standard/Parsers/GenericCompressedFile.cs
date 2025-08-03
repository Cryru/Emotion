#nullable enable

#region Using

using System.Buffers;
using System.IO;
using System.IO.Compression;

#endregion

namespace Emotion.Standard.Parsers;

public static class GenericCompressedFile
{
    public static byte Version = 1;

    public static bool IsGenericCompressedFile(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 3) return false;
        ReadOnlySpan<byte> span = data.Span;
        return span[0] == (byte) 'E' && span[1] == (byte) 'C' && span[2] == (byte) 'F';
    }

    public static byte[] Encode(byte[] fileBytes)
    {
        using var memoryStr = new MemoryStream();
        using var writer = new BinaryWriter(memoryStr);
        writer.Write((byte) 'E');
        writer.Write((byte) 'C');
        writer.Write((byte) 'F');
        writer.Write(Version);
        writer.Write((byte) 0); // Reserved
        writer.Write(fileBytes.Length);

        // Compress using the .Net deflate stream
        using var compressedStream = new MemoryStream();
        using var deflateStream = new BrotliStream(memoryStr, CompressionMode.Compress);
        deflateStream.Write(fileBytes);
        deflateStream.Flush();

        return memoryStr.ToArray();
    }

    public static byte[] Decode(ReadOnlyMemory<byte> fileData)
    {
        var reader = new NonAllocByteReader(fileData);
        reader.SkipBytes(3); // Header
        reader.ReadByte(); // Version
        reader.ReadByte(); // Reserved

        int decompressedFileLength = reader.ReadInt32();
        byte[] decompressedData = ArrayPool<byte>.Shared.Rent(decompressedFileLength);

        ReadOnlyMemory<byte> compressedData = fileData.Slice(reader.Position);
        bool success = BrotliDecoder.TryDecompress(compressedData.Span, decompressedData, out int bytesWritten);
        Assert(bytesWritten == decompressedFileLength);

        // This success code doesn't seem reliable
        //if (!success)
        //    Engine.Log.Warning($"Brotli decompression might not have succeeded :/", "ImgBin");

        return decompressedData;
    }
}