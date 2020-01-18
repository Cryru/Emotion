#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// An object representing a coroutine.
    /// </summary>
    public sealed class Coroutine : IRoutineWaiter
    {
        /// <summary>
        /// Whether the routine has finished running.
        /// </summary>
        public bool Finished
        {
            get => Enumerator == null;
        }

        /// <summary>
        /// The routines enumerator.
        /// </summary>
        internal IEnumerator Enumerator;

        /// <summary>
        /// The waiter currently waiting on.
        /// </summary>
        internal IRoutineWaiter Waiter;

        /// <summary>
        /// Create a new coroutine. Is called by the CoroutineManager.
        /// </summary>
        /// <param name="enumerator">The routine's enumerator.</param>
        internal Coroutine(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        /// <summary>
        /// Stop the routine from running. Is also called when it finishes running.
        /// </summary>
        public void Stop()
        {
            Enumerator = null;
        }

        /// <summary>
        /// Dummy update method for the IRoutineWaiter interface.
        /// </summary>
        public void Update()
        {
            // noop
        }
    }
}