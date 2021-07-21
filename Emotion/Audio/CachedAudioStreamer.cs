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
    public class CachedAudioStreamer : AudioStreamer
    {
        public static int RESAMPLE_CACHE_TIMEOUT = 1000;
        public static int RESAMPLE_INTERVAL_FRAMES = 4000;

        private float[] _cache;

        /// <summary>
        /// The point up to which the cache is filled.
        /// </summary>
        private int _cachePtr;

        /// <summary>
        /// The point up to which the cache is dirty.
        /// </summary>
        private int _cachePtrDirty;

        private Task _resampleTask;
        private object _resampleThreadLock = new();
        private bool _cancelResample;
        private AutoResetEvent _resampleEmit = new(false);
        private string _assetName;

        public CachedAudioStreamer(AudioAsset asset) : base(asset.Format, asset.SoundData)
        {
            _assetName = asset.Name;
        }

        /// <summary>
        /// Get frames from the resampling cache.
        /// </summary>
        public override int GetSamplesAt(int fromIdx, int frameCount, Span<float> buffer)
        {
            if (_cache == null) return 0;

            // Check if requesting samples from the point at which the buffer is dirty.
            if (_cachePtrDirty != 0 && fromIdx < _cachePtrDirty)
            {
                // Stop resample if running.
                if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

                // The lock will cause a wait until the old resample task has ended.
                // We don't want two tasks writing over the same memory.
                lock (_resampleThreadLock)
                {
                    _cancelResample = false;
                    _cachePtrDirty = fromIdx;
                    _cachePtr = fromIdx;
                    _resampleTask = Task.Run(TaskResampleToCache);
                }
            }

            // Check if there's enough samples in the cache.
            int channels = ConvFormat.Channels;
            int sampleEndIdx = Math.Min(fromIdx + frameCount * channels, ConvSamples);

            while (sampleEndIdx > _cachePtr)
            {
                if (_resampleEmit.WaitOne(RESAMPLE_CACHE_TIMEOUT)) continue;
                Engine.Log.Warning("Timed out while waiting for cache samples. Ending current track.", MessageSource.Audio);
                return 0;
            }

            // Copy over the needed samples.
            int sampleCount = sampleEndIdx - fromIdx;
            new Span<float>(_cache).Slice(fromIdx, sampleCount).CopyTo(buffer);

            return sampleCount;
        }

        /// <inheritdoc />
        public override void SetConvertFormat(AudioFormat dstFormat, int quality = 10)
        {
            // Stop resample if running.
            if (_resampleTask != null && !_resampleTask.IsCompleted) _cancelResample = true;

            // The lock will cause a wait until the resample task has ended.
            lock (_resampleThreadLock)
            {
                base.SetConvertFormat(dstFormat, quality);
                _cachePtrDirty = ConvSamples;

                // Resize/allocate cache if needed.
                if (_cache == null)
                    _cache = new float[ConvSamples];
                else if (_cache.Length < ConvSamples)
                    Array.Resize(ref _cache, ConvSamples);
            }
        }

        /// <summary>
        /// Resample the track ahead of where it is requested. This allows for better performance.
        /// Maybe store the cache within the Audio Asset?
        /// The cache is not guaranteed to contain the full resampled track - if the resampling resumes.
        /// </summary>
        private void TaskResampleToCache()
        {
            if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Resample Thread";
            Engine.Log.Trace($"Starting resample {_assetName} {_cachePtr}-{ConvSamples}.", MessageSource.Audio);
            int start = _cachePtr;
            lock (_resampleThreadLock)
            {
                int framesGotten = -1;
                Span<float> dataSpan = _cache;
                while (framesGotten != 0 && _cachePtr != ConvSamples)
                {
                    framesGotten = base.GetSamplesAt(_cachePtr, RESAMPLE_INTERVAL_FRAMES, dataSpan.Slice(_cachePtr));
                    if (_cancelResample) return;
                    _cachePtr += framesGotten;
                    _resampleEmit.Set();
                }
            }

            _cachePtrDirty = start;
            Engine.Log.Trace($"Resample finished {_assetName}", MessageSource.Audio);
        }
    }
}