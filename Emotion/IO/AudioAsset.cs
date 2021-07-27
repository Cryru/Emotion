#region Using

using System;
using System.Threading;
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
        public Lazy<AudioStreamer> AudioStream { get; }

        #endregion

        public AudioAsset()
        {
            AudioStream = new Lazy<AudioStreamer>(() => new AudioStreamer(Format, SoundData), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            // Check if WAV.
            if (WavFormat.IsWav(data))
            {
                // Get the data.
                ReadOnlyMemory<byte> pcm = WavFormat.Decode(data, out AudioFormat format);
                if (pcm.IsEmpty) return;
                if (format.UnsupportedBitsPerSample())
                    Engine.Log.Warning($"Unsupported bits per sample ({format.BitsPerSample}) format in audio file {Name}", MessageSource.Audio);

                // Convert to stereo 32f to have a consistent base format for all audio.
                int sourceSamples = pcm.Length / format.SampleSize;
                var monoToStereo = false;
                if (format.Channels == 1)
                {
                    sourceSamples *= 2;
                    format.Channels = 2;
                    monoToStereo = true;
                }
                if (format.Channels != 2) Engine.Log.Warning($"Unsupported channel count ({format.Channels}). Only stereo or mono supported.", MessageSource.Audio);

                var soundDataFloat = new float[sourceSamples];
                if (monoToStereo)
                    for (var i = 0; i < sourceSamples; i++)
                    {
                        float sample = AudioStreamer.GetSampleAsFloat(i / 2, pcm.Span, format);
                        soundDataFloat[i] = sample;
                        soundDataFloat[i + 1] = sample;
                    }
                else
                    for (var i = 0; i < sourceSamples; i++)
                    {
                        soundDataFloat[i] = AudioStreamer.GetSampleAsFloat(i, pcm.Span, format);
                    }

                format.IsFloat = true;
                format.BitsPerSample = 32;
                SoundData = soundDataFloat;
                Size = SoundData.Length * 4;

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