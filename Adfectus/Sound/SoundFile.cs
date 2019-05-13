#region Using

using System;
using System.IO;
using Adfectus.Common;
using Adfectus.IO;

#endregion

namespace Adfectus.Sound
{
    /// <inheritdoc />
    /// <summary>
    /// A sound file asset. Supports WAV.
    /// </summary>
    public class SoundFile : Asset
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
        /// The sample rate of the file.
        /// </summary>
        public int SampleRate { get; private set; }

        /// <summary>
        /// The bits per sample.
        /// </summary>
        public int BitsPerSample { get; private set; }

        /// <summary>
        /// The data of the sound file.
        /// </summary>
        public byte[] SoundData { get; private set; }

        #endregion

        protected override void CreateInternal(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // Read RIFF header.
                    string signature = new string(reader.ReadChars(4));
                    if (signature != "RIFF") throw new Exception("Unsupported sound signature.");

                    // Chunk size.
                    reader.ReadInt32();

                    string format = new string(reader.ReadChars(4));
                    if (format != "WAVE") throw new Exception("Unsupported sound format.");

                    // Read WAVE header.
                    string formatSignature = new string(reader.ReadChars(4));

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
                    reader.ReadInt16();
                    Channels = reader.ReadInt16();
                    // Frequency.
                    SampleRate = reader.ReadInt32();
                    // Byte rate.
                    reader.ReadInt32();
                    // Block align.
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
            }
        }

        protected override void DisposeInternal()
        {
            SoundData = null;
        }
    }
}