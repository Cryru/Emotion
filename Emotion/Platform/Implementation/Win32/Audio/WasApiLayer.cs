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
        public bool Disposed { get; protected set; }

        private WasApiAudioDevice _device;
        private WasApiLayerContext _layerContext;
        private int _bufferLengthInFrames;

        // Double buffering.
        private const int BUFFER_COUNT = 2;
        private const bool ENABLE_DB = true;
        private byte[][] _doubleBuffer = new byte[BUFFER_COUNT][];
        private int[] _dbOffset = new int[BUFFER_COUNT];
        private int[] _dbFramesStored = new int[BUFFER_COUNT];
        private int _bufferIdx;

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
                Debug.Assert(!empty);
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

            // Fill all buffers that are out of frames.
            if (_doubleBuffer[0] != null)
                for (var i = 0; i < _doubleBuffer.Length; i++)
                {
                    byte[] buffer = _doubleBuffer[i];
                    int framesStored = _dbFramesStored[i];
                    if (framesStored != 0) continue;
                    _dbOffset[i] = 0;
                    _dbFramesStored[i] = GetDataForCurrentTrack(_layerContext.AudioClientFormat, _bufferLengthInFrames, buffer);
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

            // Try to get data from double buffering.
            var framesGotten = 0;
            if (_doubleBuffer[_bufferIdx] != null)
                while (getFrames > 0)
                {
                    int framesStored = _dbFramesStored[_bufferIdx];
                    if (framesStored == 0) break; // Reached empty buffer.

                    int framesTake = Math.Min(framesStored, getFrames);
                    getFrames -= framesTake; // Mark frames as gotten.
                    framesGotten += framesTake;

                    // Copy from db to dst.
                    int bufferCopyOffset = _dbOffset[_bufferIdx];
                    int bufferCopyLength = framesTake * _layerContext.AudioClientFormat.FrameSize;
                    new Span<byte>(_doubleBuffer[_bufferIdx]).Slice(bufferCopyOffset, bufferCopyLength).CopyTo(buffer);
                    buffer = buffer[bufferCopyLength..]; // Resize dest buffer.

                    // Mark storage and buffer metadata.
                    _dbOffset[_bufferIdx] += bufferCopyLength;
                    framesStored -= framesTake;
                    _dbFramesStored[_bufferIdx] = framesStored;
                    if (framesStored != 0) continue;
                    _bufferIdx++;
                    if (_bufferIdx == _dbFramesStored.Length) _bufferIdx = 0;
                }

            // If any frames still need to be gotten, hit layer.
            if (getFrames > 0)
            {
                int frames = GetDataForCurrentTrack(_layerContext.AudioClientFormat, getFrames, buffer);
                framesGotten += frames;
            }

            error = client.ReleaseBuffer(framesGotten, framesGotten == 0 ? AudioClientBufferFlags.Silent : AudioClientBufferFlags.None);
            if (error != 0) Engine.Log.Warning($"Couldn't release device buffer, error {error}.", MessageSource.WasApi);
            return framesGotten == 0;
        }

        protected override void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus)
        {
        }

        private void SetDevice(WasApiAudioDevice device)
        {
            _device = device;
            _layerContext = device.CreateLayerContext();
            _bufferLengthInFrames = (int) _layerContext.BufferSize;

            // Reset double buffering.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (ENABLE_DB)
            {
                _bufferIdx = 0;
                for (var i = 0; i < _doubleBuffer.Length; i++)
                {
                    _doubleBuffer[i] = new byte[_bufferLengthInFrames * _layerContext.AudioClientFormat.FrameSize];
                    _dbOffset[i] = 0;
                    _dbFramesStored[i] = 0;
                }
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Disposed = true;
        }
    }
}