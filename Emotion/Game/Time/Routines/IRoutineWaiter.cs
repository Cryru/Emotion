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
        /// Runs an update tick on the waiter, must be ran in order for it to finish.
        /// </summary>
        void Update();
    }
}