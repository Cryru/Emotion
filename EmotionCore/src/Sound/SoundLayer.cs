// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Engine.Threading;
using Emotion.IO;
using Emotion.Utils;
using OpenTK.Audio.OpenAL;
using Soul;

#endregion

namespace Emotion.Sound
{
    /// <summary>
    /// A sound layer is in charge of playing one sound or a list of sounds asynchronously. To play multiple sounds you would
    /// use different layers.
    /// </summary>
    public sealed class SoundLayer
    {
        #region Properties

        /// <summary>
        /// The layer's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The layer's volume from 0 to ???.
        /// </summary>
        public float Volume { get; set; } = 1f;

        /// <summary>
        /// Whether the layer is playing, is paused, etc.
        /// </summary>
        public SoundStatus Status { get; private set; }

        /// <summary>
        /// Whether to loop the currently playing source.
        /// </summary>
        public bool Looping { get; set; }

        /// <summary>
        /// Loop the last queued track only instead of everything.
        /// </summary>
        public bool LoopLastOnly { get; set; } = true;

        /// <summary>
        /// The position of the playback within the TotalDuration in seconds.
        /// </summary>
        public float PlaybackLocation { get; private set; }

        /// <summary>
        /// The duration of all sounds queued on the layer in seconds.
        /// </summary>
        public float TotalDuration
        {
            get { return _playList.Sum(x => x?.Duration ?? 0); }
        }

        /// <summary>
        /// The file currently playing.
        /// </summary>
        public SoundFile CurrentlyPlayingFile { get; private set; }

        #region Fading

        /// <summary>
        /// The duration of the fade in effect in seconds.
        /// </summary>
        public int FadeInLength { get; set; }

        /// <summary>
        /// The duration of the fade out effect in seconds.
        /// </summary>
        public int FadeOutLength { get; set; }

        /// <summary>
        /// Whether to skip the natural fade out when a file is over but still want to keep the FadeOutLength property to support
        /// FadeOutOnChange.
        /// </summary>
        public bool SkipNaturalFadeOut { get; set; }

        /// <summary>
        /// Whether to fade in only on the first loop.
        /// </summary>
        public bool FadeInFirstLoopOnly { get; set; };

        /// <summary>
        /// Whether to fade out the file when a new one is played. Makes for smooth transitions.
        /// </summary>
        public bool FadeOutOnChange { get; set; }

        #endregion

        #endregion

        #region Internals

        private int _pointer;
        private List<SoundFile> _playList;
        private bool _isFirst;

        // Fade out on change variables.
        private bool _forceFadeOut;
        private float _forceFadeOutStartDuration;
        private float _forceFadeOutLength;
        private Action _forceFadeOutEndEvent;

        #endregion

        /// <summary>
        /// Creates a new sound layer. This is usually done and managed by the SoundManager object in the Context.
        /// </summary>
        /// <param name="name">The name of the layer. Used by the SoundManager to refer to the layer.</param>
        public SoundLayer(string name)
        {
            Name = name;
            _playList = new List<SoundFile>();

            ALThread.ExecuteALThread(() =>
            {
                // Initiate source.
                _pointer = AL.GenSource();
                Helpers.CheckErrorAL("creating source");
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Created {ToString()}.");
            });
        }

        #region API

        /// <summary>
        /// Resume playing if paused.
        /// </summary>
        public void Resume()
        {
            if (Status != SoundStatus.Paused) return;
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, $"Resumed {ToString()}.");
            ALThread.ExecuteALThread(() =>
            {
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
                Helpers.CheckErrorAL("resuming");
            });
        }

        /// <summary>
        /// Pause if playing.
        /// </summary>
        public void Pause()
        {
            if (Status != SoundStatus.Playing) return;
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, $"Paused {ToString()}.");
            ALThread.ExecuteALThread(() =>
            {
                AL.SourcePause(_pointer);
                Status = SoundStatus.Paused;
                Helpers.CheckErrorAL("pausing");
            });
        }

        /// <summary>
        /// Stop playing any files.
        /// </summary>
        /// <param name="now">Whether to stop instantly or perform FadeOutOnChange if enabled.</param>
        public ContinuousAction StopPlayingAll(bool now = false)
        {
            ContinuousAction thisAction = new ContinuousAction();

            void StopPlayingAllInternal()
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Stopped {ToString()}.");

                // Stop playback, clear played buffer.
                AL.Source(_pointer, ALSourceb.Looping, false);
                AL.SourceStop(_pointer);
                // Remove played buffers.
                RemovePlayed();
                Status = SoundStatus.Stopped;
                Helpers.CheckErrorAL("stopping");

                // Reset tracker variables.
                PerformReset();

                thisAction.Done();
            }

            if (FadeOutOnChange && !now)
                SetupForceFadeOut(StopPlayingAllInternal);
            else
                ALThread.ExecuteALThread(StopPlayingAllInternal);

            return thisAction;
        }

        /// <summary>
        /// Queue a file to be played on the layer.
        /// </summary>
        /// <param name="file"></param>
        public ContinuousAction QueuePlay(SoundFile file)
        {
            ContinuousAction thisAction = new ContinuousAction();

            void QueuePlayInternal()
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Queued [{file.Name}] on {ToString()}.");

                // If playback is over but stop wasn't called then cleanup needs to be performed.
                if (Status == SoundStatus.Stopped) PerformReset();

                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playList.Add(file);
                Helpers.CheckErrorAL($"queuing in source {_pointer}");

                // Play if not playing.
                if (Status != SoundStatus.Stopped) return;
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
                Helpers.CheckErrorAL($"playing source {_pointer}");

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Started playing [{file.Name}] on {ToString()}.");

                thisAction.Done();
            }

            if (FadeOutOnChange)
                SetupForceFadeOut(QueuePlayInternal);
            else
                ALThread.ExecuteALThread(QueuePlayInternal);

            return thisAction;
        }

        /// <summary>
        /// Play a file on the layer. If any previous file is playing it will be stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        public ContinuousAction Play(SoundFile file)
        {
            ContinuousAction thisAction = new ContinuousAction();

            void PlayInternal()
            {
                // Stop whatever was playing before.
                StopPlayingAll(true);

                // Queue the file.
                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playList.Add(file);
                Helpers.CheckErrorAL($"queuing single in source {_pointer}");
                // Play it.
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
                Helpers.CheckErrorAL($"playing single in source {_pointer}");

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Started playing [{file.Name}] on {ToString()}.");

                thisAction.Done();
            }

            // Check if forcing a fade out.
            if (FadeOutOnChange)
                SetupForceFadeOut(PlayInternal);
            else
                ALThread.ExecuteALThread(PlayInternal);

            return thisAction;
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public ContinuousAction Dispose()
        {
            ContinuousAction thisAction = new ContinuousAction();

            ALThread.ExecuteALThread(() =>
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Destroyed {ToString()}.");

                StopPlayingAll(true);
                AL.DeleteSource(_pointer);
                Helpers.CheckErrorAL($"cleanup of source {_pointer}");

                _pointer = -1;
                _playList.Clear();

                thisAction.Done();
            });

            return thisAction;
        }

        #endregion

        #region Force FadeOut Helpers

        /// <summary>
        /// Setups a forced fading out.
        /// </summary>
        /// <param name="action">The action to execute once its over.</param>
        private void SetupForceFadeOut(Action action)
        {
            // Check if there is anything currently playing to fade out at all.
            if (CurrentlyPlayingFile == null || Status == SoundStatus.Stopped)
            {
                ALThread.ExecuteALThread(action);
                return;
            }

            // Check if a force fade out is already running.
            if (!_forceFadeOut)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                _forceFadeOut = true;
                _forceFadeOutStartDuration = PlaybackLocation;
                _forceFadeOutLength = MathHelper.Clamp(FadeOutLength, FadeOutLength, timeLeft);
                _forceFadeOutEndEvent = action;
            }
            else
            {
                // Chain action if a new one is added.
                Action oldAction = _forceFadeOutEndEvent;
                _forceFadeOutEndEvent = () =>
                {
                    oldAction();
                    action();
                };
            }

            Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Performing smooth fade out on {ToString()}.");
        }

        #endregion

        /// <summary>
        /// Remove played buffers from the source queue. In the playlist they are replaced by nulls.
        /// </summary>
        private void RemovePlayed()
        {
            ALThread.ForceALThread();

            AL.GetSource(_pointer, ALGetSourcei.BuffersProcessed, out int processed);
            Helpers.CheckErrorAL($"checking processed buffers of source {_pointer}");
            if (processed > 0)
            {
                AL.SourceUnqueueBuffers(_pointer, processed);
                Helpers.CheckErrorAL($"removing {processed} buffers of source {_pointer}");
                if (_playList.Count > 0) _playList.RemoveRange(0, Math.Min(_playList.Count, processed));
                _isFirst = false;
            }
        }

        /// <summary>
        /// Resets internal trackers and variables.
        /// </summary>
        private void PerformReset()
        {
            _isFirst = true;
            _playList.Clear();
            _forceFadeOut = false;
            _forceFadeOutLength = 0f;
            _forceFadeOutStartDuration = 0f;
            PlaybackLocation = 0;
        }

        /// <summary>
        /// Update the layer. Is called on the ALThread by the sound manager.
        /// </summary>
        internal void Update()
        {
            // Check if buffer is initialized or destroyed.
            if (_pointer <= 0) return;

            Helpers.CheckErrorAL($"start update of source {_pointer}");

            // Update focused state.
            if (!UpdateFocusedState()) return;
            Helpers.CheckErrorAL($"focus update of source {_pointer}");

            // Remove played buffers.
            RemovePlayed();

            // Prepare variables in use by parts of the update.
            AL.GetSource(_pointer, ALGetSourcei.Buffer, out int filePointer);
            AL.GetSource(_pointer, ALGetSourcei.BuffersQueued, out int queuedBuffers);
            bool last = queuedBuffers == 1;
            Helpers.CheckErrorAL($"buffer getting of source {_pointer}");

            // Update volume.
            UpdateVolume(last);

            // Swap buffers which have played off the stack.
            UpdateLooping(last);

            // Update paused/playing state.
            UpdatePlayingState();

            // Update current file.
            UpdateCurrentFile(filePointer);

            // Update playback location.
            UpdatePlaybackLocation();

            // Check whether force fade out ended.
            if (_forceFadeOut)
            {
                float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                if (timeLeftForce > _forceFadeOutLength || Status == SoundStatus.Stopped)
                {
                    _forceFadeOut = false;
                    _forceFadeOutEndEvent();
                }
            }

            Helpers.CheckErrorAL($"end update of source {_pointer}");
        }

        #region Update Parts

        private bool UpdateFocusedState()
        {
            // Check if not focused.
            if (!Context.Host.Focused)
                switch (Status)
                {
                    // If focus loss paused, paused or not playing don't do anything.
                    case SoundStatus.FocusLossPause:
                    case SoundStatus.Paused:
                    case SoundStatus.Stopped:
                        return false;
                    // If playing then focus loss pause.
                    case SoundStatus.Playing:
                        AL.SourcePause(_pointer);
                        Status = SoundStatus.FocusLossPause;
                        break;
                }
            // If focused and focus loss paused, resume.
            else if (Context.Host.Focused && Status == SoundStatus.FocusLossPause)
                AL.SourcePlay(_pointer);

            return true;
        }

        private void UpdateVolume(bool last)
        {
            Helpers.CheckErrorAL($"before updating of volume of source {_pointer}");

            float systemVolume = Context.Settings.Sound ? Context.Settings.Volume / 100f : 0f;
            float scaled = MathHelper.Clamp(Volume * systemVolume, 0, 10);

            // Perform fading.
            if (CurrentlyPlayingFile != null)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                // Check if fading in.
                if (PlaybackLocation < FadeInLength && _isFirst)
                    scaled = MathHelper.Lerp(0, scaled, PlaybackLocation / FadeInLength);
                else if (PlaybackLocation > FadeInLength && _isFirst && FadeInFirstLoopOnly)
                    _isFirst = false;

                // Check if performing a forced fade out due to FadeOutOnChange.
                if (_forceFadeOut)
                {
                    float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                    if (timeLeftForce != 0) scaled = MathHelper.Lerp(scaled, 0, timeLeftForce / _forceFadeOutLength);
                }
                // Check if performing a natural fading out. Fade out only the last buffer.
                else if (timeLeft < FadeOutLength && last && !SkipNaturalFadeOut)
                {
                    scaled = MathHelper.Lerp(0, scaled, timeLeft / FadeOutLength);
                }
            }

            // Fix ultra low values.
            if (scaled < 0.000f) scaled = 0f;

            AL.Source(_pointer, ALSourcef.Gain, scaled);
            Helpers.CheckErrorAL($"updating of volume of source {_pointer} to {scaled}");
        }

        private void UpdateLooping(bool last)
        {
            if (LoopLastOnly)
                if (last)
                {
                    RemovePlayed();
                    AL.Source(_pointer, ALSourceb.Looping, Looping);
                }
                else
                {
                    AL.Source(_pointer, ALSourceb.Looping, false);
                }
            else
                AL.Source(_pointer, ALSourceb.Looping, Looping);

            Helpers.CheckErrorAL($"updating of looping of source {_pointer}");
        }

        private void UpdatePlayingState()
        {
            AL.GetSource(_pointer, ALGetSourcei.SourceState, out int status);

            switch ((ALSourceState) status)
            {
                case ALSourceState.Playing:
                    Status = SoundStatus.Playing;
                    break;
                case ALSourceState.Paused:
                    Status = Context.Host.Focused ? SoundStatus.Paused : SoundStatus.FocusLossPause;
                    break;
                case ALSourceState.Initial:
                case ALSourceState.Stopped:
                    Status = SoundStatus.Stopped;
                    break;
            }

            Helpers.CheckErrorAL($"updating of state of source {_pointer}");
        }

        private void UpdateCurrentFile(int currentFilePointer)
        {
            CurrentlyPlayingFile = currentFilePointer == 0 ? null : _playList.FirstOrDefault(x => x?.Pointer == currentFilePointer);
        }

        private void UpdatePlaybackLocation()
        {
            AL.GetSource(_pointer, ALSourcef.SecOffset, out float playbackLoc);
            PlaybackLocation = playbackLoc;

            Helpers.CheckErrorAL($"updating of playback location of source {_pointer}");
        }

        #endregion

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[Sound Layer] [ALPointer:[{_pointer}] Name:[{Name}] Looping/LastOnly:[{Looping}/{LoopLastOnly}] Status:[{Status}]";
        }
    }
}