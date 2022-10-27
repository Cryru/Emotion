#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Audio
{
    /// <summary>
    /// Object for internal reference by the audio implementation of the platform.
    /// </summary>
    public abstract partial class AudioLayer
    {
        /// <summary>
        /// The layer's friendly name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The layer's volume. 0-1
        /// </summary>
        public float Volume { get; set; } = 1;

        /// <summary>
        /// Whether the layer is destroyed.
        /// </summary>
        public bool Disposed { get; protected set; }

        /// <summary>
        /// The status of the audio layer.
        /// </summary>
        public PlaybackStatus Status { get; protected set; } = PlaybackStatus.NotPlaying;

        /// <summary>
        /// Whether the current track is being looped.
        /// </summary>
        public bool LoopingCurrent { get; set; }

        /// <summary>
        /// The track currently playing - if any.
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public AudioTrack? CurrentTrack
        {
            get => _currentTrack;
        }

        /// <summary>
        /// What percentage (0-1) of the track has finished playing.
        /// </summary>
        public float Progress
        {
            get
            {
                if (_playHead == 0 || _updateCurrentTrack) return 0f;
                return (float) _playHead / _totalSamplesConv;
            }
        }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get
            {
                if (_currentTrack == null) return 0;
                return Progress * _currentTrack.File.Duration;
            }
        }

        /// <summary>
        /// The current playlist.
        /// Do not use this except for debugging and such.
        /// To get the current track use "CurrentTrack"
        /// </summary>
        public AudioAsset[] Playlist
        {
            get
            {
                lock (_playlist)
                {
                    return _playlist.Select(x => x.File).ToArray();
                }
            }
        }

        /// <summary>
        /// Called when the current track loops.
        /// The input parameter is the track which looped.
        /// </summary>
        public event Action<AudioAsset>? OnTrackLoop;

        /// <summary>
        /// Called when the current track changes.
        /// The first parameter is the old track, the second is the new one.
        /// If there is no further track the new track parameter will be null.
        /// </summary>
        public event Action<AudioAsset?, AudioAsset?>? OnTrackChanged;

        // For fade effects volume is calculated per chunk of frames, rather than for every frame.
        private const int VOLUME_MODULATION_FRAME_GRANULARITY = 100;
        private const int INITIAL_INTERNAL_BUFFER_SIZE = 4000;

        protected List<AudioTrack> _playlist = new(); // Always read and written in locks
        protected float[] _internalBuffer; // Internal memory to keep resamples frames for applying post proc to.
        protected int _playHead; // The progress into the current track, relative to the converted format.
        protected int _totalSamplesConv; // The total sample count in the dst format. Used to know where _playHead is relative to the total.
        protected float[] _internalBufferCrossFade; // Same memory, but for the second track when crossfading.
        protected int _crossFadePlayHead; // Same progress tracking, but for the second track when crossfading.
        protected int _loopCount; // Number of times the current track has looped. 0 if it hasn't.
        protected PlaybackStatus? _pendingStatus;
        protected AudioFormat _streamingFormat;

        protected bool _updateCurrentTrack = true;
        private AudioTrack? _currentTrack;
        private AudioTrack? _nextTrack;

        protected AudioLayer(string name)
        {
            Name = name;
            _internalBuffer = new float[INITIAL_INTERNAL_BUFFER_SIZE];
            _internalBufferCrossFade = new float[INITIAL_INTERNAL_BUFFER_SIZE];

            _streamingFormat = new AudioFormat();
        }

        #region API

        /// <summary>
        /// Sets the track to be played next in the playlist. If the playlist is empty and the layer isn't paused the track is
        /// played immediately.
        /// </summary>
        /// <param name="track">The track to play next.</param>
        public void PlayNext(AudioTrack track)
        {
            if (track.File == null) return;

            lock (_playlist)
            {
                if (_playlist.Count == 0)
                    _playlist.Add(track);
                else
                    _playlist.Insert(1, track);
            }

            InvalidateCurrentTrack();
        }

        /// <inheritdoc cref="PlayNext(AudioTrack)" />
        public void PlayNext(AudioAsset file)
        {
            PlayNext(new AudioTrack(file));
        }

        /// <summary>
        /// Adds the track to the back of the playlist. If the playlist is empty and the layer isn't paused the track is played
        /// immediately.
        /// </summary>
        /// <param name="track">The track to play.</param>
        public void AddToQueue(AudioTrack track)
        {
            if (track.File == null) return;

            lock (_playlist)
            {
                _playlist.Add(track);
            }

            InvalidateCurrentTrack();
        }

        /// <inheritdoc cref="PlayNext(AudioAsset)" />
        public void AddToQueue(AudioAsset file)
        {
            AddToQueue(new AudioTrack(file));
        }

        /// <summary>
        /// Stop all previous playback, clear the playlist, and play the provided track.
        /// This is essentially the same as calling Stop and then PlayNext but causes less state transitions and doesn't involve
        /// the platform.
        /// </summary>
        public void QuickPlay(AudioTrack track)
        {
            if (track.File == null) return;

            lock (_playlist)
            {
                _playlist.Clear();
                _playlist.Add(track);
            }

            InvalidateCurrentTrack();
        }

        /// <inheritdoc cref="PlayNext(AudioAsset)" />
        public void QuickPlay(AudioAsset file)
        {
            QuickPlay(new AudioTrack(file));
        }

        /// <summary>
        /// Resume playback, if paused. If currently playing nothing happens. If currently not playing - start playing.
        /// </summary>
        public void Resume()
        {
            _pendingStatus = PlaybackStatus.Playing;
        }

        /// <summary>
        /// Pause playback. If currently not playing anything the layer is paused anyway, and will need to be resumed.
        /// </summary>
        public void Pause()
        {
            _pendingStatus = PlaybackStatus.Paused;
        }

        /// <summary>
        /// Stop all playback, and clear the playlist.
        /// </summary>
        public void Stop()
        {
            lock (_playlist)
            {
                _playlist.Clear();
            }

            InvalidateCurrentTrack();
        }

        #endregion

        #region Stream Logic

        /// <summary>
        /// The max number of blocks to sample ahead.
        /// Each block is between MinAudioAdvanceTime and MaxAudioAdvanceTime (AudioContext) of audio data
        /// But if too small, then blocks will be overriden before you hear them.
        /// </summary>
        public static int MaxDataBlocks = 20;

        /// <summary>
        /// If the number of ready blocks is less than this, then the next block will
        /// contain twice as much data to ensure streaming is ahead of the backend.
        /// </summary>
        public static int BlocksMinAhead = 1;

        /// <summary>
        /// Total bytes allocated for all data blocks. Shared between audio layers.
        /// </summary>
        public static int MetricAllocatedDataBlocks;

        /// <summary>
        /// How many frames were requested that couldn't be served.
        /// </summary>
        public int MetricDataStoredInBlocks;

        /// <summary>
        /// How many frames were dropped because the backend didn't request them.
        /// </summary>
        public int MetricBackendMissedFrames;

        private static ObjectPool<AudioDataBlock> _dataPool = new ObjectPool<AudioDataBlock>(null, MaxDataBlocks);
        private Queue<AudioDataBlock> _readyBlocks = new Queue<AudioDataBlock>();

        // Metrics
        private Stopwatch? _updateTimer;

        private class AudioDataBlock
        {
            public byte[] Data = Array.Empty<byte>();
            public int FramesWritten;
            public int FramesRead;
        }

        /// <summary>
        /// Process audio data ahead.
        /// </summary>
        /// <param name="timePassed">Time to process, in milliseconds</param>
        public void Update(int timePassed)
        {
            // Pause sound if host is paused.
            if (Engine.Host != null && Engine.Host.HostPaused)
            {
                if (GLThread.IsGLThread()) return; // Don't stop main thread.
                Engine.Host.HostPausedWaiter.WaitOne();
            }

            UpdateCurrentTrack();

            if (Status != PlaybackStatus.Playing || _currentTrack == null) return;

            // Update the backend. This will cause it to call BackendGetData
            UpdateBackend();

            if (Status != PlaybackStatus.Playing || _currentTrack == null) return;

            // Buffer data in advance, so the next update can be as fast as possible (just a copy).
            BufferDataInAdvance(timePassed);

#if DEBUG
            _updateTimer ??= Stopwatch.StartNew();

            if (_updateTimer.ElapsedMilliseconds > 100)
            {
                MetricDataStoredInBlocks = 0;
                foreach (AudioDataBlock block in _readyBlocks)
                {
                    MetricDataStoredInBlocks += block.FramesWritten - block.FramesRead;
                }

                MetricDataStoredInBlocks = (int) (_streamingFormat.GetSoundDuration(MetricDataStoredInBlocks * _streamingFormat.FrameSize) * 1000);
                _updateTimer.Restart();
            }
#endif
        }

        private void InvalidateCurrentTrack()
        {
            _updateCurrentTrack = true;
        }

        private void UpdateCurrentTrack()
        {
            if (_pendingStatus != null)
            {
                Status = _pendingStatus.Value;
                _pendingStatus = null;

                if (!_updateCurrentTrack && Status == PlaybackStatus.Playing && _currentTrack == null) Status = PlaybackStatus.NotPlaying;
            }

            if (!_updateCurrentTrack) return;
            _updateCurrentTrack = false;

            // Get the currently playing track.
            AudioTrack? currentTrack;
            AudioTrack? nextTrack;
            lock (_playlist)
            {
                if (_playlist.Count == 0)
                {
                    currentTrack = null;
                    nextTrack = null;
                    if (Status == PlaybackStatus.Playing) Status = PlaybackStatus.NotPlaying;
                }
                else
                {
                    currentTrack = _playlist[0];

                    if (LoopingCurrent)
                        nextTrack = currentTrack;
                    else if (_playlist.Count > 1)
                        nextTrack = _playlist[1];
                    else
                        nextTrack = null;
                }
            }

            bool currentChanged = _currentTrack != currentTrack;
            bool nextChanged = _nextTrack != nextTrack;
            if (currentChanged || nextChanged) InvalidateAudioBlocks();

            // If both changed...
            if (currentChanged && (nextChanged || nextTrack == null))
            {
                // And the current is the next, we assume this was the track we were crossfading into
                _playHead = currentTrack == _nextTrack ? _crossFadePlayHead : 0;
                _crossFadePlayHead = 0;
            }

            if (currentChanged && currentTrack != null)
            {
                AudioConverter converter = currentTrack.File.AudioConverter;
                _totalSamplesConv = converter.GetSampleCountInFormat(_streamingFormat);
                if (currentTrack.SetLoopingCurrent) LoopingCurrent = true;
                if (Status == PlaybackStatus.NotPlaying) Status = PlaybackStatus.Playing;
            }

            if (currentChanged)
            {
                OnTrackChanged?.Invoke(_currentTrack?.File, currentTrack?.File);
            }

            _currentTrack = currentTrack;
            _nextTrack = nextTrack;
        }

        private void BufferDataInAdvance(int timePassed)
        {
            // Convert time to frames and bytes.
            int framesToGet = _streamingFormat.GetFrameCount(timePassed / 1000f);
            int bytesToGet = framesToGet * _streamingFormat.FrameSize;
            if (bytesToGet == 0) return;

            // Get a data block to fill. If we're over the maximum blocks this might mean overriding one that was ready.
            // This will happen if the backend is lagging behind, which should never happen?
            AudioDataBlock dataBlock;
            if (_readyBlocks.Count >= MaxDataBlocks && _readyBlocks.TryDequeue(out AudioDataBlock? b))
            {
                MetricBackendMissedFrames += b.FramesWritten - b.FramesRead;
                dataBlock = b;
            }
            else
            {
                dataBlock = _dataPool.Get();
            }

            // Ensure the data block can fit the requested data.
            // The data a data block will usually carry is around AudioUpdateRate * 2 in time.
            if (dataBlock.Data.Length < bytesToGet)
            {
                Interlocked.Add(ref MetricAllocatedDataBlocks, -dataBlock.Data.Length + bytesToGet); // metric shared between threads (layers)
                Array.Resize(ref dataBlock.Data, bytesToGet);
            }

            int framesGotten = GetDataToBuffer(framesToGet, new Span<byte>(dataBlock.Data, 0, bytesToGet));

            // Nothing streamed, track is probably over.
            if (framesGotten == 0) _dataPool.Return(dataBlock);

            // Reset data block trackers so it can be used.
            dataBlock.FramesWritten = framesGotten;
            dataBlock.FramesRead = 0;
            _readyBlocks.Enqueue(dataBlock);
        }

        protected int GetDataToBuffer(int framesRequested, Span<byte> dest, int framesOffset = 0)
        {
            // Check if the internal buffer needs to be resized to fit in the request.
            int samplesRequested = framesRequested * _streamingFormat.Channels;
            if (_internalBuffer.Length < samplesRequested)
            {
                Array.Resize(ref _internalBuffer, samplesRequested);
                Array.Resize(ref _internalBufferCrossFade, samplesRequested);
            }

#if DEBUG
            // Verify that the number of samples will fit in the buffer.
            // This should never happen and is considered a backend error.
            int bytesNeeded = framesRequested * _streamingFormat.FrameSize;
            if (dest.Length < bytesNeeded)
            {
                Engine.Log.Warning($"The provided buffer to the audio streamer is of invalid size {dest.Length} while {bytesNeeded} were requested.", MessageSource.Audio);
                framesRequested = dest.Length / _streamingFormat.FrameSize;
            }
#endif

            // Get post processed 32f buffer data.
            int framesOutput = GetProcessedFramesFromTrack(_streamingFormat, _currentTrack, framesRequested, _internalBuffer, ref _playHead);

            // Fill destination buffer in destination sample size format.
            Span<byte> destBuffer = dest.Slice(framesOffset * _streamingFormat.FrameSize);
            var srcBuffer = new Span<float>(_internalBuffer, 0, samplesRequested);
            AudioConverter.SetBufferOfSamplesAsFloat(srcBuffer, destBuffer, _streamingFormat);

            // Check if the buffer was filled.
            if (framesOutput == framesRequested) return framesOutput;

            // If less frames were drawn than the buffer can take - the track is over.

            // Check if looping.
            if (LoopingCurrent)
            {
                // Manually update playhead as track wont change.
                _playHead = _crossFadePlayHead;
                _crossFadePlayHead = 0;
                _loopCount++;
                OnTrackLoop?.Invoke(_currentTrack!.File);
            }
            // Otherwise, go to next track.
            else
            {
                _loopCount = 0;
                lock (_playlist)
                {
                    _playlist.Remove(_currentTrack!); // Don't remove as index as it is possible for current track to not be first via data race.
                }

                InvalidateCurrentTrack();
                UpdateCurrentTrack();
                
            }

            // No next track, these are all the frames you're gonna get.
            if (_currentTrack == null) return framesOutput;

            // Fill rest of buffer with samples from the next track.
            framesOutput += GetDataToBuffer(framesRequested - framesOutput, dest, framesOutput);

            return framesOutput;
        }

        protected abstract void UpdateBackend();

        protected int BackendGetData(AudioFormat format, int getFrames, Span<byte> buffer)
        {
            // Pause sound if host is paused.
            if (Engine.Host != null && Engine.Host.HostPaused)
            {
                if (GLThread.IsGLThread()) return 0; // Don't stop main thread.
                Engine.Host.HostPausedWaiter.WaitOne();
            }

            if (Status != PlaybackStatus.Playing) return 0;

            // Make sure we're getting the samples in the format we think we are.
            if (!format.Equals(_streamingFormat))
            {
                float oldCrossfadeProgress = _crossFadePlayHead != 0 ? _crossFadePlayHead / _totalSamplesConv : 0;
                float progress = _playHead != 0 ? (float) _playHead / _totalSamplesConv : 0;
                _streamingFormat = format;

                if (format.UnsupportedBitsPerSample())
                    Engine.Log.Warning($"Unsupported bits per sample format by streaming format - {_streamingFormat}", MessageSource.Audio, true);

                // Continue from last streaming position. This could cause a jump
                // depending on where the backend is.
                if (_currentTrack != null)
                {
                    AudioConverter streamer = _currentTrack.File.AudioConverter;
                    _totalSamplesConv = streamer.GetSampleCountInFormat(_streamingFormat);
                }
                else
                {
                    _totalSamplesConv = 0;
                }

                _playHead = (int) MathF.Floor(_totalSamplesConv * progress);

                // Readjust crossfade playhead - if in use.
                if (_crossFadePlayHead != 0) _crossFadePlayHead = (int) MathF.Floor(_totalSamplesConv * oldCrossfadeProgress);
                InvalidateAudioBlocks();
            }

            // Verify dst size
            Debug.Assert(buffer.Length / format.FrameSize == getFrames);

            int framesLeft = getFrames;
            var framesGotten = 0;
            while (framesLeft > 0 && _readyBlocks.Count != 0)
            {
                if (_readyBlocks.TryPeek(out AudioDataBlock? b))
                {
                    int framesGot = b.FramesWritten - b.FramesRead;
                    int framesCanGet = Math.Min(framesGot, framesLeft);

                    int bufferCopyOffset = b.FramesRead * format.FrameSize;
                    int bufferCopyLength = framesCanGet * format.FrameSize;
                    new Span<byte>(b.Data).Slice(bufferCopyOffset, bufferCopyLength).CopyTo(buffer);
                    buffer = buffer[bufferCopyLength..];

                    b.FramesRead += framesCanGet;
                    framesGotten += framesCanGet;
                    framesLeft -= framesCanGet;

                    Debug.Assert(b.FramesRead <= b.FramesWritten);
                    if (b.FramesRead == b.FramesWritten)
                    {
                        _readyBlocks.TryDequeue(out AudioDataBlock? _);
                        _dataPool.Return(b);
                    }
                }
            }

            // Buffer was starved, stream directly to it.
            // This occurs frequently when starting to play a new track as
            // the backend buffer requests data further into the future initially.
            if (framesLeft > 0 && _readyBlocks.Count == 0)
            {
                int framesGot = GetDataToBuffer(framesLeft, buffer);
                framesGotten += framesGot;
            }

            return framesGotten;
        }

        protected void InvalidateAudioBlocks()
        {
            while (_readyBlocks.Count != 0 && _readyBlocks.TryDequeue(out AudioDataBlock? r))
                _dataPool.Return(r);

            MetricBackendMissedFrames = 0;
            MetricDataStoredInBlocks = 0;
        }

        #endregion

        public virtual void Dispose()
        {
            Disposed = true;
            InvalidateAudioBlocks();
        }
    }
}