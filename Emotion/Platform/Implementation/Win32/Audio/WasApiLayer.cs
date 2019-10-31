#region Using

using System;
using System.Diagnostics;
using System.Threading;
using Emotion.Audio;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    internal class WasApiLayer : AudioLayer
    {
        private WasApiAudioContext _parent;
        private Thread _thread;
        private bool _alive;
        private WasApiAudioDevice.LayerContext _layerContext;
        private volatile bool _updateDevice;

        private ManualResetEvent _playWait = new ManualResetEvent(false);

        public WasApiLayer(string name, WasApiAudioContext parent) : base(name)
        {
            _parent = parent;
            _alive = true;
            SetDevice(_parent.DefaultDevice);
            _thread = new Thread(LayerThread);
            _thread.Start();
            while (!_thread.IsAlive)
            {
            }
        }

        private void LayerThread()
        {
            if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = $"Audio Layer - {Name}";
            while (_alive && !Engine.Stopped)
            {
                // If not playing, wait for it to start playing.
                if (!Playing)
                    _playWait.WaitOne();

                if (_playlist.Count == 0 || _currentTrack == -1 || _currentTrack > _playlist.Count - 1) Debug.Assert(false);

                AudioTrack currentTrack = _playlist[_currentTrack];
                AudioStreamer streamer = currentTrack.Streamer;
                var frameCount = (int) _layerContext.BufferSize;

                // Ensure that the streamer outputs what the layer context expects.
                if (!_layerContext.AudioClientFormat.Equals(streamer.ConvFormat)) currentTrack.Streamer.SetConvertFormat(_layerContext.AudioClientFormat);

                // Check if the context is initialized.
                if (!_layerContext.Initialized)
                {
                    FillBuffer(_layerContext.RenderClient, ref streamer, (int) _layerContext.BufferSize);
                    _layerContext.Start();
                }

                if (!_layerContext.Started) _layerContext.Start();

                bool success = _layerContext.WaitHandle.WaitOne(_layerContext.TimeoutPeriod);
                if (!success)
                {
                    Engine.Log.Warning($"Layer {Name} audio context timeout.", MessageSource.Audio);
                    continue;
                }

                int error = _layerContext.AudioClient.GetCurrentPadding(out int padding);
                if (error != 0) Engine.Log.Warning($"Couldn't get device padding, error {error}.", MessageSource.Audio);

                if (!FillBuffer(_layerContext.RenderClient, ref streamer, frameCount - padding)) continue;
            }
        }

        /// <summary>
        /// Fill a render client buffer.
        /// </summary>
        /// <param name="client">The client to fill.</param>
        /// <param name="streamer">The buffer to fill from.</param>
        /// <param name="bufferFrameCount">The number of samples to fill with.</param>
        /// <returns>Whether the buffer has been read to the end.</returns>
        private unsafe bool FillBuffer(IAudioRenderClient client, ref AudioStreamer streamer, int bufferFrameCount)
        {
            if (bufferFrameCount == 0) return false;

            int error = client.GetBuffer(bufferFrameCount, out IntPtr bufferPtr);
            if (error != 0) Engine.Log.Warning($"Couldn't get device buffer, error {error}.", MessageSource.Audio);
            var buffer = new Span<byte>((void*) bufferPtr, bufferFrameCount * streamer.ConvFormat.SampleSize);
            int frames = streamer.GetNextFrames(bufferFrameCount, buffer);
            error = client.ReleaseBuffer(frames, frames == 0 ? AudioClientBufferFlags.Silent : AudioClientBufferFlags.None);
            if (error != 0) Engine.Log.Warning($"Couldn't release device buffer, error {error}.", MessageSource.Audio);
            return frames == 0;
        }

        private void SetDevice(WasApiAudioDevice device)
        {
            _layerContext = device.CreateLayerContext();
        }

        public void DefaultDeviceChanged(WasApiAudioDevice newDevice)
        {
            _updateDevice = true;
        }

        public override void Resume()
        {
            base.Resume();
            StartPlay();
        }

        public override void Pause()
        {
            base.Pause();
            StopPlay();
            _layerContext.Stop();
        }

        public override void Clear()
        {
            base.Clear();
            StopPlay();
            _layerContext.Reset();
        }

        public override void PlayNext(AudioAsset file)
        {
            base.PlayNext(file);
            StartPlay();
        }

        public override void Dispose()
        {
            base.Dispose();
            _alive = false;
        }

        private void StartPlay()
        {
            _playWait.Set();
        }

        private void StopPlay()
        {
            _playWait.Reset();
        }
    }
}