#region Using

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Platform;

#endregion

namespace Emotion.Audio
{
    public abstract class ThreadedAudioAdapter : IAudioAdapter
    {
        private int INACTIVITY_TIMEOUT = 500;

        private List<Task> _layerThreads = new List<Task>();
        private PlatformBase _platform;

        protected ThreadedAudioAdapter(PlatformBase platform)
        {
            _platform = platform;
        }

        public AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            AudioLayer newAudioLayer = CreatePlatformAudioLayerInternal(layerName);
            _layerThreads.Add(Task.Run(() => LayerThread(newAudioLayer)));
            return newAudioLayer;
        }

        private void LayerThread(AudioLayer layer)
        {
            if (_platform?.NamedThreads ?? false) Thread.CurrentThread.Name ??= $"Audio Thread - {layer.Name}";

            var layerActivity = new AutoResetEvent(false);
            layer.OnTrackChanged += (o, n) => { layerActivity.Set(); };

            while (!layer.Disposed)
            {
                UpdateLayer(layer);
                if (layer.Status != PlaybackStatus.Playing) layerActivity.WaitOne(INACTIVITY_TIMEOUT);
                Task.Delay(1).Wait();
            }
        }

        protected abstract AudioLayer CreatePlatformAudioLayerInternal(string layerName);
        protected abstract void UpdateLayer(AudioLayer layer);
    }
}