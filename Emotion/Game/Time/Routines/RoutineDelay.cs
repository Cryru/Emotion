namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// A coroutine wait handle.
    /// </summary>
    public interface IRoutineWaiter
    {
        /// <summary>
        /// Whether the waiter has finished waiting, and the routine can proceed.
        /// </summary>
        bool Finished { get; }

        /// <summary>
        /// Updates the waiter. Should be run once per update loop.
        /// </summary>
        void Update();
    }
}