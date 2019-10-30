#region Using

using System.Collections.Generic;
using Emotion.IO;
using Emotion.Utility;

#endregion

namespace Emotion.Audio
{
    /// <summary>
    /// Object for internal reference by the audio implementation of the platform.
    /// </summary>
    internal abstract class AudioLayer
    {
        /// <summary>
        /// The layer's friendly name.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The layer's volume.
        /// </summary>
        internal float Volume { get; set; }

        /// <summary>
        /// Whether the layer is currently considered playing.
        /// </summary>
        public bool Playing { get; protected set; }

        protected int _currentTrack;
        protected List<AudioTrack> _playlist = new List<AudioTrack>();
        protected bool _loopingCurrent;

        public EmotionEvent OnLooped = new EmotionEvent();
        public EmotionEvent<string, string> OnTrackChanged = new EmotionEvent<string, string>();

        internal virtual void PlayNext(AudioAsset file)
        {
            _playlist.Insert(_currentTrack, new AudioTrack(file));
            if (!Playing) Play();
        }

        internal virtual void AddToQueue(AudioAsset file)
        {
            _playlist.Add(new AudioTrack(file));
        }

        internal virtual void SetLoopCurrent(bool loop)
        {
            _loopingCurrent = loop;
        }

        internal virtual void Play()
        {
            if (Playing) return;
            if (_playlist.Count == 0) return;

            Playing = true;
        }

        internal virtual void Pause()
        {
            if (!Playing) return;

            Playing = false;
        }

        internal virtual void Clear()
        {
            _playlist.Clear();
            _currentTrack = 0;
            Playing = false;
        }
    }
}