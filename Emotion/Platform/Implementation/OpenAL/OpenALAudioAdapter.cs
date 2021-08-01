#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioAdapter : IAudioAdapter
    {
        public IntPtr AudioDevice { get; private set; }
        public IntPtr AudioContext { get; private set; }

        private PlatformBase _platform;
        private AutoResetEvent _layerActivityWait = new AutoResetEvent(false);
        private List<OpenALAudioLayer> _layers = new List<OpenALAudioLayer>();

        public static OpenALAudioAdapter TryCreate(PlatformBase platform)
        {
            var newCtx = new OpenALAudioAdapter(platform);
            newCtx.AudioDevice = Alc.OpenDevice(null);
            var attr = new int[0];
            newCtx.AudioContext = Alc.CreateContext(newCtx.AudioDevice, attr);
            if (newCtx.AudioDevice == IntPtr.Zero || newCtx.AudioContext == IntPtr.Zero)
            {
                Engine.Log.Error("Couldn't create OpenAL context.", MessageSource.OpenAL);
                return null;
            }

            bool success = Alc.MakeContextCurrent(newCtx.AudioContext);
            if (!success)
            {
                Engine.Log.Error("Couldn't make OpenAL context current.", MessageSource.OpenAL);
                return null;
            }

            return newCtx;
        }

        public OpenALAudioAdapter(PlatformBase platform)
        {
            _platform = platform;
            var thread = new Thread(LayerThread)
            {
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            thread.Start();
            while (!thread.IsAlive)
            {
            }
        }

        public AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            var newLayer = new OpenALAudioLayer(layerName, this);
            _layers.Add(newLayer);
            newLayer.OnTrackChanged += TrackChanged;
            return newLayer;
        }

        private void TrackChanged(AudioAsset oldTrack, AudioAsset newTrack)
        {
            _layerActivityWait.Set();
        }

        private void LayerThread()
        {
            if (_platform?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Audio Thread";
            while (Engine.Status != EngineStatus.Stopped)
            {
                var anyLayersPlaying = false;
                for (var i = 0; i < _layers.Count; i++)
                {
                    OpenALAudioLayer layer = _layers[i];
                    if (layer == null) continue;
                    if (layer.Disposed)
                    {
                        _layers[i] = null;
                        continue;
                    }

                    layer.ProcUpdate();
                    anyLayersPlaying = anyLayersPlaying || layer.Status == PlaybackStatus.Playing;
                }

                // If no layers are playing, sleep to prevent CPU usage.
                if (!anyLayersPlaying)
                    _layerActivityWait.WaitOne(200);
                Thread.Yield();
            }
        }
    }
}