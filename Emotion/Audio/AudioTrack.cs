#region Using

using System;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; }

        /// <summary>
        /// What percentage (0-1) of the track has finished playing.
        /// </summary>
        public float Progress
        {
            get
            {
                if (SampleIndex == 0) return 0f;
                return (float) SampleIndex / TotalSamples;
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
        /// The index of the converted sample currently at.
        /// </summary>
        public int SampleIndex { get; protected set; }

        /// <summary>
        /// The total samples.
        /// </summary>
        public int TotalSamples { get; protected set; }

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
        /// begin cross fading at, and it will extend that much into the next track as well.
        /// Overrides the current track's fade out and the next track's fade in.
        /// Though if the fade in is longer than the crossfade, it will come in affect as soon as the crossfade finishes. (which
        /// you probably dont want)
        /// </summary>
        public float? CrossFade;

        /// <summary>
        /// Whether to set the layer's LoopingCurrent setting to true when played.
        /// </summary>
        public bool SetLoopingCurrent { get; set; }

        private AudioLayer _layer;
        private TrackResampleCache _cache;
        private AudioFormat _sampleIndexFormat;

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

        public bool EnsureAudioFormat(AudioFormat format)
        {
            if (!format.Equals(_sampleIndexFormat))
            {
                SampleIndex = _cache.SetConvertFormatAndCacheFrom(format, Progress);
                _sampleIndexFormat = format;
                TotalSamples = _cache.ConvSamples;

                return true;
            }

            return false;
        }

        public int GetNextFrames(int frameCount, Span<float> buffer)
        {
            int sampleCount = GetNextSamplesAt(SampleIndex, frameCount, buffer);
            SampleIndex += sampleCount;
            return sampleCount / _sampleIndexFormat.Channels;
        }

        public int GetNextSamplesAt(int offset, int frameCount, Span<float> buffer)
        {
            return _cache.GetCachedSamples(offset, frameCount, buffer);
        }

        public void Reset(int setEmitTo = 0)
        {
            SampleIndex = setEmitTo;
        }

        public bool SetOwningLayer(AudioLayer layer)
        {
            if (_layer != null)
            {
                Engine.Log.Error("Tried to play a track which is already playing on another layer.", MessageSource.Audio);
                return false;
            }

            _layer = layer;
            return true;
        }
    }
}