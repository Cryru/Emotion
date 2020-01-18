#region Using

using System;
using Emotion.IO;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioTrack : AudioStreamer
    {
        public float Volume;
        public AudioAsset File { get; set; }

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