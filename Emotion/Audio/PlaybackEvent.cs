#region Using

using System.Runtime.CompilerServices;

#endregion

namespace Emotion.Audio
{
    public abstract class PlaybackEvent
    {
        public float TimeStamp;

        protected PlaybackEvent(float timeStamp)
        {
            TimeStamp = timeStamp;
        }

        protected PlaybackEvent(float progress, bool _)
        {
            TimeStamp = -progress;
        }

        public abstract void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float ProgressToFire(float progress, float timestamp)
        {
            if (progress == 0) return 0;

            if (TimeStamp < 0)
                return progress / -TimeStamp;
            return timestamp / TimeStamp;
        }
    }
}