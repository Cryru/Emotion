// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Debug;
using Emotion.IO;
using Emotion.System;
using OpenTK.Audio.OpenAL;

#endregion

namespace Emotion.Sound
{
    /// <inheritdoc />
    public sealed class Source : SourceBase
    {
        #region Properties

        public override bool Looping
        {
            get
            {
                AL.GetSource(Pointer, ALSourceb.Looping, out bool value);
                return value;
            }
            set => AL.Source(Pointer, ALSourceb.Looping, value);
        }

        #endregion

        public Source(SoundFile file)
        {
            Pointer = AL.GenSource();
            AL.Source(Pointer, ALSourcei.Buffer, file.Pointer);
            FileName = file.Name;
            Duration = file.Duration;
        }

        #region Public API

        public override void Play()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Playing " + this);

            AL.SourcePlay(Pointer);
        }

        public override void Pause()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Paused " + this);

            AL.SourcePause(Pointer);
        }

        public override void Reset()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Reset " + this);

            AL.SourceRewind(Pointer);
        }

        public override void Stop()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Stopped " + this);

            AL.SourceStop(Pointer);
        }

        public override void ForceStop()
        {
            _eventTracker = true;
            Stop();
        }

        #endregion

        internal override void Destroy()
        {
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Destroyed " + this);

            AL.DeleteSource(Pointer);
            Pointer = -1;
        }

        internal override void Update(Settings settings)
        {
            // Check if destroyed.
            if (Pointer == -1) return;

            // Check if the sound levels have changed.
            if (settings.Sound || settings.Volume != 0)
            {
                float scaled = PersonalVolume * (settings.Volume / 100f);

                AL.Source(Pointer, ALSourcef.Gain, scaled);
                AL.Source(Pointer, ALSourcef.MaxGain, 1f);
            }
            else
            {
                AL.Source(Pointer, ALSourcef.Gain, 0);
                AL.Source(Pointer, ALSourcef.MaxGain, 0);
            }

            // Check if over or event was triggered.
            if (!Finished || _eventTracker) return;

            // Invoke finish event.
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Finished playing source: " + this);
            CallFinishedEvent();
        }
    }
}