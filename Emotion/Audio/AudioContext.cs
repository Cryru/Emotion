#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Emotion.Common;
using Emotion.Platform;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Audio
{
    public abstract class AudioContext
    {
        protected List<AudioLayer> _layerMapping = new();
        protected ConcurrentBag<AudioLayer> _toRemove = new();
        protected ConcurrentBag<AudioLayer> _toAdd = new();

        protected volatile bool _running;
        protected Thread _audioThread;

        /// <summary>
        /// How much audio will get resampled at once (for each layer)
        /// </summary>
        public static int MaxAudioAdvanceTime = 50;

        /// <summary>
        /// The rate at which audio layers are updated. This is also the amount of time
        /// worth in audio samples that will be buffered on each tick (roughly).
        /// Buffered date will be roughly this much times 2 ahead (depending on how much the buffer consumes),
        /// which is essentially the audio lag.
        /// </summary>
        public static int AudioUpdateRate = 25;

        /// <summary>
        /// How many ms the backend buffer is expected to be. This number should be 100ms+ to prevent audio flickering.
        /// </summary>
        public static int BackendBufferExpectedAhead = MaxAudioAdvanceTime * 4;

        private PlatformBase _host;

        protected AudioContext(PlatformBase platform)
        {
            _host = platform;
            _running = true;
#if !WEB
            _audioThread = new Thread(AudioLayerProc)
            {
                IsBackground = true,
            };
            _audioThread.Start();
#endif
        }

        public virtual void AudioLayerProc()
        {
            if (_host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Audio Thread";

            var audioTimeTracker = Stopwatch.StartNew();
            long lastTick = 0;
            var audioProcessEvent = new AutoResetEvent(false);

            var layers = new List<AudioLayer>();
            while (_running)
            {
                audioProcessEvent.WaitOne(AudioUpdateRate);
                if (!_running) break;

                long timeNow = audioTimeTracker.ElapsedMilliseconds;
                var tickPassed = (int) (timeNow - lastTick);
                lastTick = timeNow;

                // Handle changes
                while (!_toAdd.IsEmpty && _toAdd.TryTake(out AudioLayer layer))
                    layers.Add(layer);

                while (!_toRemove.IsEmpty && _toRemove.TryTake(out AudioLayer layer))
                {
                    layers.Remove(layer);
                    layer.Stop();
                    layer.Dispose();
                }

                // Prevent spiral of death.
                if (tickPassed > MaxAudioAdvanceTime) tickPassed = MaxAudioAdvanceTime;

                for (var i = 0; i < layers.Count; i++)
                {
                    layers[i].Update(tickPassed);
                }
            }
        }

        public AudioLayer CreateLayer(string layerName, float layerVolume = 1)
        {
            layerName = layerName.ToLower();
            AudioLayer newLayer = CreatePlatformAudioLayer(layerName);
            newLayer.Volume = layerVolume;

            _toAdd.Add(newLayer);
            _layerMapping.Add(newLayer);

            Engine.Log.Info($"Created audio layer {newLayer.Name}", MessageSource.Audio);
            return newLayer;
        }

        public abstract AudioLayer CreatePlatformAudioLayer(string layerName);

        public void RemoveLayer(string layerName)
        {
            layerName = layerName.ToLower();
            AudioLayer layer = GetLayer(layerName);
            if (layer == null) return;

            _toRemove.Add(layer);
            _layerMapping.Remove(layer);

            Engine.Log.Info($"Removed audio layer {layer.Name}", MessageSource.Audio);
        }

        public AudioLayer GetLayer(string layerName)
        {
            layerName = layerName.ToLower();
            for (var i = 0; i < _layerMapping.Count; i++)
            {
                AudioLayer layer = _layerMapping[i];
                if (layer.Name == layerName) return layer;
            }

            return null;
        }

        public string[] GetLayers()
        {
            int entries = _layerMapping.Count;
            var names = new string[entries];
            for (var i = 0; i < entries; i++)
            {
                names[i] = _layerMapping[i].Name;
            }

            return names;
        }

        public virtual void Dispose()
        {
            _running = false;
        }
    }
}