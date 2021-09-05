#region Using

using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioLayer : AudioLayer
    {
        private const int FRAME_REQUEST_SIZE = 1000;
        private const int BUFFER_COUNT = 2;

        private OpenALAudioAdapter _parent;
        private uint _source;

        private uint[] _buffers;
        private bool[] _bufferBusy;
        private int _currentBuffer;
        private byte[] _dataHolder;

        private static AudioFormat _openALAudioFormat = new AudioFormat(32, true, 2, 24000);
        private static int _openALFormatId;

        public OpenALAudioLayer(string name, OpenALAudioAdapter parent) : base(name)
        {
            if (_openALFormatId == 0)
                _openALFormatId = _openALAudioFormat.BitsPerSample switch
                {
                    32 => Al.FORMAT_STEREO32F,
                    16 => _openALAudioFormat.Channels == 2 ? Al.FORMAT_STEREO16 : Al.FORMAT_MONO16,
                    8 => _openALAudioFormat.Channels == 2 ? Al.FORMAT_STEREO8 : Al.FORMAT_MONO8,
                    _ => _openALFormatId
                };

            _parent = parent;
            Al.GenSource(out _source);

            _buffers = new uint[BUFFER_COUNT];
            _bufferBusy = new bool[BUFFER_COUNT];
            for (var i = 0; i < _buffers.Length; i++)
            {
                Al.GenBuffer(out _buffers[i]);
            }

            _dataHolder = new byte[FRAME_REQUEST_SIZE * _openALAudioFormat.FrameSize];
        }

        public unsafe void ProcUpdate()
        {
            if (Status != PlaybackStatus.Playing)
            {
                // No longer playing - dequeue all busy buffers.
                var anyBufferBusy = false;
                for (var i = 0; i < _bufferBusy.Length; i++)
                {
                    if (!_bufferBusy[i]) continue;
                    anyBufferBusy = true;
                    break;
                }

                if (anyBufferBusy) DequeueBusyBuffers();
                SyncLayerAndALState();
            }

            // Check if the current buffer is busy. Try to free it.
            if (_bufferBusy[_currentBuffer]) DequeueBusyBuffers();
            if (!_bufferBusy[_currentBuffer])
            {
                // Try getting frames for this buffer.
                int framesGotten = GetDataForCurrentTrack(_openALAudioFormat, FRAME_REQUEST_SIZE, _dataHolder);
                if (framesGotten != 0)
                {
                    int byteLength = _dataHolder.Length;
                    if (framesGotten < FRAME_REQUEST_SIZE) byteLength = framesGotten * _openALAudioFormat.FrameSize;

                    uint buffer = _buffers[_currentBuffer];
                    UploadDataToBuffer(_dataHolder, buffer, byteLength);
                    Al.SourceQueueBuffers(_source, 1, &buffer);
                    _bufferBusy[_currentBuffer] = true;
                    _currentBuffer++;
                    if (_currentBuffer == _buffers.Length) _currentBuffer = 0;
                }
            }

            // Sync state and start playing only if data is queued.
            SyncLayerAndALState();
            Update();
        }

        /// <summary>
        /// For some reason you can't use unsafe code in coroutines :)
        /// </summary>
        private void UploadDataToBuffer(byte[] data, uint buffer, int byteLengthPerSampleChannel)
        {
            Al.BufferData(buffer, _openALFormatId, data, byteLengthPerSampleChannel, _openALAudioFormat.SampleRate);
            CheckALError();
        }

        private void CheckALError()
        {
            int error = Alc.GetError(_parent.AudioDevice);
            if (error != Al.NO_ERROR) Engine.Log.Warning($"OpenAL error {error}", MessageSource.OpenAL);
        }

        private void SyncLayerAndALState()
        {
            Al.GetSourcei(_source, Al.SOURCE_STATE, out int status);
            if (Status == PlaybackStatus.Playing && status != Al.PLAYING) Al.SourcePlay(_source);
            if (Status == PlaybackStatus.NotPlaying && status == Al.PLAYING) Al.SourceStop(_source);
            CheckALError();
        }

        private void DequeueBusyBuffers()
        {
            // Check if any are done.
            Al.GetSourcei(_source, Al.BUFFERS_PROCESSED, out int buffersProcessed);
            if (buffersProcessed == 0) return;

            // Dequeue and mark as free.
            var removed = new uint[buffersProcessed];
            Al.SourceUnqueueBuffers(_source, buffersProcessed, removed);
            for (var i = 0; i < buffersProcessed; i++)
            {
                uint bufferId = removed[i];
                for (var bIdx = 0; bIdx < _buffers.Length; bIdx++)
                {
                    if (_buffers[bIdx] == bufferId) _bufferBusy[bIdx] = false;
                }
            }
        }
    }
}