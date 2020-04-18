using Emotion.Utility;

namespace Emotion.Audio.Fading
{
    /// <summary>
    /// Fade out the current track.
    /// </summary>
    public class AudioFadeOut : AudioModulationEffect
    {
        public static float FadeOutDebug = 1.0f;

        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="duration">The duration of the fade, in seconds.</param>
        public AudioFadeOut(float duration) : base(duration)
        {
        }

        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="progress">The duration of the fade, in track progress (0-1).</param>
        /// <param name="_">Placeholder for dual constructor.</param>
        public AudioFadeOut(float progress, bool _) : base(progress, _)
        {
        }

        public override void Apply(ref float sampleValue, ref float volume, float progress, float timestamp, AudioTrack track)
        {
            // The effect is applied past the timestamp.
            float fadeOutProgress;
            if (TimeStamp > 0.0)
            {
                float activationTimeStamp = track.File.Duration - TimeStamp;
                if (timestamp < activationTimeStamp) return;
                fadeOutProgress = 1.0f - (timestamp - activationTimeStamp) / TimeStamp;
            }
            else
            {
                float activationProgress = 1.0f + TimeStamp;
                if (progress < activationProgress) return;
                fadeOutProgress = 1.0f - (progress - activationProgress) / -TimeStamp;
            }
            volume *= Maths.Clamp01(fadeOutProgress);
        }
    }
}