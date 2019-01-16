// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.IO;
using Emotion.Engine;
using Emotion.IO;

#endregion

namespace Emotion.Sound
{
    /// <summary>
    /// A sound file asset. Supports WAV.
    /// </summary>
    public sealed class SoundFile : Asset
    {
        #region Properties

        /// <summary>
        /// The duration of the sound file.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Sound file pointer to the internal system.
        /// </summary>
        public int ALBuffer { get; internal set; }

        /// <summary>
        /// The number of channels the file has.
        /// </summary>
        public int Channels { get; private set; }

        /// <summary>
        /// The sample rate of the file.
        /// </summary>
        public int SampleRate { get; private set; }

        #endregion

        #region Asset API

        internal override void CreateAsset(byte[] data)
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
                    int bitsPerSample = reader.ReadInt16();

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

                    // Create a sound buffer and load it.
                    ALBuffer = Context.SoundManager.CreateBuffer(Channels, bitsPerSample, soundData, SampleRate);

                    // Calculate duration.
                    Duration = soundData.Length / (SampleRate * Channels * bitsPerSample / 8f);
                }
            }
        }

        /// <summary>
        /// Queue the sound file to be destroyed when it is no longer in use.
        /// </summary>
        internal override void DestroyAsset()
        {
            Context.SoundManager.DestroyBuffer(this);
        }

        #endregion

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            if (!(obj is SoundFile soundFile)) return base.Equals(obj);

            // If both are non-destroyed buffers, compare the pointers.
            if (ALBuffer != -1 && soundFile.ALBuffer != -1) return ALBuffer == soundFile.ALBuffer;

            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ALBuffer;
        }
    }
}