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
    public class AudioLayer
    {
        /// <summary>
        /// The layer's friendly name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The layer's volume.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Whether the layer is currently considered playing.
        /// </summary>
        public bool Playing { get; protected set; }

        protected int _currentTrack = -1;
        protected List<AudioTrack> _playlist = new List<AudioTrack>();
        protected bool _loopingCurrent;

        public EmotionEvent OnLooped = new EmotionEvent();
        public EmotionEvent<string, string> OnTrackChanged = new EmotionEvent<string, string>();

        public AudioLayer(string name)
        {
            Name = name;
        }

        public virtual void PlayNext(AudioAsset file)
        {
            _playlist.Insert(_currentTrack + 1, new AudioTrack(file));
            if (_currentTrack == -1) _currentTrack = 0;
            if (!Playing) Resume();
        }

        public virtual void AddToQueue(AudioAsset file)
        {
            _playlist.Add(new AudioTrack(file));
        }

        public virtual void SetLoopCurrent(bool loop)
        {
            _loopingCurrent = loop;
        }

        public virtual void Resume()
        {
            if (Playing) return;
            if (_playlist.Count == 0) return;

            Playing = true;
        }

        public virtual void Pause()
        {
            if (!Playing) return;

            Playing = false;
        }

        public virtual void Clear()
        {
            _playlist.Clear();
            _currentTrack = 0;
            Playing = false;
        }

        public virtual void Dispose()
        {

        }
    }
}