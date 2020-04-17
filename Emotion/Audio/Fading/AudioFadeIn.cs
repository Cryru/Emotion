using Emotion.Utility;

namespace Emotion.Audio.Fading
{
    /// <summary>
    /// Fade in the current track.
    /// </summary>
    public class AudioFadeIn : AudioModulationEffect
    {
        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="timestamp">The timestamp in seconds to end at.</param>
        public AudioFadeIn(float timestamp) : base(timestamp)
        {
        }

        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="progress">The progress timestamp (0-1) to end at.</param>
        /// <param name="_">Placeholder for dual constructor.</param>
        public AudioFadeIn(float progress, bool _) : base(progress, _)
        {
        }

        public override void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track)
        {
            volume *= Maths.Clamp01(ProgressToTimestamp(progress, timestamp));
        }
    }
}