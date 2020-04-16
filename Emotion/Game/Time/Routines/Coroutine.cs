#region Using

using System.Collections;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// An object representing a coroutine
    /// created by the CoroutineManager to keep track of your routine.
    /// To create a coroutine make a function which returns an IEnumerator.
    /// </summary>
    public sealed class Coroutine : IRoutineWaiter
    {
        private IEnumerator _routine;
        private IRoutineWaiter _currentWaiter;

        /// <summary>
        /// Create a new coroutine.
        /// </summary>
        /// <param name="enumerator">The routine's enumerator.</param>
        public Coroutine(IEnumerator enumerator)
        {
            _routine = enumerator;
        }

        private void Run()
        {
            if (Finished) return;

            // Update the waiter.
            _currentWaiter?.Update();

            // Check if the current waiter is finished. If not, continue waiting.
            if (_currentWaiter != null && _currentWaiter?.Finished != true) return;

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

                // If the delay is some other object it will delay execution by one loop.
            }
            else
            {
                _currentWaiter = null;
                _routine = null;
            }
        }

        /// <summary>
        /// Whether the routine has finished running.
        /// </summary>
        public bool Finished { get => _routine == null; }
        public void Update()
        {
            Run();
        }

        /// <summary>
        /// Stop running the routine.
        /// </summary>
        public void Stop()
        {
            _currentWaiter = null;
            _routine = null;
        }
    }
}