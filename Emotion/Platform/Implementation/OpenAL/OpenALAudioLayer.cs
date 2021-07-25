#region Using

using System.Collections;
using System.Diagnostics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioLayer : AudioLayer
    {
        private const int BUFFER_COUNT = 2;

        private OpenALAudioAdapter _parent;
        private uint _source;

        private uint[] _buffers;
        private bool[] _bufferBusy;
        private int _currentBuffer;
        private Coroutine _layerRoutine;

        private static AudioFormat _openALAudioFormat = new AudioFormat(32, true, 2, 24000);
        private static int _openALFormatId;

        public OpenALAudioLayer(string name, OpenALAudioAdapter parent) : base(name)
        {
            if (_openALFormatId == 0)
            {
                _openALFormatId = _openALAudioFormat.BitsPerSample switch
                {
                    32 => Al.FORMAT_STEREO32F,
                    16 => _openALAudioFormat.Channels == 2 ? Al.FORMAT_STEREO16 : Al.FORMAT_MONO16,
                    8 => _openALAudioFormat.Channels == 2 ? Al.FORMAT_STEREO8 : Al.FORMAT_MONO8,
                    _ => _openALFormatId
                };
            }

            _parent = parent;
            Al.GenSource(out _source);

            _buffers = new uint[BUFFER_COUNT];
            _bufferBusy = new bool[BUFFER_COUNT];
            for (var i = 0; i < _buffers.Length; i++)
            {
                Al.GenBuffer(out _buffers[i]);
            }

            _layerRoutine = Engine.CoroutineManager.StartCoroutine(UpdateCoroutine());
        }

        private IEnumerator UpdateCoroutine()
        {
            int frameRequestSize = _openALAudioFormat.SampleRate / _openALAudioFormat.Channels;
            var dataHolder = new byte[frameRequestSize * _openALAudioFormat.FrameSize];
            var nativeArgs = new uint[1];

            while (Engine.Status == EngineStatus.Running)
            {
                yield return null;

                if (Status != PlaybackStatus.Playing)
                {
                    // No longer playing - dequeue all busy buffers.
                    bool anyBufferBusy;
                    do
                    {
                        anyBufferBusy = false;
                        for (var i = 0; i < _bufferBusy.Length; i++)
                        {
                            if (_bufferBusy[i])
                            {
                                anyBufferBusy = true;
                                break;
                            }
                        }

                        yield return DequeueBusyBuffers();
                    } while (anyBufferBusy);

                    SyncLayerAndALState();
                    continue;
                }

                int framesGotten = GetDataForCurrentTrack(_openALAudioFormat, frameRequestSize, dataHolder);
                if (framesGotten == 0) continue;
                int byteLength = dataHolder.Length;
                if (framesGotten < frameRequestSize) byteLength = framesGotten * _openALAudioFormat.FrameSize;

                // Naive implementation where OpenAL manages resources.
#if false
                uint buffer;
                Al.GetSourcei(_source, Al.BUFFERS_PROCESSED, out int buffersProcessed);
                switch (buffersProcessed)
                {
                    // No done buffers - gen new.
                    case 0:
                        Al.GenBuffer(out buffer);
                        break;
                    // One buffer freed - use it.
                    case 1:
                        Al.SourceUnqueueBuffers(_source, 1, nativeArgs);
                        buffer = nativeArgs[0];
                        break;
                    // More than one buffer freed. This is pretty rare.
                    default:
                        var removed = new uint[buffersProcessed];
                        Al.SourceUnqueueBuffers(_source, buffersProcessed, removed);
                        buffer = removed[0];
                        for (var i = 1; i < removed.Length; i++)
                        {
                            Al.DeleteBuffer(removed[i]);
                        }

                        break;
                }

                UploadDataToBuffer(dataHolder, buffer, byteLength);
                nativeArgs[0] = buffer;
                Al.SourceQueueBuffers(_source, 1, nativeArgs);

                SyncLayerAndALState();
#else
                uint buffer = _buffers[_currentBuffer];

                UploadDataToBuffer(dataHolder, buffer, byteLength);
                nativeArgs[0] = buffer;
                Al.SourceQueueBuffers(_source, 1, nativeArgs);
                _bufferBusy[_currentBuffer] = true;
                _currentBuffer++;
                if (_currentBuffer == _buffers.Length) _currentBuffer = 0;

                // Sync state and start playing only if data is queued.
                SyncLayerAndALState();

                if (!_bufferBusy[_currentBuffer]) continue;

                // Wait for the next buffer to free up.
                yield return DequeueBusyBuffers();
                Debug.Assert(!_bufferBusy[_currentBuffer]);
#endif
            }
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

        private IEnumerator DequeueBusyBuffers()
        {
            // Check if any buffers are queued.
            Al.GetSourcei(_source, Al.BUFFERS_QUEUED, out int queued);
            if (queued == 0) yield break;

            // Wait for any buffers to be processed.
            int buffersProcessed;
            do
            {
                Al.GetSourcei(_source, Al.BUFFERS_PROCESSED, out buffersProcessed);
                yield return null;
            } while (buffersProcessed == 0);

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

        protected override void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus)
        {
        }

        public override void Dispose()
        {
            if (_layerRoutine != null) Engine.CoroutineManager.StopCoroutine(_layerRoutine);
        }
    }
}