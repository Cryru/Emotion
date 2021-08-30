#region Using

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

#nullable enable

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// Manages coroutines and their execution.
    /// </summary>
    public class CoroutineManager
    {
        /// <summary>
        /// The routine currently running. Subroutines are considered.
        /// Null if no routine is currently running. Can be used by coroutine functions
        /// to get a reference to their own routine.
        /// </summary>
        public Coroutine? Current { get; protected set; }

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
        private List<Coroutine> _runningRoutines = new();

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
                Debug.Assert(routine.Parent == null, "Each coroutine should only run within one manager.");
                routine.Parent = this;
                _runningRoutines.Add(routine);
            }

            return routine;
        }

        /// <summary>
        /// Stop a coroutine from running.
        /// </summary>
        /// <param name="routine">Reference to the coroutine.</param>
        public void StopCoroutine(Coroutine? routine)
        {
            if (routine == null) return;
            lock (this)
            {
                _runningRoutines.Remove(routine);
            }
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
        /// <returns>Whether any routines were ran.</returns>
        public virtual bool Update()
        {
            lock (this)
            {
                // If no routines are running, do nothing.
                if (_runningRoutines.Count == 0) return false;

                // Run routines in this order. Routines which add other routines will be added at the back of the queue.
                for (var i = 0; i < _runningRoutines.Count; i++)
                {
                    Coroutine current = _runningRoutines[i];
                    Current = current;
                    current.Run();
                }

                for (int i = _runningRoutines.Count - 1; i >= 0; i--)
                {
                    Coroutine current = _runningRoutines[i];
                    if (current.Finished) _runningRoutines.RemoveAt(i);
                }

                Current = null;
                return true;
            }
        }

#if DEBUG
        public List<Coroutine> DbgGetRunningRoutines()
        {
            lock (_runningRoutines)
            {
                return _runningRoutines;
            }
        }
#endif
    }
}