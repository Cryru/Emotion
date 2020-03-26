#region Using

using System;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
    {
        /// <summary>
        /// The audio file this track is playing.
        /// </summary>
        public AudioAsset File { get; set; }

        /// <summary>
        /// Whether the track is playing on a layer.
        /// </summary>
        public bool HasLayer { get; set; }

        /// <summary>
        /// How far along the duration of the file the track has finished playing.
        /// </summary>
        public float Playback
        {
            get => Progress * File.Duration;
        }

        private float _volume = 1f;
        private List<PlaybackEvent> _playbackEvents;

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }

        /// <summary>
        /// Add a new event to the track's playback.
        /// </summary>
        /// <param name="newEvent">The event to add.</param>
        public void AddPlaybackEvent(PlaybackEvent newEvent)
        {
            if (_playbackEvents == null) _playbackEvents = new List<PlaybackEvent>();
            _playbackEvents.Add(newEvent);
        }

        public int GetNextVolumeModulatedFrames(float volume, int frameCount, Span<byte> buffer)
        {
            // The rest of this is in SetSampleAsFloat and volume is kept as a member
            // due to the complexity of the resampling process.
            _volume = volume;
            return base.GetNextFrames(frameCount, buffer);
        }

        protected override void SetSampleAsFloat(int index, float value, Span<byte> buffer)
        {
            float volume = _volume;

            if (_playbackEvents != null)
            {
                float progress = Progress;
                float playback = Playback;
                for (var i = 0; i < _playbackEvents.Count; i++)
                {
                    _playbackEvents[i].Apply(ref value, ref volume, progress, playback, this);
                }
            }

            volume = MathF.Pow(volume, Engine.Configuration.AudioCurve);
            value *= volume;
            base.SetSampleAsFloat(index, value, buffer);
        }
    }
}