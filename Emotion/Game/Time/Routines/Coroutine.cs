#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// An object representing a coroutine created by the CoroutineManager to keep track of your routine.
    /// To create a coroutine yourself, make a function which returns an IEnumerator and pass it to StartCoroutine.
    /// This class is also used for subroutines in which case you want to construct it or yield to an IEnumerator.
    /// </summary>
    public sealed class Coroutine : IRoutineWaiter
    {
        private IEnumerator _routine;
        private IRoutineWaiter _currentWaiter;

        /// <summary>
        /// Create a new subroutine. Can also be achieved by yielding an IEnumerator.
        /// </summary>
        /// <param name="enumerator">The routine's enumerator.</param>
        public Coroutine(IEnumerator enumerator)
        {
            _routine = enumerator;
        }

        /// <summary>
        /// Run the coroutine. This is called by either the manager or the parent routine if a subroutine.
        /// </summary>
        /// <returns>Whether the routine should be detached from the manager. Doesn't apply to subroutines.</returns>
        public void Run()
        {
            if (Finished) return;

            if (_currentWaiter != null) // No waiter, or routine finished
            {
                _currentWaiter.Update();
                if (!_currentWaiter.Finished) return; // Current waiter is not ready. Continue waiting
            }

            // Increment the routine.
            bool? incremented = _routine?.MoveNext();
            object currentYield = _routine?.Current;

            if (incremented == true)
            {
                switch (currentYield)
                {
                    // Check if a delay, and add it as the routine's delay.
                    case IRoutineWaiter routineDelay:
                        _currentWaiter = routineDelay;
                        break;
                    // Check if adding a subroutine.
                    case IEnumerator subroutine:
                        _currentWaiter = new Coroutine(subroutine);
                        break;
                }

                // If the delay is any other object it will delay execution by one coroutine tick.
                return;
            }

            _currentWaiter = null;
            _routine = null;
        }

        /// <summary>
        /// Whether the routine has finished running.
        /// </summary>
        public bool Finished
        {
            get => _routine == null;
        }

        /// <summary>
        /// IRoutineWaiter API for subroutines
        /// </summary>
        public void Update()
        {
            Run();
        }

        /// <summary>
        /// Stop running the routine. It will be thought of as finished.
        /// </summary>
        public void Stop()
        {
            _currentWaiter = null;
            _routine = null;
        }
    }
}