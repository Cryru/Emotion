#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
    /// <summary>
    /// Object for internal reference by the audio implementation of the platform.
    /// </summary>
    public abstract class AudioLayer
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

        protected int _currentTrack = -1;
        protected List<AudioTrack> _playlist = new List<AudioTrack>();

        /// <summary>
        /// Called when the current track loops.
        /// The input parameter is the track which looped.
        /// </summary>
        public EmotionEvent<AudioAsset> OnTrackLoop = new EmotionEvent<AudioAsset>();

        /// <summary>
        /// Called when the current track changes.
        /// The first parameter is the old track, the second is the new one.
        /// If there is no further track the new track parameter will be null.
        /// </summary>
        public EmotionEvent<AudioAsset, AudioAsset> OnTrackChanged = new EmotionEvent<AudioAsset, AudioAsset>();

        protected AudioLayer(string name)
        {
            Name = name;
        }

        #region API

        /// <summary>
        /// Sets the track to be played next in the playlist. If the playlist is empty and the layer isn't paused the track is
        /// played immediately.
        /// </summary>
        /// <param name="track">The track to play next.</param>
        public void PlayNext(AudioTrack track)
        {
            if (!OwnTrack(track)) return;

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
            if (!OwnTrack(track)) return;

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
            if (!OwnTrack(track)) return;

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

        protected int GetDataForCurrentTrack(AudioFormat format, int framesRequested, Span<byte> dest, int framesOffset = 0)
        {
            if (Status != PlaybackStatus.Playing) return 0;

            int playlistCount;
            lock (_playlist)
            {
                playlistCount = _playlist.Count;
            }

            if (_currentTrack < 0 || _currentTrack > playlistCount - 1) return 0;

            // Pause if window is not focused.
            if (Engine.Host != null && !Engine.Host.IsFocused) Engine.Host.FocusWait.WaitOne();

            AudioTrack currentTrack;
            lock (_playlist)
            {
                currentTrack = _playlist[_currentTrack];
            }

            if (currentTrack == null) return 0;

            Stopwatch test = Stopwatch.StartNew();

            // Set the conversion format to the requested one - if it doesn't match.
            if (!format.Equals(currentTrack.ConvFormat)) currentTrack.SetConvertFormat(format);

            // Get frames from the streamer.
            Span<byte> destBuffer = dest.Slice(framesOffset * format.FrameSize);
            int framesOutput = currentTrack.GetNextFrames(framesRequested, destBuffer);

            // If cross fading and there is another track afterward.
            if (currentTrack.CrossFade.HasValue && _currentTrack < playlistCount - 1)
            {
                AudioTrack nextTrack;
                lock (_playlist)
                {
                    nextTrack = _playlist[_currentTrack + 1];
                }

                if (nextTrack != null)
                {
                    float progress = currentTrack.Progress;
                    float playback = currentTrack.Playback;
                    float currentTrackDuration = currentTrack.File.Duration;
                    float val = currentTrack.CrossFade.Value;
                    var crossFade = false;


                    // Make sure there is enough duration in the next track to crossfade into.
                    if (val > 0.0)
                    {
                        float activationTimeStamp = currentTrackDuration - val;
                        if (playback >= activationTimeStamp)
                        {
                            float timeLeft = currentTrackDuration - activationTimeStamp;
                            crossFade = timeLeft < nextTrack.File.Duration;
                        }
                    }
                    else
                    {
                        float activationProgress = 1.0f + val;
                        if (progress >= activationProgress)
                        {
                            float activationTimeStamp = currentTrackDuration - (currentTrackDuration * -val);
                            float timeLeft = currentTrackDuration - activationTimeStamp;
                            crossFade = timeLeft < nextTrack.File.Duration;
                        }
                    }

                    if (crossFade)
                    {
                        if (nextTrack.FadeIn == null)
                        {
                            nextTrack.FadeIn = val;
                        }

                        // Match convert format and get frames from the next track.
                        if (!format.Equals(nextTrack.ConvFormat)) nextTrack.SetConvertFormat(format);
                        var nextTrackDest = new Span<byte>(new byte[destBuffer.Length]);
                        int nextTrackFrameCount = nextTrack.GetNextFrames(framesOutput, nextTrackDest);
                        Debug.Assert(nextTrackFrameCount == framesOutput);

                        int channels = format.Channels;
                        for (int i = 0; i < nextTrackFrameCount; i++)
                        {
                            for (int c = 0; c < channels; c++)
                            {
                                int sampleIdx = (i * channels) + c;
                                //float sampleCurrentTrack = AudioStreamer.GetSampleAsFloat(sampleIdx, destBuffer, format);
                                //float sampleNextTrack = AudioStreamer.GetSampleAsFloat(sampleIdx, nextTrackDest, format);

                                //sampleCurrentTrack = (sampleCurrentTrack + sampleNextTrack) / 2f;
                                //AudioStreamer.SetSampleAsFloat(sampleIdx, sampleCurrentTrack, destBuffer, format);
                            }
                        }

                    }

                }
            }

            Engine.Log.Info($"{test.ElapsedMilliseconds}", "Benchmark");

            // Check if the buffer was filled.
            Debug.Assert(framesOutput <= framesRequested);
            if (framesOutput == framesRequested) return framesOutput;

            // If less frames were drawn than the buffer can take - the track is over.

            // Check if looping.
            if (LoopingCurrent)
            {
                currentTrack.Reset();
                OnTrackLoop.Invoke(currentTrack.File);
            }
            // Otherwise, go to next track.
            else
            {
                lock (_playlist)
                {
                    _playlist.RemoveAt(0);
                }

                playlistCount--;
            }

            // Check if there are more tracks.
            if (playlistCount > 0)
            {
                framesOutput += GetDataForCurrentTrack(format, framesRequested - framesOutput, dest, framesOutput);

                AudioTrack newTrack;
                lock (_playlist)
                {
                    newTrack = _playlist[_currentTrack];
                }

                OnTrackChanged.Invoke(currentTrack.File, newTrack.File);
            }
            else
            {
                lock (_playlist)
                {
                    TransitionStatus(PlaybackStatus.NotPlaying);
                }

                OnTrackChanged.Invoke(currentTrack.File, null);
            }

            return framesOutput;
        }

        #endregion

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
            }

            InternalStatusChange(Status, newStatus);
            Status = newStatus;

            // If no longer playing, reset the current track.
            if (newStatus == PlaybackStatus.NotPlaying) _currentTrack = -1;
        }

        private bool OwnTrack(AudioTrack track)
        {
            if (track.Layer != null)
            {
                Engine.Log.Error("Tried to play a track which is already playing on another layer.", MessageSource.Audio);
                return false;
            }

            track.Layer = this;
            return true;
        }

        protected abstract void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus);
        public abstract void Dispose();
    }
}