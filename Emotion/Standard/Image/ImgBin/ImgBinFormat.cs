#region Using

using System;
using System.IO;
using System.Numerics;
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
        public static bool IsImgBin(ReadOnlyMemory<byte> data)
        {
            if (data.Length < 3) return false;
            ReadOnlySpan<byte> span = data.Span;
            return span[0] == (byte)'E' && span[1] == (byte)'I' && span[2] == (byte)'B';
        }

        public static byte[] Encode(byte[] pixels, Vector2 size, PixelFormat format)
        {
            var fileOutput = new byte[sizeof(char) * 3 + pixels.Length + sizeof(float) * 2 + sizeof(int)];
            using var memoryStr = new MemoryStream(fileOutput);
            using var writer = new BinaryWriter(memoryStr);
            writer.Write((byte) 'E');
            writer.Write((byte) 'I');
            writer.Write((byte) 'B');
            writer.Write(size.X);
            writer.Write(size.Y);
            writer.Write((int)format);
            writer.Write(pixels);

            return fileOutput;
        }

        public static byte[] Decode(ReadOnlyMemory<byte> fileData, out ImgBinFileHeader fileHeader)
        {
            fileHeader = new ImgBinFileHeader();

            var stream = new ReadOnlyMemoryStream(fileData);
            using var r = new BinaryReader(stream);
            r.ReadChars(3); // Header
            float width = r.ReadSingle();
            float height = r.ReadSingle();
            fileHeader.Size = new Vector2(width, height);
            fileHeader.Format = (PixelFormat)r.ReadInt32();
            return fileData.Span.Slice((int) stream.Position).ToArray();
        }
    }
}