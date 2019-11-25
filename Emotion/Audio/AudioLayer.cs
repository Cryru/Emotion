#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;
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
        /// The layer's volume.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// The status of the audio layer.
        /// </summary>
        public PlaybackStatus Status { get; protected set; } = PlaybackStatus.NotPlaying;

        /// <summary>
        /// The track currently playing - if any.
        /// </summary>
        public AudioAsset CurrentTrack
        {
            get => _currentTrack < 0 || _currentTrack > _playlist.Count - 1 ? null : _playlist[_currentTrack].File;
        }

        protected int _currentTrack = -1;
        protected List<AudioTrack> _playlist = new List<AudioTrack>();
        protected bool _loopingCurrent;

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

        public void PlayNext(AudioAsset file)
        {
            lock (_playlist)
            {
                _playlist.Insert(_currentTrack + 1, new AudioTrack(file));
                if (_currentTrack == -1) _currentTrack = 0;
                if (Status == PlaybackStatus.NotPlaying) TransitionStatus(PlaybackStatus.Playing);
            }
        }

        public void AddToQueue(AudioAsset file)
        {
            lock (_playlist)
            {
                _playlist.Add(new AudioTrack(file));
                if (Status == PlaybackStatus.NotPlaying) TransitionStatus(PlaybackStatus.Playing);
            }
        }

        public void SetLoopCurrent(bool loop)
        {
            _loopingCurrent = loop;
        }

        public void Resume()
        {
            lock (_playlist)
            {
                if (Status == PlaybackStatus.Playing) return;
                if (_playlist.Count == 0 || _currentTrack == -1) return;
                TransitionStatus(PlaybackStatus.Playing);
            }
        }

        public void Pause()
        {
            lock (_playlist)
            {
                TransitionStatus(PlaybackStatus.Paused);
            }
        }

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
            if (_currentTrack < 0 || _currentTrack > _playlist.Count - 1) return 0;

            // Pause if window is not focused.
            if (Engine.Host != null && Engine.Host.Window != null && Engine.Host.Window.Focused) Engine.Host.FocusWait.WaitOne();

            AudioTrack currentTrack;
            AudioStreamer streamer;
            lock (_playlist)
            {
                currentTrack = _playlist[_currentTrack];
            }

            streamer = _playlist[_currentTrack].Streamer;

            if (streamer == null) return 0;

            // Set the conversion format to the requested one - if it doesn't match.
            if (!format.Equals(streamer.ConvFormat)) streamer.SetConvertFormat(format);

            // Get frames from the streamer.
            int framesOutput = streamer.GetNextFrames(framesRequested, dest.Slice(framesOffset * format.SampleSize));

            // Check if the buffer was filled.
            Debug.Assert(framesOutput <= framesRequested);
            if (framesOutput == framesRequested) return framesOutput;

            // If less frames were drawn than the buffer can take - the track is over.

            // Check if looping.
            int playlistCount;
            lock (_playlist)
            {
                playlistCount = _playlist.Count;
            }

            if (_loopingCurrent)
            {
                streamer.Reset();
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
            InternalStatusChange(Status, newStatus);
            Status = newStatus;

            if (newStatus == PlaybackStatus.NotPlaying) _currentTrack = -1;
        }

        protected abstract void InternalStatusChange(PlaybackStatus oldStatus, PlaybackStatus newStatus);
        public abstract void Dispose();
    }
}