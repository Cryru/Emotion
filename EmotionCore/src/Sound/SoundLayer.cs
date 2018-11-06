// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
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
        public SoundStatus Status { get; private set; } = SoundStatus.Stopped;

        /// <summary>
        /// Whether to loop the currently playing source.
        /// </summary>
        public bool Looping { get; set; }

        /// <summary>
        /// Loop the last queued track only instead of everything.
        /// </summary>
        public bool LoopLastOnly { get; set; } = true;

        /// <summary>
        /// The position currently playing within the current buffer in seconds.
        /// </summary>
        public float PlaybackLocation { get; private set; }

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
        private Queue<SoundFile> _playQueue;

        #endregion

        public SoundLayer(string name)
        {
            Name = name;
            _playQueue = new Queue<SoundFile>();

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
            ALThread.ExecuteALThread(() => { AL.SourcePlay(_pointer); });
        }

        /// <summary>
        /// Pause if playing.
        /// </summary>
        public void Pause()
        {
            if (Status != SoundStatus.Playing) return;
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, $"Paused {ToString()}.");
            ALThread.ExecuteALThread(() => { AL.SourcePause(_pointer); });
        }

        /// <summary>
        /// Stop playing any files.
        /// </summary>
        public void StopPlayingAll()
        {
            ALThread.ExecuteALThread(() =>
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Stopped {ToString()}.");
                AL.SourceStop(_pointer);
                _playQueue.Clear();
            });
        }

        /// <summary>
        /// Restart the current source.
        /// </summary>
        public void ResetCurrent()
        {
            ALThread.ExecuteALThread(() => { AL.SourceRewind(_pointer); });
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

                AL.SourceQueueBuffer(_pointer, file.Pointer);
                _playQueue.Enqueue(file);

                // Play if not playing.
                if (Status == SoundStatus.Stopped) AL.SourcePlay(_pointer);

                AL.GetSource(_pointer, ALGetSourcei.BuffersQueued, out int buffersLeft);
                Console.WriteLine("play " + buffersLeft + " " + Status);
            });
        }

        /// <summary>
        /// Destroy the layer freeing resources.
        /// </summary>
        public void Dispose()
        {
            Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Destroyed {ToString()}.");

            ALThread.ExecuteALThread(() => AL.DeleteSource(_pointer));
            _pointer = -1;
            _playQueue.Clear();
        }

        #endregion

        /// <summary>
        /// Update the layer. Is called on the ALThread by the sound manager.
        /// </summary>
        internal void Update()
        {
            // Update focused state.
            if (!UpdateFocusedState()) return;

            // Update volume.
            UpdateVolume();

            // Swap buffers which have played off the stack.
            PopPlayedSources();

            // Update paused/playing state.
            UpdatePlayingState();

            // Update current file.
            UpdateCurrentFile();

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

        private void UpdateVolume()
        {
            float systemVolume = Context.Settings.Sound ? Context.Settings.Volume : 0f;
            float scaled = MathHelper.Clamp(Volume * (systemVolume / 100f), 0, 10);

            // Perform fading.
            if (CurrentlyPlayingFile != null)
            {
                float timeLeft = CurrentlyPlayingFile.Duration - PlaybackLocation;

                // Check if fading in.
                if (PlaybackLocation < FadeInLength) scaled = MathHelper.Lerp(0, scaled, PlaybackLocation / FadeInLength);

                // Check if fading out.
                if (timeLeft < FadeOutLength) scaled = MathHelper.Lerp(0, scaled, timeLeft / FadeOutLength);
            }

            AL.Source(_pointer, ALSourcef.Gain, scaled);
            // todo: Check if this is needed
            // AL.Source(_pointer, ALSourcef.MaxGain, scaled < 0 ? 0f : 1f);
        }

        private void PopPlayedSources()
        {
            // Get the number of processed buffers.
            AL.GetSource(_pointer, ALGetSourcei.BuffersProcessed, out int processed);

            // Pop off as many buffers off the stack as they were processed.
            while (processed > 0)
            {
                Debugger.Log(MessageType.Info, MessageSource.SoundManager, $"Finished playing buffer of {ToString()}.");

                // Remove buffer.
                AL.SourceUnqueueBuffers(_pointer, 1);
                processed--;

                // Remove from internal queue. The check is here for when switching from appending to single.
                if (_playQueue.Count <= 0) continue;
                SoundFile file = _playQueue.Dequeue();

                // Loop if needed.
                if (!Looping) continue;
                // Check if looping on last.
                if (LoopLastOnly)
                {
                    if (_playQueue.Count != 0) continue;
                    QueuePlay(file);
                    AL.SourcePlay(_pointer);
                }
                else
                {
                    QueuePlay(file);
                    AL.SourcePlay(_pointer);
                }
            }
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
                case ALSourceState.Stopped:
                    Status = SoundStatus.Stopped;
                    break;
            }
        }

        private void UpdateCurrentFile()
        {
            AL.GetSource(_pointer, ALGetSourcei.Buffer, out int filePointer);
            if (filePointer == 0)
                CurrentlyPlayingFile = null;
            else
                foreach (SoundFile file in _playQueue)
                {
                    if (file.Pointer == filePointer) CurrentlyPlayingFile = file;
                }
        }

        private void UpdatePlaybackLocation()
        {
            AL.GetSource(_pointer, ALSourcef.SecOffset, out float playbackLoc);
            PlaybackLocation = playbackLoc;
        }

        #endregion

        public override string ToString()
        {
            return $"[Sound Layer] [ ALPointer: [{_pointer}] Name:[{Name}] Looping/LastOnly:[{Looping}/{LoopLastOnly}] Status:[{Status}] ]";
        }
    }
}