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
                    return _currentTrack < 0 || _currentTrack > _playlist.Count - 1 ? null : _playlist[_currentTrack];
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
                if (_playHead == 0) return 0f;
                return (float) _playHead / _totalSamples;
            }
        }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get
            {
                if (_playStateTrack == null) return 0;
                return Progress * _playStateTrack.File.Duration;
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

        protected int _currentTrack = -1; // Always updated in _playlist locks
        protected List<AudioTrack> _playlist = new(); // Always read and written in locks
        protected float[] _internalBuffer;
        protected float[] _internalBufferCrossFade;

        protected AudioTrack _playStateTrack;
        protected CachedAudioStreamer _cache;
        protected AudioFormat _sampleIndexFormat;
        protected int _totalSamples;
        protected int _playHead;
        protected int _crossFadePlayHead;
        protected int _loopCount;

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
                _playlist.Insert(_currentTrack + 1, track);
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
                _currentTrack = 0;
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

        #endregion

        #region Stream Logic

#if DEBUG
        public static long LongestFillTimeTaken;
        public static Stopwatch DbgBufferFillTimeTaken = new Stopwatch();
        private static object _profilerRefresh = ((Func<object>) (() =>
        {
            Engine.DebugOnUpdateStart += (s, e) => { DbgBufferFillTimeTaken.Reset(); };
            return null;
        }))();
#endif

        protected int GetDataForCurrentTrack(AudioFormat format, int framesRequested, Span<byte> dest, int framesOffset = 0)
        {
            if (Status != PlaybackStatus.Playing) return 0;
            Debug.Assert(dest.Length == framesRequested * format.FrameSize);

            // Pause sound if host is paused.
            if (Engine.Host != null && Engine.Host.HostPaused)
            {
                if (GLThread.IsGLThread()) return 0; // Don't stop main thread.
                Engine.Host.HostPausedWaiter.WaitOne();
            }

            // Get the currently playing track.
            AudioTrack currentTrack;
            lock (_playlist)
            {
                if (_currentTrack < 0 || _currentTrack > _playlist.Count - 1) return 0;
                currentTrack = _playlist[_currentTrack];
                if (currentTrack == null) return 0;
            }

            // Resize internal buffers if needed.
            int samplesRequested = framesRequested * format.Channels;
            if (_internalBuffer.Length < samplesRequested)
            {
                Array.Resize(ref _internalBuffer, samplesRequested);
                Array.Resize(ref _internalBufferCrossFade, samplesRequested);
            }

#if DEBUG
            DbgBufferFillTimeTaken.Start();
#endif

            // Check if the play state needs updating. This means that the current track has changed.
            if (_playStateTrack != currentTrack)
            {
                _playStateTrack = currentTrack;
                _playHead = _crossFadePlayHead;
                _crossFadePlayHead = 0;
                _cache = currentTrack.File.AudioStream.Value;
                _sampleIndexFormat = _cache.ConvFormat;
                _totalSamples = _cache.ConvSamples;
            }

            // Make sure we're getting the samples in the format we think we are.
            float oldCrossfadeProgress = _crossFadePlayHead != 0 ? _crossFadePlayHead / _totalSamples : 0;
            if (!format.Equals(_sampleIndexFormat))
            {
                float progress = _playHead != 0 ? (float) _playHead / _totalSamples : 0;
                _cache.SetConvertFormat(format);
                _playHead = (int) MathF.Floor(_cache.ConvSamples * progress);
                _sampleIndexFormat = format;
                _totalSamples = _cache.ConvSamples;

                // Readjust crossfade playhead - if in use.
                if (_crossFadePlayHead != 0) _crossFadePlayHead = (int) MathF.Floor(_totalSamples * oldCrossfadeProgress);
            }

            // Get post processed buffer data. (Float samples)
            int framesOutput = GetProcessedFramesFromTrack(format, currentTrack, framesRequested, _internalBuffer, ref _playHead);

            // Fill destination buffer in destination sample size format.
            Span<byte> destBuffer = dest.Slice(framesOffset * format.FrameSize);
            var srcBuffer = new Span<float>(_internalBuffer, 0, framesOutput * format.FrameSize / 4);
            AudioStreamer.SetBufferOfSamplesAsFloat(srcBuffer, destBuffer, format);

#if DEBUG
            if (DbgBufferFillTimeTaken.ElapsedMilliseconds > LongestFillTimeTaken) LongestFillTimeTaken = DbgBufferFillTimeTaken.ElapsedMilliseconds;
            DbgBufferFillTimeTaken.Stop();
#endif

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
                OnTrackLoop?.Invoke(currentTrack.File);
                newTrack = currentTrack;
            }
            // Otherwise, go to next track.
            else
            {
                _loopCount = 0;
                lock (_playlist)
                {
                    _playlist.Remove(currentTrack);
                    if (_playlist.Count > 0 && _currentTrack != -1)
                        newTrack = _playlist[_currentTrack];
                }
            }

            // Check if there are more tracks.
            if (newTrack != null)
            {
                OnTrackChanged?.Invoke(currentTrack.File, newTrack.File);
                if (newTrack.SetLoopingCurrent) LoopingCurrent = true;

                // Fill rest of buffer with samples from the next track.
                framesOutput += GetDataForCurrentTrack(format, framesRequested - framesOutput, dest, framesOutput);
            }
            else
            {
                lock (_playlist)
                {
                    TransitionStatus(PlaybackStatus.NotPlaying);
                }

                OnTrackChanged?.Invoke(currentTrack.File, null);
            }

            return framesOutput;
        }

        #endregion

        /// <summary>
        /// Always used in _playlist locks.
        /// </summary>
        private void TransitionStatus(PlaybackStatus newStatus)
        {
            // If wasn't playing - but now am, and the current track is invalid, set the current track.
            if ((Status == PlaybackStatus.NotPlaying || Status == PlaybackStatus.Paused) && newStatus == PlaybackStatus.Playing && _currentTrack == -1)
            {
                // Check if there is anything in the playlist.
                if (_playlist.Count == 0)
                {
                    Engine.Log.Warning($"Tried to play layer {Name}, but the playlist is empty.", MessageSource.Audio);
                    return;
                }

                _currentTrack = 0;
                _crossFadePlayHead = 0;
            }

            InternalStatusChange(Status, newStatus);
            Status = newStatus;

            // If no longer playing, reset the current track.
            if (newStatus == PlaybackStatus.NotPlaying) _currentTrack = -1;
        }

        protected abstract void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus);
        public abstract void Dispose();
    }
}