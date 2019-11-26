#region Using

using System;
using Emotion.Standard.Audio;

#endregion

namespace Emotion.Audio
{
    public class AudioStreamerEffects : AudioStreamer
    {
        public float Volume;

        public AudioStreamerEffects(AudioFormat srcFormat, byte[] audioData) : base(srcFormat, audioData)
        {
        }

        protected override void SetSampleAsFloat(int index, float value, Span<byte> buffer)
        {
            value *= Volume;
            base.SetSampleAsFloat(index, value, buffer);
        }
    }
}