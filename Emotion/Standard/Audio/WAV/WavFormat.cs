#region Using

using System;
using System.IO;
using System.Text;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Audio.WAV
{
    public static class WavFormat
    {
        /// <summary>
        /// The size of the magic for recognizing this file.
        /// </summary>
        public const int FILE_MAGIC_SIZE = 2;

        /// <summary>
        /// Verify whether the bytes are a wav file, by checking the magic.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns>Whether the data fits the wav magic.</returns>
        public static bool IsWav(ReadOnlyMemory<byte> data)
        {
            ReadOnlySpan<byte> span = data.Span;

            var wav = false;
            if (data.Length >= 4)
                wav =
                    span[0] == 'R' &&
                    span[1] == 'I' &&
                    span[2] == 'F' &&
                    span[3] == 'F';

            return wav;
        }

        /// <summary>
        /// Encodes the provided sound data to a wav file.
        /// </summary>
        /// <param name="soundData">The date to encode.</param>
        /// <param name="format">The format of the data.</param>
        /// <returns>A 24bit BMP image as bytes.</returns>
        public static byte[] Encode(ReadOnlyMemory<byte> soundData, AudioFormat format)
        {
            var file = new byte[12 + 24 + 8 + soundData.Length];
            using var writer = new BinaryWriter(new MemoryStream(file));

            // Header - 12
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(file.Length - 8);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            // Format header - 24
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write(format.IsFloat ? (short) 3 : (short) 1); // 2
            writer.Write((short) format.Channels); // 2
            writer.Write(format.SampleRate); // 4
            writer.Write(format.SampleRate * format.FrameSize); // 4
            writer.Write((short) format.FrameSize); // 2
            writer.Write((short) format.BitsPerSample); // 2

            // Data - 8 + soundLength
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(soundData.Length);
            writer.Write(soundData.Span);

            return file;
        }

        /// <summary>
        /// Decodes the provided wav data to sound data.
        /// </summary>
        /// <param name="wavData">The data to decode.</param>
        /// <param name="format">The audio format.</param>
        /// <returns>The sound data.</returns>
        public static ReadOnlyMemory<byte> Decode(ReadOnlyMemory<byte> wavData, out AudioFormat format)
        {
            format = new AudioFormat();

            using var reader = new ByteReader(wavData);

            // Read RIFF header.
            var signature = new string(reader.ReadChars(4));
            if (signature != "RIFF") throw new Exception("Unsupported sound signature.");

            // Chunk size.
            reader.ReadInt32();

            var f = new string(reader.ReadChars(4));
            if (f != "WAVE") throw new Exception("Unsupported sound format.");

            // Read WAVE header.
            var formatSignature = new string(reader.ReadChars(4));

            // Skip junk.
            while (formatSignature != "fmt ")
            {
                int junkSize = reader.ReadInt32();
                reader.ReadBytes(junkSize);
                formatSignature = new string(reader.ReadChars(4));
            }

            // Format chunk size.
            int chunkSize = reader.ReadInt32();

            // Audio format.
            short soundFmt = reader.ReadInt16();
            format.IsFloat = soundFmt == 3;

            format.Channels = reader.ReadInt16();
            // Frequency.
            format.SampleRate = reader.ReadInt32();
            // Byte rate.
            reader.ReadInt32();
            // Block align. How many bytes is one sample - Channels * BitsPerSample / 8
            reader.ReadInt16();
            format.BitsPerSample = reader.ReadInt16();

            // Finish the rest of the chunk.
            reader.ReadBytes(chunkSize - 16);

            // Read the signature.
            formatSignature = new string(reader.ReadChars(4));

            // Find the data chunk.
            while (formatSignature != "data")
            {
                int junkSize = reader.ReadInt32();
                reader.ReadBytes(junkSize);
                formatSignature = new string(reader.ReadChars(4));
            }

            // Read the data chunk length.
            int dataLength = reader.ReadInt32();
            // If invalid length, interpret the reset of the file as the data.
            if (dataLength == -1 || dataLength == 0)
            {
                reader.Position -= 4;
                dataLength = (int) (reader.Length - reader.Position);
            }

            // Get the data and return it. This won't copy it.
            return wavData.Slice(reader.Position, dataLength);
        }
    }
}