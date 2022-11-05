#region Using

using System;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Standard.Audio.WAV;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    /// <inheritdoc />
    /// <summary>
    /// A sound file asset. Supports WAV.
    /// </summary>
    public class AudioAsset : Asset
    {
        #region Properties

        /// <summary>
        /// The duration of the sound file, in seconds.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// The raw PCM in Format.
        /// </summary>
        public float[] SoundData { get; private set; }

        /// <summary>
        /// The sound format of the PCM.
        /// </summary>
        public AudioFormat Format { get; private set; }

        /// <summary>
        /// The format the file was read in.
        /// </summary>
        public AudioFormat OriginalFormat { get; private set; }

        /// <summary>
        /// A cached version of the track. Resampled and converted to a specific format.
        /// </summary>
        public AudioConverter AudioConverter { get; private set; }

        #endregion

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            // Check if WAV.
            if (WavFormat.IsWav(data))
            {
                // Get the data.
                ReadOnlySpan<byte> soundDataDecoded = WavFormat.Decode(data, out AudioFormat format).Span;
                if (format.SampleSize == 0)
                {
                    Engine.Log.Error($"Error reading sound file {Name}.", MessageSource.Audio);
                    return;
                }

                // Convert to float and store it as such.
                SoundData = new float[soundDataDecoded.Length / format.SampleSize];
                for (var i = 0; i < SoundData.Length; i++)
                {
                    SoundData[i] = AudioConverter.GetSampleAsFloat(i, soundDataDecoded, format);
                }

                OriginalFormat = format.Copy();
                format.BitsPerSample = 32;
                format.IsFloat = true;

                ByteSize = SoundData.Length;
                Format = format;
                Duration = format.GetSoundDuration(SoundData.Length * sizeof(float));
                AudioConverter = new AudioConverter(Format, SoundData, 10);

                if (Format.UnsupportedBitsPerSample())
                    Engine.Log.Error($"The audio format of {Name} has an unsupported number of bits per sample ({Format.BitsPerSample}). Supported values are 8/16/32", MessageSource.Audio);
            }

            if (Format == null || SoundData == null) Engine.Log.Warning($"Couldn't load audio file - {Name}.", MessageSource.AssetLoader);
        }

        protected override void DisposeInternal()
        {
            // nop
        }
    }
}