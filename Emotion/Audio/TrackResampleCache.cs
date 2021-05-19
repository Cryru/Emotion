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
        public static int RESAMPLE_CACHE_TIMEOUT = 1000;
        public static int RESAMPLE_INTERVAL = 4000;

        private float[] _cache;

        /// <summary>
        /// The point up to which the cache is filled.
        /// </summary>
        private int _cachePtr;

        /// <summary>
        /// The point after which the cache is dirty.
        /// </summary>
        private int _cachePtrDirty;

        private Task _resampleTask;
        private object _resampleThreadLock = new();
        private bool _cancelResample;
        private AutoResetEvent _resampleEmit = new(false);

        public TrackResampleCache(AudioAsset asset) : base(asset.Format, asset.SoundData)
        {
        }

        /// <summary>
        /// Get frames from the resampling cache.
        /// </summary>
        public int GetCachedSamples(int fromIdx, int frameCount, Span<float> buffer)
        {
            lock (this)
            {
                if (_cache == null) return 0;

                // Check if requesting samples from the point at which the buffer is dirty.
                if (_cachePtrDirty != 0 && fromIdx < _cachePtrDirty)
                {
                    Engine.Log.Trace("Resample cache is dirty, will resample again.", MessageSource.Audio);

                    // Stop resample if running.
                    if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

                    // The lock will cause a wait until the resample task has ended.
                    // We don't want two tasks writing over the same memory.
                    lock (_resampleThreadLock)
                    {
                        _cancelResample = false;
                        _cachePtrDirty = 0;
                        _cachePtr = 0;
                        Reset();
                        _resampleTask = Task.Run(TaskResampleToCache);
                    }
                }

                // Check if there's enough samples in the cache.
                int channels = ConvFormat.Channels;
                int sampleEndIdx = Math.Min(fromIdx + frameCount * channels, ConvSamples);

                while (sampleEndIdx > _cachePtr)
                {
                    if (_resampleEmit.WaitOne(RESAMPLE_CACHE_TIMEOUT)) continue;
                    Engine.Log.Warning("Timed out while waiting for cache samples.", MessageSource.Audio);
                    return 0;
                }

                // Copy over the needed samples.
                int sampleCount = sampleEndIdx - fromIdx;
                new Span<float>(_cache).Slice(fromIdx, sampleCount).CopyTo(buffer);

                return sampleCount;
            }
        }

        /// <inheritdoc />
        public override void SetConvertFormat(AudioFormat dstFormat, int quality = 10, bool keepProgress = true)
        {
            lock (this)
            {
                // Stop resample if running.
                if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

                // The lock will cause a wait until the resample task has ended.
                lock (_resampleThreadLock)
                {
                    base.SetConvertFormat(dstFormat, quality, keepProgress);

                    // Resize/allocate cache if needed.
                    if (_cache == null)
                        _cache = new float[ConvSamples];
                    else if (_cache.Length < ConvSamples)
                        Array.Resize(ref _cache, ConvSamples);

                    // Start resampling thread.
                    _cancelResample = false;
                    _resampleTask = Task.Run(TaskResampleToCache);
                }
            }
        }

        /// <summary>
        /// Set the convert format, and restart caching from a certain point.
        /// </summary>
        /// <param name="dstFormat">The format to set.</param>
        /// <param name="progress">The progress to restart caching from.</param>
        /// <returns></returns>
        public int SetConvertFormatAndCacheFrom(AudioFormat dstFormat, float progress)
        {
            int restartIndex;
            lock (this)
            {
                if (ConvFormat != null && ConvFormat.Equals(dstFormat)) return (int) MathF.Floor(ConvSamples * progress);

                // Stop resample if running.
                if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

                // The lock will cause a wait until the resample task has ended.

                lock (_resampleThreadLock)
                {
                    base.SetConvertFormat(dstFormat, ConvQuality, false);
                    _cachePtr = 0;

                    // Restore resample thread progress from emit progress
                    // We don't want to wait for everything to be resampled from the very beginning.
                    restartIndex = (int) MathF.Floor(ConvSamples * progress);
                    FastForwardResample(ref _srcResume, ref _dstResume, restartIndex);
                    _cachePtr = _dstResume;
                    _cachePtrDirty = _cachePtr;

                    // Resize/allocate cache if needed.
                    if (_cache == null)
                        _cache = new float[ConvSamples];
                    else if (_cache.Length < ConvSamples)
                        Array.Resize(ref _cache, ConvSamples);

                    // Start resampling thread.
                    _cancelResample = false;
                    _resampleTask = Task.Run(TaskResampleToCache);
                }
            }


            return restartIndex;
        }

        /// <summary>
        /// Resample the track ahead of where it is requested. This allows for better performance.
        /// Maybe store the cache within the Audio Asset?
        /// The cache is not guaranteed to contain the full resampled track - if the resampling resumes.
        /// </summary>
        private void TaskResampleToCache()
        {
            if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Resample Thread";
            Engine.Log.Trace("Starting resample.", MessageSource.Audio);
            lock (_resampleThreadLock)
            {
                int framesGotten = -1;
                Span<float> dataSpan = _cache;
                while (framesGotten != 0 && _cachePtr != ConvSamples)
                {
                    framesGotten = base.GetNextFrames(RESAMPLE_INTERVAL, dataSpan.Slice(_cachePtr));
                    if (_cancelResample) return;
                    _cachePtr += framesGotten;
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