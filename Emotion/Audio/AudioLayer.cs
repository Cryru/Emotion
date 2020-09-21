#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        protected float[] _resampleMemory;
        protected float[] _resampleMemoryCrossFade;

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
            const int resampleMemoryInitSize = 4000;
            _resampleMemory = new float[resampleMemoryInitSize];
            _resampleMemoryCrossFade = new float[resampleMemoryInitSize];
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

            // Resize resample memory if needed.
            int samplesRequested = framesRequested * format.Channels;
            if (_resampleMemory.Length < samplesRequested)
            {
                Array.Resize(ref _resampleMemory, samplesRequested);
                Array.Resize(ref _resampleMemoryCrossFade, samplesRequested);
            }

            int channels = format.Channels;
            float baseVolume = Volume * Engine.Configuration.MasterVolume;
            int framesOutput = GetProcessedFramesFromTrack(format, currentTrack, framesRequested, _resampleMemory, baseVolume);

            // If cross fading check if there is another track afterward.
            if (currentTrack.CrossFade.HasValue && _currentTrack < playlistCount - 1)
            {
                AudioTrack nextTrack;
                lock (_playlist)
                {
                    nextTrack = _playlist[_currentTrack + 1];
                }

                if (nextTrack != null)
                    PostProcessCrossFade(format, currentTrack, nextTrack, framesOutput, baseVolume, _resampleMemory, _resampleMemoryCrossFade);
            }

            // Fill destination buffer.
            Span<byte> destBuffer = dest.Slice(framesOffset * format.FrameSize);
            for (var i = 0; i < framesOutput; i++)
            {
                int frameIdx = i * channels;
                for (var c = 0; c < channels; c++)
                {
                    int sampleIdx = frameIdx + c;
                    AudioStreamer.SetSampleAsFloat(sampleIdx, _resampleMemory[sampleIdx], destBuffer, format);
                }
            }

            // Check if the buffer was filled.
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
                AudioTrack newTrack;
                lock (_playlist)
                {
                    newTrack = _playlist[_currentTrack];
                }

                OnTrackChanged.Invoke(currentTrack.File, newTrack.File);
                framesOutput += GetDataForCurrentTrack(format, framesRequested - framesOutput, dest, framesOutput);
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

        protected static int GetProcessedFramesFromTrack(AudioFormat format, AudioTrack track, int frames, float[] memory, float baseVolume)
        {
            // Set the conversion format to the requested one - if it doesn't match.
            track.EnsureAudioFormat(format);

            // Get data.
            int framesOutput = track.GetNextFrames(frames, memory);
            Debug.Assert(framesOutput <= frames);

            // Preprocess data.
            int channels = format.Channels;
            bool mergeChannels = Engine.Configuration.ForceMono && track.File.Format.Channels == 2 && channels == 2;
            if (mergeChannels) PostProcessForceMono(framesOutput, memory);
            PostProcessApplyFading(baseVolume, track, framesOutput, channels, memory);

            return framesOutput;
        }

        /// <summary>
        /// Apply cross fading between two tracks.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PostProcessCrossFade(AudioFormat format, AudioTrack currentTrack, AudioTrack nextTrack, int frameCount, float baseVolume, float[] soundData, float[] crossFadeMemory)
        {
            int channels = format.Channels;
            float currentTrackDuration = currentTrack.File.Duration;
            float progress = currentTrack.Progress;
            float playback = progress * currentTrackDuration;
            float crossFadeVal = currentTrack.CrossFade!.Value;
            float crossFadeProgress = 0;

            if (crossFadeVal < 0.0) crossFadeVal = currentTrackDuration - currentTrackDuration * -crossFadeVal;

            // Make sure there is enough duration in the next track to cross fade into.
            float activationTimeStamp = currentTrackDuration - crossFadeVal;
            if (playback >= activationTimeStamp)
            {
                float timeLeft = currentTrackDuration - playback;
                if (timeLeft < nextTrack.File.Duration) crossFadeProgress = timeLeft / crossFadeVal;
            }

            if (crossFadeProgress == 0.0f) return;

            // Add a fade in to the next track (if none). Makes the cross fade better.
            nextTrack.FadeIn ??= crossFadeVal;

            // Get data from the next track.
            int nextTrackFrameCount = GetProcessedFramesFromTrack(format, nextTrack, frameCount, crossFadeMemory, baseVolume);
            Debug.Assert(nextTrackFrameCount == frameCount);

            for (var i = 0; i < frameCount; i++)
            {
                for (var c = 0; c < channels; c++)
                {
                    int sampleIdx = i * channels + c;
                    float sampleCurrentTrack = soundData[sampleIdx];
                    float sampleNextTrack = crossFadeMemory[sampleIdx];
                    sampleCurrentTrack = Maths.Lerp(sampleNextTrack, sampleCurrentTrack, crossFadeProgress);
                    soundData[sampleIdx] = sampleCurrentTrack;
                }
            }
        }

        /// <summary>
        /// Check if forcing mono sound. This matters only if both the source and destination formats are stereo.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void PostProcessForceMono(int framesOutput, float[] soundData)
        {
            for (var i = 0; i < framesOutput; i++)
            {
                int sampleIdx = i * 2;
                float left = soundData[sampleIdx];
                float right = soundData[sampleIdx + 1];

                float merged = (left + right) / 2f;
                soundData[sampleIdx] = merged;
                soundData[sampleIdx + 1] = merged;
            }
        }

        /// <summary>
        /// Apply fade in and fade out effects.
        /// </summary>
        private static void PostProcessApplyFading(float baseVolume, AudioTrack currentTrack, int frameCount, int channels, float[] soundData)
        {
            float currentTrackDuration = currentTrack.File.Duration;
            float progress = currentTrack.Progress;
            float playback = currentTrackDuration * progress;
            Debug.Assert(progress >= 0.0f && progress <= 1.0f);

            if (currentTrack.FadeIn != null)
            {
                float val = currentTrack.FadeIn.Value;
                float fadeProgress;
                if (currentTrack.FadeIn < 0)
                    fadeProgress = progress / -val;
                else
                    fadeProgress = playback / val;
                baseVolume *= Maths.Clamp01(fadeProgress);
            }

            if (currentTrack.FadeOut != null || currentTrack.CrossFade != null)
            {
                float val = currentTrack.FadeOut ?? currentTrack.CrossFade.Value;
                var fadeProgress = 1.0f;
                float fileDuration = currentTrackDuration;

                if (val < 0.0) val = fileDuration * -val;
                float activationTimeStamp = fileDuration - val;
                if (playback >= activationTimeStamp)
                    fadeProgress = 1.0f - (playback - activationTimeStamp) / val;

                baseVolume *= Maths.Clamp01(fadeProgress);
            }

            baseVolume = MathF.Pow(baseVolume, Engine.Configuration.AudioCurve);

            for (var i = 0; i < frameCount; i++)
            {
                int frameIdx = i * channels;
                for (var c = 0; c < channels; c++)
                {
                    int sampleIdx = frameIdx + c;
                    soundData[sampleIdx] *= baseVolume;
                }
            }
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