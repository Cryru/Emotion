#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;

#endregion

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
        public AudioTrack CurrentTrack
        {
            get
            {
                lock (_playlist)
                {
                    return _playlist.Count == 0 ? null : _playlist[0];
                }
            }
        }

        /// <summary>
        /// What percentage (0-1) of the track has finished playing.
        /// </summary>
        public float Progress
        {
            get
            {
                if (_playHead == 0 || _updateStreamingTrack) return 0f;
                return (float)_playHead / _totalSamplesConv;
            }
        }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get
            {
                if (_streamingTrack == null) return 0;
                return Progress * _streamingTrack.File.Duration;
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
        public event Action<AudioAsset> OnTrackLoop;

        /// <summary>
        /// Called when the current track changes.
        /// The first parameter is the old track, the second is the new one.
        /// If there is no further track the new track parameter will be null.
        /// </summary>
        public event Action<AudioAsset, AudioAsset> OnTrackChanged;

        // For fade effects volume is calculated per chunk of frames, rather than for every frame.
        private const int VOLUME_MODULATION_FRAME_GRANULARITY = 100;
        private const int INITIAL_INTERNAL_BUFFER_SIZE = 4000;

        protected List<AudioTrack> _playlist = new(); // Always read and written in locks
        protected float[] _internalBuffer; // Internal memory to keep resamples frames for applying post proc to.
        protected int _playHead; // The progress into the current track, relative to the converted format.
        protected int _totalSamplesConv; // The total sample count in the dst format. Used to know where _playHead is.
        protected float[] _internalBufferCrossFade; // Same memory, but for the second track when crossfading.
        protected int _crossFadePlayHead; // Same progress tracking, but for the second track when crossfading.
        protected int _loopCount; // Number of times the current track has looped. 0 if it hasn't.

        protected AudioLayer(string name)
        {
            Name = name;
            _internalBuffer = new float[INITIAL_INTERNAL_BUFFER_SIZE];
            _internalBufferCrossFade = new float[INITIAL_INTERNAL_BUFFER_SIZE];
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
                if (_playlist.Count == 1) InvalidateDoubleBuffer();
                if (Status == PlaybackStatus.NotPlaying) TransitionStatus(PlaybackStatus.Playing);
            }
        }

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
                if (_playlist.Count == 1) InvalidateDoubleBuffer();
                if (Status == PlaybackStatus.NotPlaying) TransitionStatus(PlaybackStatus.Playing);
            }
        }

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
                InvalidateStreamingTrack();
                InvalidateDoubleBuffer();
                if (Status == PlaybackStatus.NotPlaying) TransitionStatus(PlaybackStatus.Playing);
            }
        }

        public void QuickPlay(AudioAsset file)
        {
            QuickPlay(new AudioTrack(file));
        }

        /// <summary>
        /// Resume playback, if paused. If currently playing nothing happens. If currently not playing - start playing.
        /// </summary>
        public void Resume()
        {
            lock (_playlist)
            {
                if (Status == PlaybackStatus.Playing) return;
                TransitionStatus(PlaybackStatus.Playing);
            }
        }

        /// <summary>
        /// Pause playback. If currently not playing anything the layer is paused anyway, and will need to be resumed.
        /// </summary>
        public void Pause()
        {
            lock (_playlist)
            {
                TransitionStatus(PlaybackStatus.Paused);
            }
        }

        /// <summary>
        /// Stop all playback, and clear the playlist.
        /// </summary>
        public void Stop()
        {
            lock (_playlist)
            {
                _playlist.Clear();
                TransitionStatus(PlaybackStatus.NotPlaying);
            }
        }

        protected void TrackChanged(AudioTrack old, AudioTrack newT, bool instantUpdate = false)
        {
            OnTrackChanged?.Invoke(old?.File, newT?.File);
            InvalidateStreamingTrack();
            if (instantUpdate) UpdateStreamingTrack();
        }

        /// <summary>
        /// Always used in _playlist locks.
        /// </summary>
        protected virtual void TransitionStatus(PlaybackStatus newStatus)
        {
            // If wasn't playing - but now am, and the current track is invalid, set the current track.
            if (Status == PlaybackStatus.NotPlaying && newStatus == PlaybackStatus.Playing)
            {
                // Check if there is anything in the playlist.
                if (_playlist.Count == 0)
                {
                    Engine.Log.Warning($"Tried to play layer {Name}, but the playlist is empty.", MessageSource.Audio);
                    return;
                }

                _crossFadePlayHead = 0;
                TrackChanged(null, _playlist[0]);
            }

            Status = newStatus;
        }

        #endregion

        #region Streaming Double Buffering

        private const int BUFFER_COUNT = 2;
        private const bool ENABLE_DB = true;
        private byte[][] _doubleBuffer = new byte[BUFFER_COUNT][];
        private int[] _dbOffset = new int[BUFFER_COUNT];
        private int[] _dbFramesStored = new int[BUFFER_COUNT];
        private int _bufferIdx;

#if DEBUG
        private Stopwatch _timer = new();
        public static int TimeTaken;
#endif

        protected int GetDataForCurrentTrack(AudioFormat format, int getFrames, Span<byte> buffer)
        {
            // Check if any track playing.
            UpdateStreamingTrack();
            if (_streamingTrack == null) return 0;

            // Pause sound if host is paused.
            if (Engine.Host != null && Engine.Host.HostPaused)
            {
                if (GLThread.IsGLThread()) return 0; // Don't stop main thread.
                Engine.Host.HostPausedWaiter.WaitOne();
            }

#if DEBUG
            _timer.Restart();
#endif

            // Verify dst size
            Debug.Assert(buffer.Length / format.FrameSize == getFrames);

            // Resize internal scratch memory if needed.
            int samplesRequested = getFrames * format.Channels;
            if (_internalBuffer.Length < samplesRequested)
            {
                Array.Resize(ref _internalBuffer, samplesRequested);
                Array.Resize(ref _internalBufferCrossFade, samplesRequested);

                InvalidateDoubleBuffer();
            }

            // Make sure we're getting the samples in the format we think we are.
            AudioConverter streamer = _streamingTrack.File.AudioConverter;
            float oldCrossfadeProgress = _crossFadePlayHead != 0 ? _crossFadePlayHead / _totalSamplesConv : 0;
            if (!format.Equals(_streamingFormat))
            {
                float progress = _playHead != 0 ? (float)_playHead / _totalSamplesConv : 0;
                _streamingFormat = format;
                _totalSamplesConv = streamer.GetSampleCountInFormat(_streamingFormat);
                _playHead = (int)MathF.Floor(_totalSamplesConv * progress);

                // Readjust crossfade playhead - if in use.
                if (_crossFadePlayHead != 0) _crossFadePlayHead = (int)MathF.Floor(_totalSamplesConv * oldCrossfadeProgress);

                InvalidateDoubleBuffer();
            }

            int framesLeft = getFrames;
            var framesGotten = 0;
            if (_doubleBuffer[_bufferIdx] != null)
                while (framesLeft > 0)
                {
                    int framesStored = _dbFramesStored[_bufferIdx];
                    if (framesStored == 0) break; // Reached empty buffer.

                    int framesTake = Math.Min(framesStored, framesLeft);
                    framesLeft -= framesTake; // Mark frames as gotten.
                    framesGotten += framesTake;

                    // Copy from db to dst.
                    int bufferCopyOffset = _dbOffset[_bufferIdx];
                    int bufferCopyLength = framesTake * format.FrameSize;
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

            // If any frames still need to be gotten, get them from the track.
            if (framesLeft > 0)
            {
#if DEBUG
                if (_playHead != 0) ResortedToLayer = true;
#endif

                int frames = StreamDataFromCurrentTrack(framesLeft, buffer);
                framesGotten += frames;
                framesLeft -= frames;
            }

            // No more frames could be generated. Playback is over.
            if (framesLeft > 0)
            {
                lock (_playlist)
                {
                    TransitionStatus(PlaybackStatus.NotPlaying);
                }

                TrackChanged(_streamingTrack, null);
            }

#if DEBUG
            var sampleStored = 0;
            for (var i = 0; i < _doubleBuffer.Length; i++)
            {
                int framesStored = _dbFramesStored[i];
                sampleStored += framesStored;
            }

            SamplesStored = sampleStored;

            if (SamplesStored != 0)
                LeastSamplesStored = Math.Min(SamplesStored, LeastSamplesStored);

            _timer.Stop();
            TimeTaken = Math.Max(TimeTaken, (int)_timer.ElapsedMilliseconds);
#endif
            return framesGotten;
        }

        protected virtual void Update()
        {
            // Fill all buffers that are out of frames.
            if (_doubleBuffer[0] != null && _streamingTrack != null)
                for (var i = 0; i < _doubleBuffer.Length; i++)
                {
                    byte[] buffer = _doubleBuffer[i];
                    int framesStored = _dbFramesStored[i];
                    if (framesStored != 0) continue;
                    _dbOffset[i] = 0;
                    _dbFramesStored[i] = StreamDataFromCurrentTrack(_internalBuffer.Length / _streamingFormat.Channels, buffer);
                }
        }

        protected void InvalidateDoubleBuffer()
        {
            // Reset double buffering.
            if (!ENABLE_DB || _streamingFormat == null) return;
            _bufferIdx = 0;
            for (var i = 0; i < _doubleBuffer.Length; i++)
            {
                _doubleBuffer[i] = new byte[_internalBuffer.Length * _streamingFormat.SampleSize];
                _dbOffset[i] = 0;
                _dbFramesStored[i] = 0;
            }
        }

        #endregion

        #region Stream Logic

        protected bool _updateStreamingTrack = true;
        protected AudioTrack _streamingTrack;
        protected AudioConverter _converter;
        protected AudioFormat _streamingFormat;

#if DEBUG
        public static bool ResortedToLayer;
        public static int SamplesStored;
        public static int LeastSamplesStored = int.MaxValue;
#endif

        protected void InvalidateStreamingTrack()
        {
            _updateStreamingTrack = true;
        }

        protected void UpdateStreamingTrack()
        {
            if (!_updateStreamingTrack) return;

            // Get the currently playing track.
            AudioTrack currentTrack;
            lock (_playlist)
            {
                if (Status == PlaybackStatus.NotPlaying || _playlist.Count == 0)
                {
                    _streamingTrack = null;
                    return;
                }

                currentTrack = _playlist[0];
                if (currentTrack == null)
                {
                    _streamingTrack = null;
                    return;
                }
            }

            _streamingTrack = currentTrack;
            _playHead = _crossFadePlayHead; // Assuming track changed to the one we were crossfading into.
            _crossFadePlayHead = 0;
            _converter = currentTrack.File.AudioConverter;
            if (_streamingFormat != null) _totalSamplesConv = _converter.GetSampleCountInFormat(_streamingFormat);
            _updateStreamingTrack = false;
        }

        protected int StreamDataFromCurrentTrack(int framesRequested, Span<byte> dest, int framesOffset = 0)
        {
            // Get post processed 32f buffer data.
            int framesOutput = GetProcessedFramesFromTrack(_streamingFormat, _streamingTrack, framesRequested, _internalBuffer, ref _playHead);

            // Fill destination buffer in destination sample size format.
            Span<byte> destBuffer = dest.Slice(framesOffset * _streamingFormat.FrameSize);
            int samplesRequested = framesRequested * _streamingFormat.Channels;
            var srcBuffer = new Span<float>(_internalBuffer, 0, samplesRequested);
            AudioConverter.SetBufferOfSamplesAsFloat(srcBuffer, destBuffer, _streamingFormat);

            // Check if the buffer was filled.
            if (framesOutput == framesRequested) return framesOutput;

            // If less frames were drawn than the buffer can take - the track is over.

            // Check if looping.
            AudioTrack newTrack = null;
            if (LoopingCurrent)
            {
                // Manually update playhead as track wont change.
                _playHead = _crossFadePlayHead;
                _crossFadePlayHead = 0;
                _loopCount++;
                OnTrackLoop?.Invoke(_streamingTrack.File);
                newTrack = _streamingTrack;
            }
            // Otherwise, go to next track.
            else
            {
                _loopCount = 0;
                lock (_playlist)
                {
                    _playlist.Remove(_streamingTrack);
                    if (_playlist.Count > 0)
                        newTrack = _playlist[0];
                }

                if (newTrack != null) TrackChanged(_streamingTrack, newTrack, true);
            }

            // Check if there are more tracks.
            if (newTrack != null)
            {
                if (newTrack.SetLoopingCurrent) LoopingCurrent = true;

                // Fill rest of buffer with samples from the next track.
                framesOutput += StreamDataFromCurrentTrack(framesRequested - framesOutput, dest, framesOutput);
            }

            return framesOutput;
        }

        #endregion

        public virtual void Dispose()
        {
            Disposed = true;
        }
    }
}