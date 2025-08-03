#nullable enable

#region Using

using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using OpenGL;

#endregion

namespace Emotion.Standard.Parsers.Image.ImgBin;

public struct ImgBinFileHeader
{
    public Vector2 Size;
    public PixelFormat Format;
}

public static class ImgBinFormat
{
    public static byte Version = 1;

    public static bool IsImgBin(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 3) return false;
        ReadOnlySpan<byte> span = data.Span;
        return span[0] == (byte) 'E' && span[1] == (byte) 'I' && span[2] == (byte) 'B';
    }

    public static byte[] Encode(byte[] pixels, Vector2 size, PixelFormat format)
    {
        //var fileOutput = new byte[sizeof(char) * 3 + pixels.Length + sizeof(float) * 2 + sizeof(int)];
        using var memoryStr = new MemoryStream();
        using var writer = new BinaryWriter(memoryStr);
        writer.Write((byte) 'E');
        writer.Write((byte) 'I');
        writer.Write((byte) 'B');
        writer.Write(Version);
        writer.Write((byte) 0); // Reserved
        writer.Write(size.X);
        writer.Write(size.Y);
        writer.Write((int) format);

        // Compress using the .Net deflate stream
        using var compressedStream = new MemoryStream();
        using var deflateStream = new BrotliStream(memoryStr, CompressionMode.Compress);
        deflateStream.Write(pixels);
        deflateStream.Flush();

        return memoryStr.ToArray();
    }

    public static byte[] Decode(ReadOnlyMemory<byte> fileData, out ImgBinFileHeader fileHeader)
    {
        fileHeader = new ImgBinFileHeader();

        var reader = new NonAllocByteReader(fileData);
        reader.SkipBytes(3); // Header
        reader.ReadByte(); // Version
        reader.ReadByte(); // Reserved
        float width = reader.ReadSingle();
        float height = reader.ReadSingle();
        fileHeader.Size = new Vector2(width, height);
        fileHeader.Format = (PixelFormat) reader.ReadInt32();

        ReadOnlyMemory<byte> compressedPixels = fileData.Slice(reader.Position);

        int decompressedPixelSize = (int) (width * height * Gl.PixelFormatToComponentCount(fileHeader.Format));
        byte[] pixels = ArrayPool<byte>.Shared.Rent(decompressedPixelSize);
        bool success = BrotliDecoder.TryDecompress(compressedPixels.Span, pixels, out int bytesWritten);
        Assert(bytesWritten == decompressedPixelSize);

        // This success code doesn't seem reliable
        //if (!success)
        //    Engine.Log.Warning($"Brotli decompression might not have succeeded :/", "ImgBin");

        return pixels;
    }
}