// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.IO;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    public sealed class Source
    {
        #region Properties

        /// <summary>
        /// The name of the file playing through this source.
        /// </summary>
        public string FileName { get; private set; }

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

        internal Source(SoundFile file)
        {
            Pointer = AL.GenSource();
            AL.Source(Pointer, ALSourcei.Buffer, file.Pointer);
            FileName = file.AssetName;
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
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Resumed " + this);

            AL.SourcePlay(Pointer);
        }

        /// <summary>
        /// Pause the source.
        /// </summary>
        public void Pause()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Paused " + this);

            AL.SourcePause(Pointer);
        }

        /// <summary>
        /// Restart the source, starting from the beginning.
        /// </summary>
        public void Reset()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Reset " + this);

            AL.SourceRewind(Pointer);
        }

        /// <summary>
        /// Stop playing the source.
        /// </summary>
        public void Stop()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Stopped " + this);

            AL.SourceStop(Pointer);
        }

        /// <summary>
        /// Forcefully stop the source without running its OnFinished event.
        /// </summary>
        public void ForceStop()
        {
            _eventTracker = true;
            Stop();
        }


        #endregion

        internal void Destroy()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Destroyed " + this);

            AL.DeleteSource(Pointer);
            Pointer = -1;
        }

        /// <summary>
        /// Update the source to comply with the max volume in the settings.
        /// </summary>
        /// <param name="settings"></param>
        internal void Update(Settings settings)
        {
            // Check if the sound levels have changed.
            if (settings.Sound || settings.Volume == 0)
            {
                float scaled = PersonalVolume * (settings.Volume / 100f);

                AL.Source(Pointer, ALSourcef.Gain, scaled);
                AL.Source(Pointer, ALSourcef.MaxGain, 1f);
            }
            else
            {
                AL.Source(Pointer, ALSourcef.MaxGain, 0);
            }

            // Check if over or event was triggered.
            if (!Finished || _eventTracker) return;

            // Invoke finish event.
            _eventTracker = true;
            OnFinished?.Invoke(this, EventArgs.Empty);
        }

        #region Debugging

        public override string ToString()
        {
            string result = "[Source " + Pointer + "]";
            result += $"(file: {FileName}, loop: {Looping}, personalVolume: {PersonalVolume})";
            return result;
        }

        #endregion
    }
}