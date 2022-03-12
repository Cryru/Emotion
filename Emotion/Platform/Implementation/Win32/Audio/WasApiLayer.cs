#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiLayer : AudioLayer
    {
        private WasApiAudioAdapter _adapter;
        private WasApiLayerContext _layerContext;
        private int _bufferLengthInFrames;

        public WasApiLayer(WasApiAudioAdapter adapter, string name) : base(name)
        {
            _adapter = adapter;
            _adapter.OnDefaultDeviceChangedInternal += SetDevice;
            SetDevice(adapter.DefaultDevice);
        }

        public override void Dispose()
        {
            _adapter.OnDefaultDeviceChangedInternal -= SetDevice;
            _adapter = null;
            base.Dispose();
        }

        public override bool Update()
        {
            if (_layerContext != null) UpdateInternal();
            return base.Update();
        }

        private void UpdateInternal()
        {
            try
            {
                // Check if the context is initialized.
                if (!_layerContext.Initialized)
                {
                    // Fill buffer before starting to prevent noise.
                    FillBuffer(_layerContext.RenderClient, _bufferLengthInFrames);
                    _layerContext.Start();
                }

                // Start if not started.
                if (!_layerContext.Started) _layerContext.Start();

                // Get more frames.
                int error = _layerContext.AudioClient.GetCurrentPadding(out int padding);
                if (error != 0) Engine.Log.Warning($"Couldn't get device padding, error {error}.", MessageSource.WasApi);
                FillBuffer(_layerContext.RenderClient, _bufferLengthInFrames - padding);
            }
            catch (COMException ex)
            {
                // Audio device is the same, but the configuration has changed.
                // Tracking these changes in the adapter is a huge drag, so we just catch the error instead.
                // https://www.hresult.info/FACILITY_AUDCLNT/0x88890004
                if ((uint) ex.ErrorCode == 0x88890004)
                {
                    SetDevice(_adapter.DefaultDevice);
                    Engine.Log.Trace("Default audio device changed.", MessageSource.WasApi);
                }
                else
                {
                    Engine.Log.Error(ex.ToString(), MessageSource.WasApi);
                }
            }
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
            return framesGotten == 0; // This should only be true if the buffer was exactly exhausted.
        }

        /// <summary>
        /// Set the audio device the layer will output into.
        /// </summary>
        public void SetDevice(WasApiAudioDevice device)
        {
            if (device == null)
            {
                _layerContext = null;
                return;
            }

            _layerContext = device.CreateLayerContext(out uint bufferSize);
            _bufferLengthInFrames = (int) bufferSize;
        }
    }
}