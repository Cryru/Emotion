// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.IO;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    public class Source
    {
        #region Properties

        /// <summary>
        /// Whether the source is currently playing.
        /// </summary>
        public bool isPlaying
        {
            get => AL.GetSourceState(Pointer) == ALSourceState.Playing;
        }

        /// <summary>
        /// Whether the source has finished playing.
        /// </summary>
        public bool isOver
        {
            get => AL.GetSourceState(Pointer) == ALSourceState.Stopped;
        }

        /// <summary>
        /// Whether the source has been destroyed;
        /// </summary>
        public bool isDestroyed
        {
            get => Pointer == -1;
        }

        /// <summary>
        /// Whether the source will continue playing after stopping.
        /// </summary>
        public bool Looping
        {
            get
            {
                AL.GetSource(Pointer, ALSourceb.Looping, out bool value);
                return value;
            }
            set => AL.Source(Pointer, ALSourceb.Looping, value);
        }

        /// <summary>
        /// Whether the source was paused by a focus loss.
        /// </summary>
        internal bool FocusLossPaused { get; private set; }

        /// <summary>
        /// Will be called when the source has finished playing. If the source is looping it will never be called.
        /// </summary>
        public event EventHandler OnFinished;

        /// <summary>
        /// Tracks whether the event has been fired.
        /// </summary>
        private bool _eventTracker;

        /// <summary>
        /// The personal volume of the source.
        /// </summary>
        internal float PersonalVolume;

        #endregion

        internal int Pointer;

        internal Source(SoundFile file, float volume = 1f)
        {
            Pointer = AL.GenSource();
            AL.Source(Pointer, ALSourcei.Buffer, file.Pointer);
            PersonalVolume = volume;
        }

        #region Public API

        /// <summary>
        /// Play the source.
        /// </summary>
        public void Play()
        {
            Resume();
        }

        public void Resume()
        {
            // Check if was stopped.
            if (isOver) AL.SourceRewind(Pointer);

            AL.Source(Pointer, ALSourcef.Gain, PersonalVolume);
            AL.SourcePlay(Pointer);

            FocusLossPaused = false;
        }

        /// <summary>
        /// Pause the source.
        /// </summary>
        public void Pause()
        {
            AL.SourcePause(Pointer);
        }

        /// <summary>
        /// Stop playing the source.
        /// </summary>
        public void Stop()
        {
            AL.SourceStop(Pointer);
        }

        #endregion

        internal void FocusLossPause()
        {
            FocusLossPaused = true;
            Pause();
        }

        internal void Destroy()
        {
            AL.DeleteSource(Pointer);
            Pointer = -1;
        }

        internal void Update(Settings settings)
        {
            // Check if the sound levels have changed.
            if (settings.Sound)
            {
                AL.Source(Pointer, ALSourcef.Gain, PersonalVolume);
                AL.Source(Pointer, ALSourcef.MaxGain, settings.Volume / 100f);
            }
            else
            {
                AL.Source(Pointer, ALSourcef.MaxGain, 0);
            }

            // Check if over or event was triggered.
            if (!isOver || _eventTracker) return;

            // Invoke finish event.
            _eventTracker = true;
            OnFinished?.Invoke(this, EventArgs.Empty);
        }
    }
}