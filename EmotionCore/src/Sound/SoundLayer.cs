// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Engine.Threading;
using Emotion.IO;
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

        /// <summary>
        /// The duration of the fade in effect.
        /// </summary>
        public int FadeInLength { get; set; }

        /// <summary>
        /// The duration of the fade out effect.
        /// </summary>
        public int FadeOutLength { get; set; }

        #endregion

        #region Internals

        private int _pointer;
        private List<SoundFile> _playList;

        #endregion

        public SoundLayer(string name)
        {
            Name = name;
            _playList = new List<SoundFile>();

            ALThread.ExecuteALThread(() =>
            {
                // Initiate source.
                _pointer = AL.GenSource();

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

                // Stop playback.
                AL.Source(_pointer, ALSourceb.Looping, false);
                AL.SourceStop(_pointer);
                Status = SoundStatus.Stopped;

                // Clear buffers.
                RemovePlayed();
                _playList.Clear();
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
                if (Status == SoundStatus.Stopped) StopPlayingAll();

                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playList.Add(file);

                // Play if not playing.
                if (Status != SoundStatus.Stopped) return;
                AL.SourcePlay(_pointer);
                Status = SoundStatus.Playing;
            });
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public void Dispose()
        {
            Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Destroyed {ToString()}.");

            ALThread.ExecuteALThread(() =>
            {
                StopPlayingAll();
                AL.DeleteSource(_pointer);
            });
            _pointer = -1;
            _playList.Clear();
        }

        #endregion

        private void RemovePlayed()
        {
            ALThread.ForceALThread();

            AL.GetSource(_pointer, ALGetSourcei.BuffersProcessed, out int processed);
            while (processed > 0)
            {
                AL.SourceUnqueueBuffers(_pointer, 1);
                processed--;

                // This sets the removed ones to null so the first and last positions are retained. The StopPlayingAll function will clear the list.
                int notNull = _playList.FindIndex(x => x != null);
                if (notNull != -1) _playList[notNull] = null;
            }
        }

        /// <summary>
        /// Update the layer. Is called on the ALThread by the sound manager.
        /// </summary>
        internal void Update()
        {
            // Update focused state.
            if (!UpdateFocusedState()) return;

            // Prepare variables in use by parts of the update.
            AL.GetSource(_pointer, ALGetSourcei.Buffer, out int filePointer);
            bool last = filePointer == _playList.LastOrDefault()?.Pointer;
            bool first = filePointer == _playList.FirstOrDefault()?.Pointer;

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
                if (timeLeft < FadeOutLength && last) scaled = MathHelper.Lerp(0, scaled, timeLeft / FadeOutLength);
            }

            AL.Source(_pointer, ALSourcef.Gain, scaled);
            // todo: Check if this is needed
            // AL.Source(_pointer, ALSourcef.MaxGain, scaled < 0 ? 0f : 1f);
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
        }

        private void UpdateCurrentFile(int currentFilePointer)
        {
            CurrentlyPlayingFile = currentFilePointer == 0 ? null : _playList.FirstOrDefault(x => x?.Pointer == currentFilePointer);
        }

        private void UpdatePlaybackLocation()
        {
            AL.GetSource(_pointer, ALSourcef.SecOffset, out float playbackLoc);
            PlaybackLocation = playbackLoc;
        }

        #endregion

        public override string ToString()
        {
            return $"[Sound Layer] [ ALPointer:[{_pointer}] Name:[{Name}] Looping/LastOnly:[{Looping}/{LoopLastOnly}] Status:[{Status}] ]";
        }
    }
}