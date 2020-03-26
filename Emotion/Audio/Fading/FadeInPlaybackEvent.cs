using Emotion.Utility;

namespace Emotion.Audio.Fading
{
    public class FadeInPlaybackEvent : PlaybackEvent
    {
        public FadeInPlaybackEvent(float timestamp) : base(timestamp)
        {
        }

        public FadeInPlaybackEvent(float progress, bool _) : base(progress, _)
        {
        }

        public override void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track)
        {
            volume *= Maths.Clamp01(ProgressToFire(progress, timestamp));
        }
    }
}