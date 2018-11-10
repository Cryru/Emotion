// Emotion - https://github.com/Cryru/Emotion

#region Using

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
        /// The position of the playback within the TotalDuration.
        /// </summary>
        public float PlaybackLocation { get; private set; }

        /// <summary>
        /// The duration of all sounds queued on the layer.
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
        /// The duration of the fade in effect.
        /// </summary>
        public int FadeInLength { get; set; }

        /// <summary>
        /// The duration of the fade out effect.
        /// </summary>
        public int FadeOutLength { get; set; }

        /// <summary>
        /// Whether to fade out the file when a new one is played. Makes for smooth transitions.
        /// </summary>
        public bool FadeOutOnChange { get; set; }

        #endregion

        #endregion

        #region Internals

        private int _pointer;
        private List<SoundFile> _playList;

        // Fade out on change variables.
        private bool _forceFadeOut;
        private float _forceFadeOutStartDuration;
        private float _forceFadeOutLength;
        private bool _isFirst;

        #endregion

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
        public void StopPlayingAll()
        {
            ALThread.ExecuteALThread(() =>
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Stopped {ToString()}.");

                // Stop playback, clear played buffer.
                AL.Source(_pointer, ALSourceb.Looping, false);
                AL.SourceStop(_pointer);
                RemovePlayed();
                Status = SoundStatus.Stopped;
                Helpers.CheckErrorAL("stopping");

                // Clear buffers.
                PerformReset();
            });
        }

        /// <summary>
        /// Queue a file to be played on the layer.
        /// </summary>
        /// <param name="file"></param>
        public void QueuePlay(SoundFile file)
        {
            ALThread.ExecuteALThread(() =>
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Queued [{file.Name}] on {ToString()}.");

                // If playback is over but stop wasn't called then cleanup needs to be performed.
                if (Status == SoundStatus.Stopped) PerformReset();

                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playList.Add(file);
                Helpers.CheckErrorAL("queuing");

                // Play if not playing.
                if (Status != SoundStatus.Stopped) return;
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
                Helpers.CheckErrorAL("playing");

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Started playing [{file.Name}] on {ToString()}.");
            });
        }

        /// <summary>
        /// Play a file on the layer. If any previous file is playing it will be stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        /// <param name="skipFadeOutOnChange">Whether to skip checking the FadeOutOnChange option.</param>
        public void Play(SoundFile file, bool skipFadeOutOnChange = false)
        {
            ALThread.ExecuteALThread(() =>
            {
                // Check if playing fade out on change.
                if (FadeOutOnChange && CurrentlyPlayingFile != null && !skipFadeOutOnChange)
                {
                    float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                    _forceFadeOut = true;
                    _forceFadeOutStartDuration = PlaybackLocation;
                    _forceFadeOutLength = MathHelper.Clamp(FadeOutLength, FadeOutLength, timeLeft);

                    QueuePlay(file);

                    Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Performing smooth fade out on {ToString()} before switch to [{file.Name}].");
                    return;
                }

                // Stop whatever was playing before.
                StopPlayingAll();

                // Queue the file.
                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playList.Add(file);
                Helpers.CheckErrorAL("queuing single");

                // Play it.
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
                Helpers.CheckErrorAL("playing single");

                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Stopped current and started playing [{file.Name}] on {ToString()}.");
            });
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public void Dispose()
        {
            ALThread.ExecuteALThread(() =>
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Destroyed {ToString()}.");

                StopPlayingAll();
                AL.DeleteSource(_pointer);
                Helpers.CheckErrorAL("cleanup");

                _pointer = -1;
                _playList.Clear();
            });
        }

        #endregion

        /// <summary>
        /// Remove played buffers from the source queue. In the playlist they are replaced by nulls.
        /// </summary>
        private void RemovePlayed()
        {
            ALThread.ForceALThread();

            AL.GetSource(_pointer, ALGetSourcei.BuffersProcessed, out int processed);
            if (processed > 0)
            {
                AL.SourceUnqueueBuffers(_pointer, processed);
                _playList.RemoveRange(0, processed);
                _isFirst = false;
            }


            Helpers.CheckErrorAL("removing buffers");
        }

        private void PerformReset()
        {
            _isFirst = true;
            _playList.Clear();
            _forceFadeOut = false;
            _forceFadeOutLength = 0f;
            _forceFadeOutStartDuration = 0f;
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
            bool first = _isFirst;
            Helpers.CheckErrorAL($"buffer getting of source {_pointer}");

            // Update volume.
            UpdateVolume(first, last);

            // Swap buffers which have played off the stack.
            UpdateLooping(last);

            // Update paused/playing state.
            UpdatePlayingState();

            // Update current file.
            UpdateCurrentFile(filePointer);

            // Update playback location.
            UpdatePlaybackLocation();

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

        private void UpdateVolume(bool first, bool last)
        {
            Helpers.CheckErrorAL($"before updating of volume of source {_pointer}");

            float systemVolume = Context.Settings.Sound ? Context.Settings.Volume : 0f;
            float scaled = MathHelper.Clamp(Volume * (systemVolume / 100f), 0, 10);

            // Perform fading.
            if (CurrentlyPlayingFile != null)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                // Check if fading in.
                if (PlaybackLocation < FadeInLength && first)
                    scaled = MathHelper.Lerp(0, scaled, PlaybackLocation / FadeInLength);

                // Check if fading out. Fade out only the last buffer.
                if (timeLeft < FadeOutLength && last)
                {
                    scaled = MathHelper.Lerp(0, scaled, timeLeft / FadeOutLength);
                }
                // Check if performing a forced fade out. This is done when switching tracks.
                else if (_forceFadeOut)
                {
                    float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                    if (timeLeftForce != 0) scaled = MathHelper.Lerp(scaled, 0, timeLeftForce / _forceFadeOutLength);

                    // Check if force fade out is over.
                    if (timeLeftForce > _forceFadeOutLength)
                    {
                        _forceFadeOut = false;
                        Play(_playList.Last(), true);
                        return;
                    }
                }
            }

            AL.Source(_pointer, ALSourcef.Gain, scaled);
            // todo: Check if this is needed
            // AL.Source(_pointer, ALSourcef.MaxGain, scaled < 0 ? 0f : 1f);

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

        public override string ToString()
        {
            return $"[Sound Layer] [ALPointer:[{_pointer}] Name:[{Name}] Looping/LastOnly:[{Looping}/{LoopLastOnly}] Status:[{Status}]";
        }
    }
}