using Emotion.Utility;

namespace Emotion.Audio.Fading
{
    public class FadeOutPlaybackEvent : PlaybackEvent
    {
        public FadeOutPlaybackEvent(float timestamp) : base(timestamp)
        {
        }

        public FadeOutPlaybackEvent(float progress, bool _) : base(progress, _)
        {
        }

        public override void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track)
        {
            // The effect is applied past the timestamp.
            float fadeInProgress = ProgressToFire(progress, timestamp);
            if (fadeInProgress < 1.0f) return;

            fadeInProgress = Maths.Clamp01(2.0f - fadeInProgress);
            volume *= fadeInProgress;
        }
    }
}