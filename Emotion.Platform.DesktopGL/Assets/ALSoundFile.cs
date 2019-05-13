using System;
using Adfectus.Common;
using Adfectus.OpenAL;
using Adfectus.Sound;
using Emotion.Platform.DesktopGL.Sound;

namespace Emotion.Platform.DesktopGL.Assets
{
    public sealed class ALSoundFile : SoundFile
    {
        public uint ALBuffer { get; set; }
        
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            if (!(obj is ALSoundFile soundFile)) return base.Equals(obj);

            // If both are non-destroyed buffers, compare the pointers.
            if (ALBuffer != 0 && soundFile.ALBuffer != 0) return ALBuffer == soundFile.ALBuffer;

            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(obj);
        }

        protected override unsafe void CreateInternal(byte[] soundData)
        {
            // Load by base SoundFile.
            base.CreateInternal(soundData);

            // Upload to the sound manager.
            ALThread.ExecuteALThread(() =>
            {
                Al.GenBuffer(out uint buffer);
                ALBuffer = buffer;

                fixed (byte* dataBuffer = &soundData[0])
                {
                    Al.BufferData(ALBuffer, GetSoundFormat(Channels, BitsPerSample), dataBuffer, soundData.Length, SampleRate);
                }
            }).Wait();
        }

        protected override void DisposeInternal()
        {
            ((ALSoundManager) Engine.SoundManager).DestroyBuffer(this);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int) ALBuffer;
        }

        /// <summary>
        /// Returns the type of sound format based on channels and bits.
        /// </summary>
        /// <param name="channels">The number of channels.</param>
        /// <param name="bits">The number of bits.</param>
        /// <returns>
        /// The OpenAL sound format depending ont he channels and bits. Supported formats are 8bit mono and stereo, and 16
        /// bit mono and stereo.
        /// </returns>
        private static int GetSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? Al.FormatMono8 : Al.FormatMono16;
                case 2: return bits == 8 ? Al.FormatStereo8 : Al.FormatStereo16;
                default: throw new Exception("Unknown format.");
            }
        }
    }
}
