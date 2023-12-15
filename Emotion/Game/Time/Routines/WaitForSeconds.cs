namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Wrapper over <see cref="Emotion.Game.Time.After" />.
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