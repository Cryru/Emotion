using Emotion.Utility;

namespace Emotion.Audio.Fading
{
    /// <summary>
    /// Fade out the current track.
    /// </summary>
    public class AudioFadeOut : AudioModulationEffect
    {
        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="timestamp">The timestamp in seconds to begin at.</param>
        public AudioFadeOut(float timestamp) : base(timestamp)
        {
        }

        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="progress">The progress timestamp (0-1) to begin at.</param>
        /// <param name="_">Placeholder for dual constructor.</param>
        public AudioFadeOut(float progress, bool _) : base(progress, _)
        {
        }

        public override void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track)
        {
            // The effect is applied past the timestamp.
            float fadeInProgress = ProgressToTimestamp(progress, timestamp);
            if (fadeInProgress < 1.0f) return;

            fadeInProgress = Maths.Clamp01(2.0f - fadeInProgress);
            volume *= fadeInProgress;
        }
    }
}