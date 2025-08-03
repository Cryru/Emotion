#nullable enable

#region Using

using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Emotion.Core.Systems.Audio;
using Emotion.Standard.Parsers.Image.ImgBin;


#endregion

namespace Emotion.Standard.Parsers.Audio.AudioBin;

public static class AudioBinFormat
{
    public static byte Version = 1;

    public static bool IsAudioBin(ReadOnlyMemory<byte> data)
    {
        if (data.Length < 3) return false;
        ReadOnlySpan<byte> span = data.Span;
        return span[0] == (byte) 'E' && span[1] == (byte) 'A' && span[2] == (byte) 'B';
    }

    public static byte[] Encode(float[] soundData, int sampleRate)
    {
        //var fileOutput = new byte[sizeof(char) * 3 + pixels.Length + sizeof(float) * 2 + sizeof(int)];
        using var memoryStr = new MemoryStream();
        using var writer = new BinaryWriter(memoryStr);
        writer.Write((byte) 'E');
        writer.Write((byte) 'A');
        writer.Write((byte) 'B');
        writer.Write(Version);
        writer.Write((byte) 0); // Reserved
        writer.Write(sampleRate);
        writer.Write(soundData.Length * sizeof(float));

        // Compress using the .Net deflate stream
        using var compressedStream = new MemoryStream();
        using var deflateStream = new BrotliStream(memoryStr, CompressionMode.Compress);
        Span<byte> soundAsByte = MemoryMarshal.Cast<float, byte>(soundData);
        deflateStream.Write(soundAsByte);
        deflateStream.Flush();

        return memoryStr.ToArray();
    }

    public static float[] Decode(ReadOnlyMemory<byte> fileData, out AudioFormat format)
    {
        format = new AudioFormat();

        var reader = new NonAllocByteReader(fileData);
        reader.SkipBytes(3); // Header
        reader.ReadByte(); // Version
        reader.ReadByte(); // Reserved

        int sampleRate = reader.ReadInt32();
        format.SampleRate = sampleRate;

        int soundByteLength = reader.ReadInt32();

        ReadOnlyMemory<byte> compressedData = fileData.Slice(reader.Position);
        float[] soundData = new float[soundByteLength / sizeof(float)];
        Span<byte> soundAsByte = MemoryMarshal.Cast<float, byte>(soundData);
        bool success = BrotliDecoder.TryDecompress(compressedData.Span, soundAsByte, out int bytesWritten);
        Assert(bytesWritten == soundByteLength);

        // This success code doesn't seem reliable
        //if (!success)
        //    Engine.Log.Warning($"Brotli decompression might not have succeeded :/", "ImgBin");

        return soundData;
    }
}