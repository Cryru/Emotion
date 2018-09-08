// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.IO;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.IO
{
    public sealed class SoundFile : Asset
    {
        internal int Pointer;
        public int Duration { get; private set; }

        internal override void Create(byte[] data)
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
                    int channels = reader.ReadInt16();
                    // Frequency.
                    int sampleRate = reader.ReadInt32();
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
                    Pointer = AL.GenBuffer();
                    AL.BufferData(Pointer, GetSoundFormat(channels, bitsPerSample), soundData, soundData.Length, sampleRate);

                    Duration = soundData.Length / (sampleRate * channels * bitsPerSample / 8);
                }
            }
        }

        internal override void Destroy()
        {
            AL.DeleteBuffer(Pointer);
        }

        #region Helpers

        /// <summary>
        /// Returns the type of sound format based on channels and bits.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bits">The number of bits.</param>
        /// <returns>The OpenAL </returns>
        private static ALFormat GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new Exception("Unknown format.");
            }
        }

        #endregion
    }
}