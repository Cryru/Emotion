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

        public int SampleIndex
        {
            get => _emitPtr;
        }

        public int TotalSamples
        {
            get => _emitPtrFormatConvSamples;
        }

        public float SecondaryPlayHeadProgress
        {
            get => (float) _emitPtrTwo / _emitPtrFormatConvSamples;
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
        /// begin cross fading at, and it will extend that much into the next track as well.
        /// Overrides the current track's fade out and the next track's fade in.
        /// Though if the fade in is longer than the crossfade, it will come in affect as soon as the crossfade finishes. (which you probably dont want)
        /// </summary>
        public float? CrossFade;

        /// <summary>
        /// Whether to set the layer's LoopingCurrent setting to true when played.
        /// </summary>
        public bool SetLoopingCurrent;

        private AudioLayer _layer;
        private TrackResampleCache _cache;
        private int _emitPtr;
        private AudioFormat _emitPtrFormat;
        private int _emitPtrFormatConvSamples;


        private int _emitPtrTwo;

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
                float secondaryProgress = SecondaryPlayHeadProgress;

                _emitPtr = _cache.SetConvertFormatAndCacheFrom(format, Progress);
                _emitPtrFormat = format;
                _emitPtrFormatConvSamples = _cache.ConvSamples;

                if (_emitPtrTwo != 0) _emitPtrTwo = (int) MathF.Floor(_emitPtrFormatConvSamples * secondaryProgress);
            }
        }

        public int GetNextFrames(int frameCount, Span<float> buffer, bool secondaryPlayHead = false)
        {
            int sampleCount;
            if (secondaryPlayHead)
            {
                sampleCount = _cache.GetCachedSamples(_emitPtrTwo, frameCount, buffer);
                _emitPtrTwo += sampleCount;
            }
            else
            {
                sampleCount = _cache.GetCachedSamples(_emitPtr, frameCount, buffer);
                _emitPtr += sampleCount;
            }

            return sampleCount / _emitPtrFormat.Channels;
        }

        public void Reset()
        {
            _emitPtr = _emitPtrTwo;
            _emitPtrTwo = 0;
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