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
        private List<IRoutineWaiter> _runningRoutines = new List<IRoutineWaiter>();

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
                    IRoutineWaiter current = _runningRoutines[i];

                    // Update the current delay of the coroutine.
                    current?.Update();

                    // Check if the current delay is finished. If not, skip.
                    if (current != null && current.Finished != true) continue;

                    // Remove the routine from the list and decrement.
                    _runningRoutines.RemoveAt(i);
                    i--;
                }

                return true;
            }
        }
    }
}