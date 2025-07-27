using Emotion.Game.Time;

namespace Emotion.Game.Routines
{
    /// <summary>
    /// Wrapper over <see cref="After" />.
    /// This exists to parrot the class in Unity.
    /// </summary>
    public sealed class WaitForSeconds : After
    {
        /// <summary>
        /// Create a new time waiter.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait for.</param>
        public WaitForSeconds(float seconds) : base(seconds * 1000)
        {
        }
    }
}