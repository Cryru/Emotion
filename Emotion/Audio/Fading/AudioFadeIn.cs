#region Using

using Emotion.Utility;

#endregion

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
        /// <param name="duration">The duration of the fade, in seconds.</param>
        public AudioFadeIn(float duration) : base(duration)
        {
        }

        /// <summary>
        /// Create a new fade out effect.
        /// </summary>
        /// <param name="progress">The duration of the fade, in track progress (0-1).</param>
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