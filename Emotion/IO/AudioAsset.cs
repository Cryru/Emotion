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
        public ReadOnlyMemory<byte> SoundData { get; private set; }

        /// <summary>
        /// The sound format.
        /// </summary>
        public AudioFormat Format { get; private set; }

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
                SoundData = WavFormat.Decode(data, out AudioFormat format);
                Size = SoundData.Length;
                Format = format;
                Duration = format.GetSoundDuration(SoundData.Length);
                AudioConverter = new AudioConverter(Format, SoundData);

                if (Format.UnsupportedBitsPerSample())
                    Engine.Log.Error($"The audio format of {Name} has an unsupported number of bits per sample ({Format.BitsPerSample}). Supported values are 8/16/32", MessageSource.Audio);
            }

            if (Format == null || SoundData.IsEmpty) Engine.Log.Warning($"Couldn't load audio file - {Name}.", MessageSource.AssetLoader);
        }

        protected override void DisposeInternal()
        {
            SoundData = null;
            Format = null;
        }
    }
}