#region Using

using System.Threading;
using Emotion.Platform;

#endregion

namespace Emotion.Audio
{
    public abstract class ThreadedAudioAdapter : IAudioAdapter
    {
        private int INACTIVITY_TIMEOUT = 500;

        private PlatformBase _platform;

        protected ThreadedAudioAdapter(PlatformBase platform)
        {
            _platform = platform;
        }

        public AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            AudioLayer newAudioLayer = CreatePlatformAudioLayerInternal(layerName);
            var th = new Thread(LayerThread)
            {
                IsBackground = true,
            };
            th.Start(newAudioLayer);
            return newAudioLayer;
        }

        public virtual void Dispose()
        {
        }

        private void LayerThread(object layerObj)
        {
            var layer = (AudioLayer) layerObj;
            if (_platform?.NamedThreads ?? false) Thread.CurrentThread.Name ??= $"Audio Thread - {layer.Name}";

            var layerActivity = new AutoResetEvent(false);
            layer.OnTrackChanged += (o, n) => { layerActivity.Set(); };

            while (!layer.Disposed)
            {
                layer.Update();
                if (layer.Status != PlaybackStatus.Playing) layerActivity.WaitOne(INACTIVITY_TIMEOUT);
                Thread.Sleep(1);
            }
        }

        protected abstract AudioLayer CreatePlatformAudioLayerInternal(string layerName);
    }
}