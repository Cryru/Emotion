#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Adfectus.Common;
using Adfectus.Common.Threading;
using Adfectus.Logging;
using Adfectus.OpenAL;
using Adfectus.Platform.DesktopGL.Assets;
using Adfectus.Sound;
using Adfectus.Utility;

#endregion

namespace Adfectus.Platform.DesktopGL.Sound
{
    public sealed class ALSoundLayer : SoundLayer
    {
        #region Properties

        public override List<SoundFile> PlayList
        {
            get => ALPlayList.Select(x => (SoundFile) x).ToList();
        }

        /// <summary>
        /// The list of files queued.
        /// </summary>
        public List<ALSoundFile> ALPlayList { get; private set; } = new List<ALSoundFile>();

        #endregion

        #region InternAls

        /// <summary>
        /// The OpenAl pointer to the AlSource.
        /// </summary>
        public uint AlSource { get; private set; }

        /// <summary>
        /// Tracker for whether the currently playing track is the first.
        /// </summary>
        private bool _isFirst;

        // Fade out on change variables.
        private bool _forceFadeOut;
        private float _forceFadeOutStartDuration;
        private float _forceFadeOutLength;
        private AwAction _forceFadeOutEndEvent;

        #endregion

        /// <summary>
        /// Creates a new sound layer. This is usuAlly done and managed by the ALSoundManager object in the Context.
        /// </summary>
        /// <param name="name">The name of the layer. Used by the ALSoundManager to refer to the layer.</param>
        public ALSoundLayer(string name) : base(name)
        {
            ALThread.ExecuteALThread(() =>
            {
                // Initiate source.
                Al.GenSource(out uint temp);
                AlSource = temp;

                ALThread.CheckError("creating source");
                Engine.Log.Info($"Created {this}.", MessageSource.SoundManager);
            }).Wait();
        }

        #region API

        /// <summary>
        /// Resume playing if paused.
        /// </summary>
        public AwAction Resume()
        {
            if (Status != SoundStatus.Paused) return new AwAction(true);
            Engine.Log.Trace($"Resuming {this}.", MessageSource.SoundManager);
            return ALThread.ExecuteALThread(() =>
            {
                Engine.Log.Info($"Resumed {this}.", MessageSource.SoundManager);
                Al.SourcePlay(AlSource);
                ALThread.CheckError("resuming");
                UpdatePlayingState();
            });
        }

        /// <summary>
        /// Pause if playing.
        /// </summary>
        public AwAction Pause()
        {
            if (Status != SoundStatus.Playing) return new AwAction(true);
            Engine.Log.Trace($"Pausing {this}.", MessageSource.SoundManager);
            return ALThread.ExecuteALThread(() =>
            {
                Engine.Log.Info($"Paused {this}.", MessageSource.SoundManager);
                Al.SourcePause(AlSource);
                ALThread.CheckError("pausing");
                UpdatePlayingState();
            });
        }

        /// <summary>
        /// Stop playing any files.
        /// </summary>
        /// <param name="now">Whether to stop instantly or perform FadeOutOnChange if enabled.</param>
        public AwAction StopPlayingAll(bool now = false)
        {
            Engine.Log.Trace($"Stopping {this}.", MessageSource.SoundManager);

            void StopPlayingAllInternAl()
            {
                Engine.Log.Info($"Stopped {this}.", MessageSource.SoundManager);

                // Stop playback, clear played buffer.
                Al.Sourcei(AlSource, Al.Looping, 0);
                Al.SourceStop(AlSource);
                UpdatePlayingState();

                // Remove played buffers.
                RemovePlayed();
                ALThread.CheckError("stopping");

                // Reset tracker variables.
                PerformReset();
            }

            return FadeOutOnChange && !now ? SetupForceFadeOut(StopPlayingAllInternAl) : ALThread.ExecuteALThread(StopPlayingAllInternAl);
        }

        /// <summary>
        /// Queue a file to be played on the layer.
        /// </summary>
        /// <param name="file"></param>
        public AwAction QueuePlay(SoundFile file)
        {
            // Convert to platform format.
            if(!(file is ALSoundFile alSoundFile)) return null;

            Engine.Log.Trace($"Queuing [{file.Name}] on {this}.", MessageSource.SoundManager);

            void QueuePlayInternAl()
            {
                // Check if mixing number of channels.
                if (ALPlayList.Count > 0 && ALPlayList[0].Channels != file.Channels)
                {
                    Engine.Log.Warning("Queuing a track with a different number of channels to the one(s) in the playlist is not supported.", MessageSource.SoundManager);
                    return;
                }

                Engine.Log.Info($"Queued [{file.Name}] on {this}.", MessageSource.SoundManager);

                // If playback is over but stop wasn't called then cleanup needs to be performed.
                if (Status == SoundStatus.Stopped) PerformReset();

                Al.SourceQueueBuffers(AlSource, 1, new[] {alSoundFile.ALBuffer});
                ALPlayList.Add(alSoundFile);
                ALThread.CheckError($"queuing {alSoundFile.ALBuffer} in source {AlSource}");

                // Play if not playing.
                if (Status != SoundStatus.Stopped) return;
                Al.SourcePlay(AlSource);
                UpdatePlayingState();
                ALThread.CheckError($"playing {alSoundFile.ALBuffer} on source {AlSource}");

                Engine.Log.Info($"Started playing [{file.Name}] on {this}.", MessageSource.SoundManager);
            }

            return FadeOutOnChange && Status != SoundStatus.Playing ? SetupForceFadeOut(QueuePlayInternAl) : ALThread.ExecuteALThread(QueuePlayInternAl);
        }

        /// <summary>
        /// Play a file on the layer. If any previous file is playing it will be stopped.
        /// </summary>
        /// <param name="file">The file to play.</param>
        public AwAction Play(SoundFile file)
        {
            // Convert to platform format.
            if (!(file is ALSoundFile alSoundFile))
            {
                Engine.Log.Warning("The file provided to the ALSoundLayer isn't a ALSoundFile.", MessageSource.SoundManager);
                return null;
            }

            Engine.Log.Trace($"Playing [{file.Name}] on {this}.", MessageSource.SoundManager);

            void PlayInternAl()
            {
                // Stop whatever was playing before.
                StopPlayingAll(true).Wait();

                // Queue the file.
                Al.SourceQueueBuffers(AlSource, 1, new[] {alSoundFile.ALBuffer});
                ALPlayList.Add(alSoundFile);
                ALThread.CheckError($"queuing single {alSoundFile.ALBuffer} in source {AlSource}");

                // Play it.
                Al.SourcePlay(AlSource);
                UpdatePlayingState();
                ALThread.CheckError($"playing single {alSoundFile.ALBuffer} in source {AlSource}");

                Engine.Log.Info($"Started playing [{file.Name}] on {this}.", MessageSource.SoundManager);
            }

            // Check if forcing a fade out.
            return FadeOutOnChange ? SetupForceFadeOut(PlayInternAl) : ALThread.ExecuteALThread(PlayInternAl);
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public void Dispose()
        {
            ALThread.ExecuteALThread(() =>
            {
                Engine.Log.Info($"Destroyed {this}.", MessageSource.SoundManager);

                StopPlayingAll(true).Wait();
                Al.DeleteSource(AlSource);
                ALThread.CheckError($"cleanup of source {AlSource}");

                AlSource = 0;
                ALPlayList.Clear();
            }).Wait();
        }

        #endregion

        #region Force FadeOut Helpers

        /// <summary>
        /// Setups a forced fading out.
        /// </summary>
        /// <param name="action">The action to execute once its over.</param>
        private AwAction SetupForceFadeOut(Action action)
        {
            // Check if there is anything currently playing to fade out at All.
            if (CurrentlyPlayingFile == null || Status == SoundStatus.Stopped)
                return ALThread.ExecuteALThread(action);

            // Check if a force fade out is Already running.
            if (!_forceFadeOut)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                _forceFadeOut = true;
                _forceFadeOutStartDuration = PlaybackLocation;
                _forceFadeOutLength = MathExtension.Clamp(FadeOutLength, FadeOutLength, timeLeft);
                _forceFadeOutEndEvent = new AwAction(action);
            }
            else
            {
                // Chain action if a new one is added.
                _forceFadeOutEndEvent.ContinueWith(action);
            }

            Engine.Log.Info($"Performing smooth fade out on {this}.", MessageSource.SoundManager);
            return _forceFadeOutEndEvent;
        }

        #endregion

        /// <summary>
        /// Remove played buffers from the source queue. In the playlist they are replaced by nulls.
        /// </summary>
        private void RemovePlayed()
        {
            Al.GetSourcei(AlSource, Al.BuffersProcessed, out int processed);
            ALThread.CheckError($"checking processed buffers of source {AlSource}");

            if (processed <= 0) return;
            uint[] removed = new uint[processed];
            Al.SourceUnqueueBuffers(AlSource, processed, removed);
            Engine.Log.Trace($"removed {processed} buffers of source {AlSource} - {string.Join(", ", removed)}", MessageSource.SoundManager);
            ALThread.CheckError($"removing {processed} buffers of source {AlSource}");

            if (ALPlayList.Count > 0) ALPlayList.RemoveRange(0, Math.Min(ALPlayList.Count, processed));
            _isFirst = false;
        }

        /// <summary>
        /// Resets internAl trackers and variables.
        /// </summary>
        private void PerformReset()
        {
            _isFirst = true;
            ALPlayList.Clear();
            _forceFadeOut = false;
            _forceFadeOutLength = 0f;
            _forceFadeOutStartDuration = 0f;
            PlaybackLocation = 0;
        }

        private int UpdateCurrentlyPlayingFile()
        {
            // Check if anything playing.
            if (ALPlayList.Count == 0) return -1;

            // Accumulative playback tracker.
            float playback = 0;

            // Calculate currently playing through the playback and the playlist.
            for (int i = 0; i < ALPlayList.Count; i++)
            {
                SoundFile sf = ALPlayList[i];
                playback += sf.Duration;
                if (PlaybackLocation <= playback) return i;
            }

            // Unknown.
            Engine.Log.Warning($"Unknown currently playing file. {PlaybackLocation} with playlist {string.Join(", ", ALPlayList.Select(x => x.Name + ">" + x.Duration))}", MessageSource.SoundManager);

            return ALPlayList.Count - 1;
        }

        /// <summary>
        /// Update the layer. Is called on the ALThread by the sound manager.
        /// </summary>
        internal void Update()
        {
            // Check if buffer is initialized or destroyed.
            if (AlSource <= 0) return;

            ALThread.CheckError($"start update of source {AlSource}");

            // Update focused state. Manages pausing when focus is lost and resuming. The layer update should not proceed if paused.
            if (!UpdateFocusedState()) return;

            // Remove played buffers. StandAlone Al call.
            RemovePlayed();

            // Update playback location. StandAlone Al call.
            UpdatePlaybackLocation();

            // Update the currently playing file. This requires the playback location and a vetted playlist.
            UpdateCurrentFile();

            // Update paused/playing state.
            UpdatePlayingState();

            // Check whether the current track is last.
            bool last = ALPlayList.Count != 0 && CurrentlyPlayingFileIndex == ALPlayList.Count - 1;

            // Update volume. Requires an updated currently playing track.
            UpdateVolume(last);

            // Updates the looping state.
            UpdateLooping(last);

            // Check whether force fade out ended.
            if (_forceFadeOut)
            {
                float timeLeftForce = PlaybackLocation - _forceFadeOutStartDuration;
                if (timeLeftForce > _forceFadeOutLength || Status == SoundStatus.Stopped || timeLeftForce < 0)
                {
                    _forceFadeOut = false;
                    _forceFadeOutEndEvent.Run();
                }
            }

            ALThread.CheckError($"end update of source {AlSource}");
        }

        #region Update Parts

        private bool UpdateFocusedState()
        {
            // Check if the host exists.
            if (Engine.Host == null) return false;

            // Check if not focused.
            if (Engine.IsUnfocused)
                switch (Status)
                {
                    // If focus loss paused, paused or not playing don't do anything.
                    case SoundStatus.FocusLossPause:
                    case SoundStatus.Paused:
                    case SoundStatus.Stopped:
                        return false;
                    // If playing then focus loss pause.
                    case SoundStatus.Playing:
                        Al.SourcePause(AlSource);
                        Status = SoundStatus.FocusLossPause;
                        break;
                }
            // If focused and focus loss paused, resume.
            else if (!Engine.IsUnfocused && Status == SoundStatus.FocusLossPause)
                Al.SourcePlay(AlSource);

            return true;
        }

        private void UpdateVolume(bool last)
        {
            ALThread.CheckError($"before updating of volume of source {AlSource}");

            float systemVolume = Engine.SoundManager.Sound ? Engine.SoundManager.Volume / 100f : 0f;
            float scaled = MathExtension.Clamp(Volume / 100f * systemVolume, 0, 10);

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

            ReportedVolume = scaled;
            Al.Sourcef(AlSource, Al.Gain, scaled);
            ALThread.CheckError($"updating of volume of source {AlSource} to {scaled}");
        }

        private void UpdateLooping(bool last)
        {
            if (LoopLastOnly)
                if (last)
                {
                    RemovePlayed(); //?
                    Al.Sourcei(AlSource, Al.Looping, Looping ? 1 : 0);
                }
                else
                {
                    Al.Sourcei(AlSource, Al.Looping, 0);
                }
            else
                Al.Sourcei(AlSource, Al.Looping, Looping ? 1 : 0);

            ALThread.CheckError($"updating of looping of source {AlSource}");
        }

        private void UpdatePlayingState()
        {
            Al.GetSourcei(AlSource, Al.SourceState, out int status);

            switch (status)
            {
                case Al.Playing:
                    Status = SoundStatus.Playing;
                    break;
                case Al.Paused:
                    Status = !Engine.IsUnfocused ? SoundStatus.Paused : SoundStatus.FocusLossPause;
                    break;
                case Al.Initial:
                case Al.Stopped:
                    Status = SoundStatus.Stopped;
                    break;
            }

            ALThread.CheckError($"updating of state of source {AlSource}");
        }

        private void UpdateCurrentFile()
        {
            CurrentlyPlayingFileIndex = UpdateCurrentlyPlayingFile();

            if (CurrentlyPlayingFileIndex == -1 || CurrentlyPlayingFileIndex > ALPlayList.Count) CurrentlyPlayingFile = null;
            else CurrentlyPlayingFile = ALPlayList[CurrentlyPlayingFileIndex];

            TotalDuration = ALPlayList.Sum(x => x.Duration);
        }

        private void UpdatePlaybackLocation()
        {
            Al.GetSourcef(AlSource, Al.SecOffset, out float playbackLoc);
            PlaybackLocation = playbackLoc;

            ALThread.CheckError($"updating of playback location of source {AlSource}");
        }

        #endregion

        /// <summary>
        /// Convert to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[ALSoundLayer {Name}] {{ State:{Status}, Playback:{PlaybackLocation}/{TotalDuration}, Pointer:{AlSource} }}";
        }
    }
}