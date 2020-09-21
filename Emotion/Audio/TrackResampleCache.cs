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
    public class TrackResampleCache : AudioStreamer
    {
        public int CacheDirty;

        private const int RESAMPLE_INTERVAL = 4000;

        private float[] _cache;
        private int _cachePtr;

        private Task _resampleTask;
        private bool _cancelResample;
        private AutoResetEvent _resampleEmit = new AutoResetEvent(false);

        public TrackResampleCache(AudioAsset asset) : base(asset.Format, asset.SoundData)
        {
        }

        /// <summary>
        /// Get frames from the resampling cache.
        /// </summary>
        public int GetCachedSamples(int fromIdx, int frameCount, Span<float> buffer)
        {
            if (_cache == null) return 0;

            // Check if requesting samples from the point at which the buffer is dirty.
            if (CacheDirty != 0 && fromIdx < CacheDirty)
            {
                Engine.Log.Trace("Resample cache is dirty, will resample again.", MessageSource.Audio);
                CacheDirty = 0;
                _cachePtr = 0;
                Reset();

                // Stop if running.
                if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

                lock (this)
                {
                    _cancelResample = false;
                    _resampleTask = Task.Run(FillCache);
                }
            }

            // Check if there's enough samples in the cache.
            int channels = ConvFormat.Channels;
            int sampleEndIdx = Math.Min(fromIdx + frameCount * channels, ConvSamples);
            if (sampleEndIdx > _cachePtr)
            {
                while (sampleEndIdx > _cachePtr)
                {
                    if (_resampleEmit.WaitOne(5000)) continue;
                    Engine.Log.Warning("Timed out while waiting for cache samples.", MessageSource.Audio);
                    return 0;
                }
            }

            // Copy over the needed samples.
            int sampleCount = sampleEndIdx - fromIdx;
            new Span<float>(_cache).Slice(fromIdx, sampleCount).CopyTo(buffer);

            return sampleCount;
        }

        /// <inheritdoc />
        public override void SetConvertFormat(AudioFormat dstFormat, int quality = 10, bool keepProgress = true)
        {
            // Cancel the old task.
            if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

            lock (this)
            {
                base.SetConvertFormat(dstFormat, quality, keepProgress);

                // Resize/allocate cache if needed.
                if (_cache == null)
                    _cache = new float[ConvSamples];
                else if (_cache.Length < ConvSamples)
                    Array.Resize(ref _cache, ConvSamples);
            }

            // Start resampling thread.
            _cancelResample = false;
            _resampleTask = Task.Run(FillCache);
        }

        /// <summary>
        /// Set the convert format, and restart caching from a certain point.
        /// </summary>
        /// <param name="dstFormat">The format to set.</param>
        /// <param name="progress">The progress to restart caching from.</param>
        /// <returns></returns>
        public int SetConvertFormatAndCacheFrom(AudioFormat dstFormat, float progress)
        {
            // Cancel the old task.
            if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

            int restartIndex;

            // This lock will ensure the handle from the resample task is unlocked.
            lock (this)
            {
                base.SetConvertFormat(dstFormat, ConvQuality, false);
                _cachePtr = 0;

                // Restore resample thread progress from emit progress
                // We don't want to wait for everything to be resampled from the very beginning.
                restartIndex = (int) MathF.Floor(ConvSamples * progress);
                FastForwardResample(ref _srcResume, ref _dstResume, restartIndex);
                _cachePtr = _dstResume;
                CacheDirty = _cachePtr;

                // Resize/allocate cache if needed.
                if (_cache == null)
                    _cache = new float[ConvSamples];
                else if (_cache.Length < ConvSamples)
                    Array.Resize(ref _cache, ConvSamples);
            }

            // Start resampling thread.
            _cancelResample = false;
            _resampleTask = Task.Run(FillCache);

            return restartIndex;
        }

        /// <summary>
        /// Resample the track ahead of where it is requested. This allows for better performance.
        /// Maybe store the cache within the Audio Asset?
        /// The cache is not guaranteed to contain the full resampled track - if the resampling resumes.
        /// </summary>
        private void FillCache()
        {
            Thread.CurrentThread.Name ??= "Resample Thread";
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
        private int FastForwardResample(ref double srcStartIdx, ref int dstSampleIdx, int getSamples)
        {
            int channels = ConvFormat.Channels;
            int iStart = dstSampleIdx;

            if (dstSampleIdx + getSamples >= ConvSamples) getSamples = ConvSamples - dstSampleIdx;
            for (; dstSampleIdx < ConvSamples; dstSampleIdx += channels)
            {
                if (dstSampleIdx + channels - iStart > getSamples) return getSamples;
                srcStartIdx += _resampleStep;
            }

            return ConvSamples - iStart;
        }
    }
}