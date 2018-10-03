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
    public sealed class StreamingSource : SourceBase
    {
        #region Properties

        /// <summary>
        /// Whether to loop the last file.
        /// </summary>
        public override bool Looping { get; set; }

        /// <summary>
        /// The id of the current file playing.
        /// </summary>
        public int FileId { get; private set; }

        /// <summary>
        /// The total number of files queued.
        /// </summary>
        public int FilesQueued
        {
            get
            {
                AL.GetSource(Pointer, ALGetSourcei.BuffersQueued, out int files);
                return files;
            }
        }

        #endregion

        private SoundFile[] _files;

        public StreamingSource(SoundFile[] files)
        {
            Pointer = AL.GenSource();

            FileName = files[0].Name;
            Duration = files[0].Duration;
            FileId = 0;

            _files = files;

            // Queue all files.
            foreach (SoundFile file in files)
            {
                AL.SourceQueueBuffer(Pointer, file.Pointer);
            }
        }

        #region API

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
            _files = null;
        }

        internal override void Update(Settings settings)
        {
            // Check if destroyed.
            if (Pointer == -1) return;

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

            // Check which file is playing.
            AL.GetSource(Pointer, ALGetSourcei.BuffersProcessed, out int curFile);
            if (curFile > 0)
                if (curFile != _files.Length)
                {
                    Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Streaming source " + this + " finished id " + FileId);

                    FileId = FileId + curFile;
                    Duration = _files[FileId].Duration;
                    FileName = _files[FileId].Name;

                    // Remove buffer.
                    AL.SourceUnqueueBuffers(Pointer, 1);

                    Debugger.Log(MessageType.Info, MessageSource.SoundManager, "Streaming source " + this + " starting id " + FileId);
                }

            // Check if last.
            if (curFile == _files.Length - 1) AL.Source(Pointer, ALSourceb.Looping, Looping);

            // Check if over or event was triggered.
            if (!Finished || _eventTracker) return;

            // Invoke finish event.
            Debugger.Log(MessageType.Trace, MessageSource.SoundManager, "Finished playing source: " + this);
            CallFinishedEvent();
        }
    }
}