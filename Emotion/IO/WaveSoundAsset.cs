#region Using

using System;
using System.IO;

#endregion

namespace Emotion.IO
{
    /// <inheritdoc />
    /// <summary>
    /// A sound file asset. Supports WAV.
    /// </summary>
    public class WaveSoundAsset : Asset
    {
        #region Properties

        /// <summary>
        /// The duration of the sound file.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// The number of channels the file has.
        /// </summary>
        public int Channels { get; private set; }

        /// <summary>
        /// The samples per second, or frequency, of the file.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// The number of bits one sound sample contains.
        /// </summary>
        public int BitsPerSample { get; private set; }

        /// <summary>
        /// The sound data itself.
        /// </summary>
        public byte[] SoundData { get; private set; }

        /// <summary>
        /// Whether the data in the sound data is of the float type.
        /// The bits per sample will be 32 when it is an int and float, so this flag is needed.
        /// </summary>
        public bool IsFloat { get; private set; }

        #endregion

        protected override void CreateInternal(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            // Read RIFF header.
            var signature = new string(reader.ReadChars(4));
            if (signature != "RIFF") throw new Exception("Unsupported sound signature.");

            // Chunk size.
            reader.ReadInt32();

            var format = new string(reader.ReadChars(4));
            if (format != "WAVE") throw new Exception("Unsupported sound format.");

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
            IsFloat = soundFmt == 3;

            Channels = reader.ReadInt16();
            // Frequency.
            SampleRate = reader.ReadInt32();
            // Byte rate.
            reader.ReadInt32();
            // Block align. How many bytes is one sample - Channels * BitsPerSample / 8
            reader.ReadInt16();
            BitsPerSample = reader.ReadInt16();

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

            // Read the data.
            byte[] soundData = reader.ReadBytes(dataLength);

            // Calculate duration.
            Duration = soundData.Length / (SampleRate * Channels * BitsPerSample / 8f);

            // Set the sound data.
            SoundData = soundData;
        }

        protected override void DisposeInternal()
        {
            SoundData = null;
        }
    }
}