// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.System;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    public abstract class SourceBase
    {
        #region Properties

        /// <summary>
        /// The name of the file playing through this source.
        /// </summary>
        public string FileName { get; protected set; }

        /// <summary>
        /// Whether the source is currently playing.
        /// </summary>
        public bool Playing
        {
            get => AL.GetSourceState(Pointer) == ALSourceState.Playing;
        }

        /// <summary>
        /// Whether the source is paused.
        /// </summary>
        public bool Paused
        {
            get => AL.GetSourceState(Pointer) == ALSourceState.Paused;
        }

        /// <summary>
        /// Whether the source has finished playing.
        /// </summary>
        public bool Finished
        {
            get => AL.GetSourceState(Pointer) == ALSourceState.Stopped;
        }

        /// <summary>
        /// Whether the source has been destroyed;
        /// </summary>
        public bool Destroyed
        {
            get => Pointer == -1;
        }

        /// <summary>
        /// Whether the source will continue playing after stopping.
        /// </summary>
        public abstract bool Looping { get; set; }

        /// <summary>
        /// The playback position pointer within the source in seconds.
        /// </summary>
        public float PlayPosition
        {
            get
            {
                AL.GetSource(Pointer, ALSourcef.SecOffset, out float r);
                return r;
            }
        }

        /// <summary>
        /// The total duration in seconds of the file being played.
        /// </summary>
        public float Duration { get; protected set; }

        /// <summary>
        /// Will be called when the source has finished playing. If the source is looping it will never be called.
        /// </summary>
        public event EventHandler OnFinished;

        #endregion

        /// <summary>
        /// The personal volume of the source.
        /// </summary>
        internal float PersonalVolume;

        internal int Pointer;

        /// <summary>
        /// Tracks whether the event has been fired.
        /// </summary>
        protected bool _eventTracker;

        #region API

        /// <summary>
        /// Play the source. If paused it is resumed.
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Pause the source.
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Restart the source, starting from the beginning.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Stop playing the source.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Forcefully stop the source without running its OnFinished event.
        /// </summary>
        public abstract void ForceStop();

        #endregion

        /// <summary>
        /// Destroy the source freeing resources. The sound files attached are not destroyed.
        /// </summary>
        internal abstract void Destroy();

        /// <summary>
        /// Update the source to comply with the max volume in the settings.
        /// </summary>
        /// <param name="settings"></param>
        internal abstract void Update(Settings settings);

        protected void CallFinishedEvent()
        {
            _eventTracker = true;
            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        #region Debugging

        public override string ToString()
        {
            string result = "[Source " + Pointer + " of type " + GetType() + "]";
            result += $"(file: {FileName}, loop: {Looping}, personalVolume: {PersonalVolume})";
            return result;
        }

        #endregion
    }
}