#region Using

using System;
using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; set; }

        /// <summary>
        /// The layer the track is playing on.
        /// </summary>
        public AudioLayer Layer { get; set; }

        /// <summary>
        /// What percentage (0-1) of the track has finished playing.
        /// </summary>
        public float Progress
        {
            get
            {
                if (_emitPtr == 0) return 0f;
                return (float) _emitPtr / _emitPtrFormatConvSamples;
            }
        }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get => Progress * File.Duration;
        }

        /// <summary>
        /// Fade in time in seconds. Null for none.
        /// If the number is negative it is the track progress (0-1)
        /// The timestamp is the time to finish fading in at.
        /// </summary>
        public float? FadeIn;

        /// <summary>
        /// Fade out time in seconds. Null for none.
        /// If the number is negative it is the track progress (0-1)
        /// The timestamp is the time to begin fading out at.
        /// </summary>
        public float? FadeOut;

        /// <summary>
        /// CrossFade this track into the next (if any). The timestamp is the time to
        /// begin cross fading at.
        /// </summary>
        public float? CrossFade;

        private TrackResampleCache _cache;
        private int _emitPtr;
        private AudioFormat _emitPtrFormat;
        private int _emitPtrFormatConvSamples;

        public AudioTrack(AudioAsset file)
        {
            File = file;

            if (file.ResampleCache == null)
                lock (file)
                {
                    file.ResampleCache ??= new TrackResampleCache(file);
                }

            _cache = file.ResampleCache;
        }

        public void EnsureAudioFormat(AudioFormat format)
        {
            if (!format.Equals(_emitPtrFormat))
            {
                _emitPtr = _cache.SetConvertFormatAndCacheFrom(format, Progress);
                _emitPtrFormat = format;
                _emitPtrFormatConvSamples = _cache.ConvSamples;
            }
        }

        public int GetNextFrames(int frameCount, Span<float> buffer)
        {
            int sampleCount = _cache.GetCachedSamples(_emitPtr, frameCount, buffer);
            _emitPtr += sampleCount;
            return sampleCount / _emitPtrFormat.Channels;
        }

        public void Reset()
        {
            _emitPtr = 0;
        }
    }
}