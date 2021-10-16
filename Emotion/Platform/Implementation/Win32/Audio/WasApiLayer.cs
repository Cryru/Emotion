#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    internal class WasApiLayer : AudioLayer
    {
        private WasApiAudioDevice _device;
        private WasApiLayerContext _layerContext;
        private int _bufferLengthInFrames;

        public WasApiLayer(string name) : base(name)
        {
        }

        public void ProcUpdate(WasApiAudioDevice device)
        {
            // Check if the device has changed.
            if (_device != device) SetDevice(device);

            // If not playing, wait for it to start playing.
            if (Status != PlaybackStatus.Playing) return;

            try
            {
                // Check if the context is initialized.
                if (!_layerContext.Initialized)
                {
                    FillBuffer(_layerContext.RenderClient, (int) _layerContext.BufferSize);
                    _layerContext.Start();
                }

                // Start if not started.
                if (!_layerContext.Started) _layerContext.Start();

                // Get more frames.
                int error = _layerContext.AudioClient.GetCurrentPadding(out int padding);
                if (error != 0) Engine.Log.Warning($"Couldn't get device padding, error {error}.", MessageSource.WasApi);
                bool empty = FillBuffer(_layerContext.RenderClient, _bufferLengthInFrames - padding);
                //Debug.Assert(!empty);
            }
            catch (COMException ex)
            {
                // Audio device has disappeared or whatever.
                if ((uint) ex.ErrorCode == 0x88890004)
                {
                    SetDevice(device);
                    Engine.Log.Info("Default audio device changed.", MessageSource.WasApi);
                }

                Engine.Log.Error(ex.ToString(), MessageSource.WasApi);
            }

            Update();
        }

        /// <summary>
        /// Fill a render client buffer.
        /// </summary>
        /// <param name="client">The client to fill.</param>
        /// <param name="getFrames">The number of samples to fill with.</param>
        /// <returns>Whether the buffer has been read to the end.</returns>
        private unsafe bool FillBuffer(IAudioRenderClient client, int getFrames)
        {
            if (getFrames == 0) return false;

            int error = client.GetBuffer(getFrames, out IntPtr bufferPtr);
            if (error != 0) Engine.Log.Warning($"Couldn't get device buffer, error {error}.", MessageSource.WasApi);
            var buffer = new Span<byte>((void*) bufferPtr, getFrames * _layerContext.AudioClientFormat.FrameSize);

            int framesGotten = GetDataForCurrentTrack(_layerContext.AudioClientFormat, getFrames, buffer);
            error = client.ReleaseBuffer(framesGotten, framesGotten == 0 ? AudioClientBufferFlags.Silent : AudioClientBufferFlags.None);
            if (error != 0) Engine.Log.Warning($"Couldn't release device buffer, error {error}.", MessageSource.WasApi);
            return framesGotten == 0;
        }

        private void SetDevice(WasApiAudioDevice device)
        {
            _device = device;
            _layerContext = device.CreateLayerContext();
            _bufferLengthInFrames = (int) _layerContext.BufferSize;
        }
    }
}