// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using OpenTK.Audio.OpenAL;

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
        /// The layer's volume from 0 to ???. Is 1 by default.
        /// </summary>
        public float Volume { get; set; } = 1f;

        /// <summary>
        /// Whether the layer is playing, is paused, etc.
        /// </summary>
        public SoundStatus Status { get; private set; } = SoundStatus.Initial;

        /// <summary>
        /// Whether to loop the currently playing source.
        /// </summary>
        public bool Looping { get; set; }

        /// <summary>
        /// Loop the last queued track only instead of everything. Is true by default.
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
            get { return PlayList.ToArray().Sum(x => x?.Duration ?? 0); }
        }

        /// <summary>
        /// The file currently playing.
        /// </summary>
        public SoundFile CurrentlyPlayingFile
        {
            get
            {
                // Check if anything playing.
                if (PlayList.Count == 0) return null;

                // Calculate currently playing through the playback and the playlist.
                SoundFile[] tempList = PlayList.ToArray();
                for (int i = 0; i < tempList.Length; i++)
                {
                    if (PlaybackLocation <= tempList[i].Duration) return tempList[i];
                }

                // Unknown.
                Context.Log.Warning("Unknown currently playing file.", MessageSource.SoundManager);

                return tempList[tempList.Length - 1];
            }
        }

        /// <summary>
        /// The list of files queued.
        /// </summary>
        public List<SoundFile> PlayList { get; private set; }

        #region Fading

        /// <summary>
        /// The duration of the fade in effect in seconds.
        /// </summary>
        public float FadeInLength { get; set; }

        /// <summary>
        /// The duration of the fade out effect in seconds.
        /// </summary>
        public float FadeOutLength { get; set; }

        /// <summary>
        /// Whether to skip the natural fade out when a file is over but still want to keep the FadeOutLength property to support
        /// FadeOutOnChange.
        /// </summary>
        public bool SkipNaturalFadeOut { get; set; }

        /// <summary>
        /// Whether to fade in only on the first loop.
        /// </summary>
        public bool FadeInFirstLoopOnly { get; set; }

        /// <summary>
        /// Whether to fade out the file when a new one is played. Makes for smooth transitions.
        /// </summary>
        public bool FadeOutOnChange { get; set; }

        #endregion

        #endregion

        #region Internals

        /// <summary>
        /// The OpenAL pointer to the ALSource.
        /// </summary>
        public int ALSource { get; private set; }

        /// <summary>
        /// Tracker for whether the currently playing track is the first.
        /// </summary>
        private bool _isFirst;

        // Fade out on change variables.
        private bool _forceFadeOut;
        private float _forceFadeOutStartDuration;
        private float _forceFadeOutLength;
        private Task _forceFadeOutEndEvent;

        #endregion

        /// <summary>
        /// Creates a new sound layer. This is usually done and managed by the SoundManager object in the Context.
        /// </summary>
        /// <param name="name">The name of the layer. Used by the SoundManager to refer to the layer.</param>
        public SoundLayer(string name)
        {
            Name = name;
            PlayList = new List<SoundFile>();

            ALThread.ExecuteALThread(() =>
            {
                // Initiate source.
                ALSource = AL.GenSource();
                ALThread.CheckError("creating source");
                Context.Log.Info($"Created {this}.", MessageSource.SoundManager);
            }).Wait();
        }

        #region API

        /// <summary>
        /// Resume playing if paused.
        /// </summary>
        public void Resume()
        {
            if (Status != SoundStatus.Paused) return;
            Context.Log.Trace($"Resumed {this}.", MessageSource.SoundManager);
            ALThread.ExecuteALThread(() =>
            {
                AL.SourcePlay(ALSource);
                Status = SoundStatus.Playing;
                ALThread.CheckError("resuming");
            });
        }

        /// <summary>
        /// Pause if playing.
        /// </summary>
        public void Pause()
        {
            if (Status != SoundStatus.Playing) return;
            Context.Log.Trace($"Paused {this}.", MessageSource.SoundManager);
            ALThread.ExecuteALThread(() =>
            {
                AL.SourcePause(ALSource);
                Status = SoundStatus.Paused;
                ALThread.CheckError("pausing");
            });
        }

        /// <summary>
        /// Stop playing any files.
        /// </summary>
        /// <param name="now">Whether to stop instantly or perform FadeOutOnChange if enabled.</param>
        public Task StopPlayingAll(bool now = false)
        {
            void StopPlayingAllInternal()
            {
                Context.Log.Info($"Stopped {this}.", MessageSource.SoundManager);

                // Stop playback, clear played buffer.
                AL.Source(ALSource, ALSourceb.Looping, false);
                AL.SourceStop(ALSource);

                // Remove played buffers.
                RemovePlayed();
                Status = SoundStatus.Stopped;
                ALThread.CheckError("stopping");

                // Reset tracker variables.
                PerformReset();
            }

            return FadeOutOnChange && !now ? SetupForceFadeOut(StopPlayingAllInternal) : ALThread.ExecuteALThread(StopPlayingAllInternal);
        }

        /// <summary>
        /// Queue a file to be played on the layer.
        /// </summary>
        /// <param name="file"></param>
        public Task QueuePlay(SoundFile file)
        {
            void QueuePlayInternal()
            {
                // Check if mixing number of channels.
                if (PlayList.Count > 0 && PlayList[0].Channels != file.Channels)
                {
                    Context.Log.Warning("Queuing a track with a different number of channels to the one(s) in the playlist is not supported.", MessageSource.SoundManager);
                    return;
                }

                Context.Log.Info($"Queued [{file.Name}] on {this}.", MessageSource.SoundManager);

                // If playback is over but stop wasn't called then cleanup needs to be performed.
                if (Status == SoundStatus.Stopped) PerformReset();

                AL.SourceQueueBuffer(ALSource, file.ALBuffer);
                PlayList.Add(file);
                ALThread.CheckError($"queuing {file.ALBuffer} in source {ALSource}");

                // Play if not playing.
                if (Status != SoundStatus.Stopped) return;
                AL.SourcePlay(ALSource);
                Status = SoundStatus.Playing;
                ALThread.CheckError($"playing {file.ALBuffer} on source {ALSource}");

                Context.Log.Info($"Started playing [{file.Name}] on {this}.", MessageSource.SoundManager);
            }

            return FadeOutOnChange ? SetupForceFadeOut(QueuePlayInternal) : ALThread.ExecuteALThread(QueuePlayInternal);
        }

        /// <summary>
        /// Play a file on the layer. If any previous file is playing it will be stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        public Task Play(SoundFile file)
        {
            void PlayInternal()
            {
                // Stop whatever was playing before.
                StopPlayingAll(true);

                // Queue the file.
                AL.SourceQueueBuffer(ALSource, file.ALBuffer);
                PlayList.Add(file);
                ALThread.CheckError($"queuing single {file.ALBuffer} in source {ALSource}");
                // Play it.
                AL.SourcePlay(ALSource);
                Status = SoundStatus.Playing;
                ALThread.CheckError($"playing single {file.ALBuffer} in source {ALSource}");

                Context.Log.Info($"Started playing [{file.Name}] on {this}.", MessageSource.SoundManager);
            }

            // Check if forcing a fade out.
            return FadeOutOnChange ? SetupForceFadeOut(PlayInternal) : ALThread.ExecuteALThread(PlayInternal);
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public void Dispose()
        {
            ALThread.ExecuteALThread(() =>
            {
                Context.Log.Info($"Destroyed {this}.", MessageSource.SoundManager);

                StopPlayingAll(true);
                AL.DeleteSource(ALSource);
                ALThread.CheckError($"cleanup of source {ALSource}");

                ALSource = -1;
                PlayList.Clear();
            }).Wait();
        }

        #endregion

        #region Force FadeOut Helpers

        /// <summary>
        /// Setups a forced fading out.
        /// </summary>
        /// <param name="action">The action to execute once its over.</param>
        private Task SetupForceFadeOut(Action action)
        {
            // Check if there is anything currently playing to fade out at all.
            if (CurrentlyPlayingFile == null || Status == SoundStatus.Stopped) return ALThread.ExecuteALThread(action);

            // Check if a force fade out is already running.
            if (!_forceFadeOut)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                _forceFadeOut = true;
                _forceFadeOutStartDuration = PlaybackLocation;
                _forceFadeOutLength = MathExtension.Clamp(FadeOutLength, FadeOutLength, timeLeft);
                _forceFadeOutEndEvent = new Task(action);
            }
            else
            {
                // Chain action if a new one is added.
                _forceFadeOutEndEvent.ContinueWith(_ => { action(); });
            }

            Context.Log.Info($"Performing smooth fade out on {this}.", MessageSource.SoundManager);
            return _forceFadeOutEndEvent;
        }

        #endregion

        /// <summary>
        /// Remove played buffers from the source queue. In the playlist they are replaced by nulls.
        /// </summary>
        private void RemovePlayed()
        {
            AL.GetSource(ALSource, ALGetSourcei.BuffersProcessed, out int processed);
            ALThread.CheckError($"checking processed buffers of source {ALSource}");
            if (processed <= 0) return;
            AL.SourceUnqueueBuffers(ALSource, processed);
            ALThread.CheckError($"removing {processed} buffers of source {ALSource}");
            if (PlayList.Count > 0) PlayList.RemoveRange(0, Math.Min(PlayList.Count, processed));
            _isFirst = false;
        }

        /// <summary>
        /// Resets internal trackers and variables.
        /// </summary>
        private void PerformReset()
        {
            _isFirst = true;
            PlayList.Clear();
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
            if (ALSource <= 0) return;

            ALThread.CheckError($"start update of source {ALSource}");

            // Update focused state.
            if (!UpdateFocusedState()) return;
            ALThread.CheckError($"focus update of source {ALSource}");

            // Remove played buffers.
            RemovePlayed();

            // Update paused/playing state.
            UpdatePlayingState();

            // Prepare variables in use by parts of the update.
            AL.GetSource(ALSource, ALGetSourcei.BuffersQueued, out int queuedBuffers);
            bool last = queuedBuffers == 1;
            ALThread.CheckError($"buffer getting of source {ALSource}");

            if (PlayList.Contains(null))
            {
                bool a = true;
            }

            // Update volume.
            UpdateVolume(last);

            // Swap buffers which have played off the stack.
            UpdateLooping(last);

            // Update current file.
            UpdateCurrentFile();

            // Update playback location.
            UpdatePlaybackLocation();

            // Check whether force fade out ended.
            if (_forceFadeOut)
            {
                float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                if (timeLeftForce > _forceFadeOutLength || Status == SoundStatus.Stopped || timeLeftForce < 0)
                {
                    _forceFadeOut = false;
                    _forceFadeOutEndEvent.RunSynchronously();
                }
            }

            ALThread.CheckError($"end update of source {ALSource}");
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
                        AL.SourcePause(ALSource);
                        Status = SoundStatus.FocusLossPause;
                        break;
                }
            // If focused and focus loss paused, resume.
            else if (Context.Host.Focused && Status == SoundStatus.FocusLossPause)
                AL.SourcePlay(ALSource);

            return true;
        }

        private void UpdateVolume(bool last)
        {
            ALThread.CheckError($"before updating of volume of source {ALSource}");

            float systemVolume = Context.Settings.SoundSettings.Sound ? Context.Settings.SoundSettings.Volume / 100f : 0f;
            float scaled = MathExtension.Clamp(Volume * systemVolume, 0, 10);

            // Perform fading if anything is playing and not muted.
            if (CurrentlyPlayingFile != null && scaled != 0f)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                // Check if fading in.
                if (PlaybackLocation < FadeInLength && _isFirst)
                    scaled = MathExtension.Lerp(0, scaled, PlaybackLocation / FadeInLength);
                else if (PlaybackLocation > FadeInLength && _isFirst && FadeInFirstLoopOnly)
                    _isFirst = false;

                // Check if performing a forced fade out due to FadeOutOnChange.
                if (_forceFadeOut)
                {
                    float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                    if (timeLeftForce != 0) scaled = MathExtension.Lerp(scaled, 0, timeLeftForce / _forceFadeOutLength);
                }
                // Check if performing a natural fading out. Fade out only the last buffer.
                else if (timeLeft < FadeOutLength && last && !SkipNaturalFadeOut)
                {
                    scaled = MathExtension.Lerp(0, scaled, timeLeft / FadeOutLength);
                }
            }
            // Set volume to 0 when not playing.
            else if (CurrentlyPlayingFile == null)
            {
                scaled = 0f;
            }

            // Clamp ultra low resulting values. These can break OpenAL.
            if (scaled < 0.000f) scaled = 0f;

            AL.Source(ALSource, ALSourcef.Gain, scaled);
            ALThread.CheckError($"updating of volume of source {ALSource} to {scaled}");
        }

        private void UpdateLooping(bool last)
        {
            if (LoopLastOnly)
                if (last)
                {
                    RemovePlayed();
                    AL.Source(ALSource, ALSourceb.Looping, Looping);
                }
                else
                {
                    AL.Source(ALSource, ALSourceb.Looping, false);
                }
            else
                AL.Source(ALSource, ALSourceb.Looping, Looping);

            ALThread.CheckError($"updating of looping of source {ALSource}");
        }

        private void UpdatePlayingState()
        {
            AL.GetSource(ALSource, ALGetSourcei.SourceState, out int status);

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

            ALThread.CheckError($"updating of state of source {ALSource}");
        }

        private void UpdateCurrentFile()
        {
            //AL.GetSource(ALSource, ALGetSourcei.Buffer, out int filePointer);

            //// The currently playing file is considered to be the one at the front of the playlist which isn't a null.
            //// This logic is enforced by the RemovePlayed() function in the layer Update loop.
            ////CurrentlyPlayingFile = PlayList.FirstOrDefault(x => x != null);
            //if (PlayList.Count == 0)
            //{
            //    CurrentlyPlayingFile = null;
            //}
            //else
            //{
            //    CurrentlyPlayingFile = PlayList[filePointer];
            //}

            //// The current buffer is always reported as 0 on Mac.
            //if ((CurrentlyPlayingFile?.ALBuffer ?? 0) != filePointer && CurrentPlatform.OS != PlatformName.Mac)
            //    Context.Log.Warning($"Currently playing file might be wrong for layer [{Name}].", MessageSource.SoundManager);
        }

        private void UpdatePlaybackLocation()
        {
            AL.GetSource(ALSource, ALSourcef.SecOffset, out float playbackLoc);
            PlaybackLocation = playbackLoc;

            ALThread.CheckError($"updating of playback location of source {ALSource}");
        }

        #endregion

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[Sound Layer] [ALPointer:[{ALSource}] Name:[{Name}] Playback:[{PlaybackLocation}/{TotalDuration}] Status:[{Status}]]";
        }
    }
}