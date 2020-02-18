#region Using

using System.Collections;
using System.Collections.Generic;

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Manages coroutines and their execution.
    /// </summary>
    public class CoroutineManager
    {
        /// <summary>
        /// The number of coroutines currently running.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return _runningRoutines.Count;
                }
            }
        }

        /// <summary>
        /// List of running routines.
        /// </summary>
        private readonly List<Coroutine> _runningRoutines = new List<Coroutine>();

        /// <summary>
        /// Start a new coroutine.
        /// </summary>
        /// <param name="enumerator">The enumerator of the coroutine.</param>
        /// <returns>An object representing your coroutine.</returns>
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            var routine = new Coroutine(enumerator);

            lock (this)
            {
                _runningRoutines.Add(routine);
            }

            return routine;
        }

        /// <summary>
        /// Stop all running routines.
        /// </summary>
        public void StopAll()
        {
            lock (this)
            {
                foreach (Coroutine r in _runningRoutines)
                {
                    r.Stop();
                }

                _runningRoutines.Clear();
            }
        }

        /// <summary>
        /// Update all running coroutines. Performs cleanup as well.
        /// </summary>
        /// <returns>Whether any routines were updated.</returns>
        public bool Update()
        {
            lock (this)
            {
                // If no routines are running, do nothing.
                if (_runningRoutines.Count <= 0) return false;

                for (var i = 0; i < _runningRoutines.Count; i++)
                {
                    Coroutine current = _runningRoutines[i];

                    // Update the current delay of the coroutine.
                    current.Waiter?.Update();

                    // Check if the current delay is finished. If not, skip.
                    if (current.Waiter != null && current.Waiter?.Finished != true) continue;

                    // Increment the routine.
                    bool? incremented = current.Enumerator?.MoveNext();
                    object currentYield = current.Enumerator?.Current;
                    if (incremented == true && currentYield != null)
                    {
                        switch (currentYield)
                        {
                            // Check if a delay, and add it as the routine's delay.
                            case IRoutineWaiter routineDelay:
                                current.Waiter = routineDelay;
                                break;
                            // Check if adding a subroutine.
                            case IEnumerator subroutine:
                                current.Waiter = StartCoroutine(subroutine);
                                break;
                        }

                        // If the delay is some other object it will delay execution by one loop.
                    }
                    else
                    {
                        // If there was no next, the routine is over. Perform cleanup.

                        // Mark as stopped.
                        current.Stop();

                        // Remove the routine from the list and decrement.
                        _runningRoutines.RemoveAt(i);
                        i--;
                    }
                }

                return true;
            }
        }
    }
}