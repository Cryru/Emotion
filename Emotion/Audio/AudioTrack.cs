#region Using

using System;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
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

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }

        public override float Progress
        {
            get
            {
                if (_emitPtr == 0) return 0f;
                return (float) _emitPtr / _dstLength;
            }
        }

        private float[] _cache;
        private bool _cacheDirty;
        private int _cachePtr;
        private int _emitPtr;
        private const int RESAMPLE_INTERVAL = 4000;

        private Task _resampleTask;
        private bool _cancelResample;
        private AutoResetEvent _resampleEmit = new AutoResetEvent(false);

        public override void SetConvertFormat(AudioFormat dstFormat, int quality = 10, bool keepProgress = true)
        {
            // Cancel the old task.
            if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

            // This lock will ensure the handle from the resample task is unlocked.
            lock (this)
            {
                float progress = Progress;
                base.SetConvertFormat(dstFormat, quality, keepProgress);

                // Reset resampling.
                _cachePtr = 0;
                _emitPtr = 0;
                base.Reset();

                // Restore resample thread progress from emit progress
                // We don't want to wait for everything to be resampled from the very beginning.
                if (keepProgress)
                {
                    _cacheDirty = true;
                    _emitPtr = (int) MathF.Floor(_dstLength * progress);
                    FastForwardResample(ref _srcResume, ref _dstResume, _emitPtr);
                    _cachePtr = _dstResume;
                }
            }

            // Resize/allocate cache if needed.
            if (_cache == null)
                _cache = new float[_dstLength];
            else if (_cache.Length < _dstLength) Array.Resize(ref _cache, _dstLength);

            // Start resampling thread.
            _cancelResample = false;
            _resampleTask = Task.Run(FillCache);
        }

        public override void Reset()
        {
            base.Reset();
            _emitPtr = 0;
            if (!_cacheDirty) return;

            Engine.Log.Trace("Resample cache is dirty, will resample again.", MessageSource.Audio);
            _cachePtr = 0;
            _cacheDirty = false;

            if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

            lock (this)
            {
                // Start resampling thread.
                _cancelResample = false;
                _resampleTask = Task.Run(FillCache);
            }
        }

        /// <summary>
        /// Resample the track ahead of where it is requested. This allows for better performance.
        /// Maybe store the cache within the Audio Asset?
        /// The cache is not guaranteed to contain the full resampled track - if the resampling resumes.
        /// </summary>
        private void FillCache()
        {
            Thread.CurrentThread.Name ??= $"Resample Thread - {File.Name}";
            Engine.Log.Trace("Starting resample.", MessageSource.Audio);
            lock (this)
            {
                int channels = ConvFormat.Channels;
                int framesGotten = -1;
                Span<float> dataSpan = _cache;
                while (framesGotten != 0)
                {
                    framesGotten = base.GetNextFrames(RESAMPLE_INTERVAL, dataSpan.Slice(_cachePtr));
                    if (_cancelResample) return;
                    _cachePtr += framesGotten * channels;
                    _resampleEmit.Set();
                }
            }

            Engine.Log.Trace("Resample finished.", MessageSource.Audio);
        }

        /// <summary>
        /// The resample function, without the buffer fill.
        /// Used to restore resampling thread progress.
        /// </summary>
        protected int FastForwardResample(ref double srcStartIdx, ref int dstSampleIdx, int getSamples)
        {
            int channels = ConvFormat.Channels;
            int iStart = dstSampleIdx;

            if (dstSampleIdx + getSamples >= _dstLength) getSamples = _dstLength - dstSampleIdx;
            for (; dstSampleIdx < _dstLength; dstSampleIdx += channels)
            {
                srcStartIdx += _resampleStep;
                if (dstSampleIdx + channels - iStart < getSamples) continue;
                dstSampleIdx += channels;
                return getSamples;
            }

            return _dstLength - iStart;
        }

        /// <summary>
        /// Get frames from the resampling cache.
        /// </summary>
        public override int GetNextFrames(int frameCount, Span<float> buffer)
        {
            if (_cache == null) return 0;

            // Check if there's enough samples in the cache.
            int channels = ConvFormat.Channels;
            int samplesGet = Math.Min(_emitPtr + frameCount * channels, _dstLength);
            if (samplesGet > _cachePtr)
            {
                Engine.Log.Trace($"Trying to emit samples which aren't ready. Requested: {samplesGet}, CachePtr: {_cachePtr}", MessageSource.Audio);
                while (samplesGet > _cachePtr)
                {
                    if (_resampleEmit.WaitOne(5000)) continue;
                    Engine.Log.Warning("Timed out while waiting for cache samples.", MessageSource.Audio);
                    return 0;
                }
            }

            // Copy over the needed samples.
            int samplesLength = samplesGet - _emitPtr;
            new Span<float>(_cache).Slice(_emitPtr, samplesLength).CopyTo(buffer);
            _emitPtr = samplesGet;

            return samplesLength / channels;
        }
    }
}