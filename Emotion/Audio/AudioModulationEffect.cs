#region Using

using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Audio
{
    /// <summary>
    /// A base for audio modulation effects.
    /// </summary>
    public abstract class AudioModulationEffect
    {
        public float TimeStamp;

        protected AudioModulationEffect(float timeStamp)
        {
            TimeStamp = timeStamp;
        }

        protected AudioModulationEffect(float progress, bool _)
        {
            TimeStamp = -progress;
        }

        public abstract void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float ProgressToTimestamp(float progress, float timestamp)
        {
            if (progress == 0) return 0;

            if (TimeStamp < 0)
                return progress / -TimeStamp;
            return timestamp / TimeStamp;
        }
    }
}