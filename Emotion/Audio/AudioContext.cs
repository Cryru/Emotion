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

        protected bool _running;
        protected Thread _audioThread;

        public static int MaxAudioAdvanceTime = 50;
        public static int MinAudioAdvanceTime = 10;

        protected Stopwatch _audioTimeTracker;
        protected long _lastTick;
        protected int _timeToProcess;
        protected AutoResetEvent _audioProcessEvent;

        protected AudioContext()
        {
            _audioTimeTracker = new Stopwatch();
            _audioProcessEvent = new AutoResetEvent(false);

            _running = true;
#if !WEB
            _audioThread = new Thread(AudioLayerProc)
            {
                IsBackground = true,
            };
            _audioThread.Start();
#endif
        }

        public virtual void Update()
        {
            if (!_audioTimeTracker.IsRunning) _audioTimeTracker.Start();

            long timeNow = _audioTimeTracker.ElapsedMilliseconds;
            var timePassed = (int) (timeNow - _lastTick);
            if (timePassed < MinAudioAdvanceTime) return;
            Interlocked.Add(ref _timeToProcess, timePassed);
            _lastTick = timeNow;
            _audioProcessEvent.Set();
        }

        public void AudioLayerProc()
        {
            if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Audio Thread";

            var layers = new List<AudioLayer>();
            while (_running)
            {
                _audioProcessEvent.WaitOne();

                while (!_toAdd.IsEmpty && _toAdd.TryTake(out AudioLayer layer))
                    layers.Add(layer);

                int timePassed = Interlocked.Exchange(ref _timeToProcess, 0);

                // Prevent spiral of death.
                if (timePassed > MaxAudioAdvanceTime) timePassed = MaxAudioAdvanceTime;

                for (var i = 0; i < layers.Count; i++)
                {
                    layers[i].ProcessAhead(timePassed);
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

            layer.Stop();
            layer.Dispose();

            // todo: _toRemove
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