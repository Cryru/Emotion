#region Using

using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Standard.Image.ImgBin
{
    public class ImgBinFileHeader
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

            var reader = new NonAllocBinaryReader(fileData);
            reader.SkipBytes(3); // Header
            reader.ReadByte(); // Version
            reader.ReadByte(); // Reserved
            float width = reader.ReadSingle();
            float height = reader.ReadSingle();
            fileHeader.Size = new Vector2(width, height);
            fileHeader.Format = (PixelFormat) reader.ReadInt32();

            var compressedPixels = fileData.Slice(reader.Position);

            // Decompress using the .Net deflate stream
            using var compressInput = new ReadOnlyMemoryStream(compressedPixels);
            using var deflateStream = new BrotliStream(compressInput, CompressionMode.Decompress);
            using var decompressOutput = new MemoryStream();
            deflateStream.CopyTo(decompressOutput);
            deflateStream.Flush();

            return decompressOutput.ToArray();
        }
    }

    public struct NonAllocBinaryReader
    {
        public ReadOnlyMemory<byte> Data;
        public int Position;

        public NonAllocBinaryReader(ReadOnlyMemory<byte> memory)
        {
            Data = memory;
        }

        public void SkipBytes(int bytes)
        {
            for (int i = 0; i < bytes; i++)
            {
                ReadGeneric<byte>();
            }
        }

        public byte ReadByte()
        {
            return ReadGeneric<byte>();
        }

        public int ReadInt32()
        {
            return ReadGeneric<int>();
        }

        public float ReadSingle()
        {
            return ReadGeneric<float>();
        }

        public unsafe T ReadGeneric<T>() where T : unmanaged
        {
            var size = sizeof(T);
            var output = MemoryMarshal.Read<T>(Data.Slice(Position, size).Span);
            Position += size;
            return output;
        }
    }
}