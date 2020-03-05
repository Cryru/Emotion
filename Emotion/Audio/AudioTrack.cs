#region Using

using System;
using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
    {
        /// <summary>
        /// The volume of the track.
        /// </summary>
        public float Volume;

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

        public AudioTrack(AudioAsset file) : base(file.Format, file.SoundData)
        {
            File = file;
        }

        protected override void SetSampleAsFloat(int index, float value, Span<byte> buffer)
        {
            value *= Volume;
            base.SetSampleAsFloat(index, value, buffer);
        }
    }
}