#region Using

using System;
using Emotion.Audio;
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
        /// The raw PCM as floats.
        /// </summary>
        public Memory<float> SoundData { get; private set; }

        /// <summary>
        /// The sound format.
        /// </summary>
        public AudioFormat Format { get; private set; }

        /// <summary>
        /// A cached version of the track. Resampled and converted to a specific format.
        /// </summary>
        public TrackResampleCache ResampleCache { get; set; }

        #endregion

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            // Check if WAV.
            if (WavFormat.IsWav(data))
            {
                // Get the data.
                ReadOnlyMemory<byte> pcm = WavFormat.Decode(data, out AudioFormat format);
                if (pcm.IsEmpty) return;
                if(format.UnsupportedBitsPerSample())
                    Engine.Log.Warning($"Unsupported bits per sample ({format.BitsPerSample}) format in audio file {Name}", MessageSource.Audio);

                // Convert to float, for easier resampling and post processing.
                int sourceSamples = pcm.Length / format.SampleSize;
                var soundDataFloat = new float[sourceSamples];
                for (var i = 0; i < sourceSamples; i++)
                {
                    soundDataFloat[i] = AudioStreamer.GetSampleAsFloat(i, pcm.Span, format);
                }

                format.IsFloat = true;
                format.BitsPerSample = 32;
                SoundData = soundDataFloat;

                Format = format;
                Duration = format.GetSoundDuration(SoundData.Length * sizeof(float));
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