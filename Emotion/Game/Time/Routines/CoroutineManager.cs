#region Using

using System.Collections;

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
        /// List of routines waiting on a specific time to pass, sorted by time left.
        /// This list is rebuilt on execution, the list here is just a cache to prevent reallocation.
        /// </summary>
        private List<Coroutine> _timeWaitingRoutines = new();

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
                Assert(routine.Parent == null, "Each coroutine should only run within one manager.");
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
        public virtual bool Update(float timePassed = 0)
        {
            lock (this)
            {
                _timeWaitingRoutines.Clear();

                // If no routines are running, do nothing.
                if (_runningRoutines.Count == 0) return false;

                // Run routines in this order. Routines which add other routines will be added at the back of the queue.
                for (var i = 0; i < _runningRoutines.Count; i++)
                {
                    Coroutine current = _runningRoutines[i];

                    // Don't run the routine waiter - it's waiting for precise time.
                    if (current.WaitingForTime != 0)
                    {
                        _timeWaitingRoutines.Add(current);
                        continue;
                    }

                    Current = current;
                    current.Run();

                    // Check if it just yielded a time wait.
                    // Usually we don't run the same routine twice in a tick (such as like when yielding subroutines)
                    // but that is considered a side effect that must be corrected. New features like the time precision wait
                    // go around this to prevent precision issues.
                    if (current.WaitingForTime != 0)
                    {
                        _timeWaitingRoutines.Add(current);
                        continue;
                    }
                }

                // Advance routines waiting for time.
                if (_timeWaitingRoutines.Count > 0)
                    RunCoroutinesWaitingForTime(_timeWaitingRoutines, timePassed);

                // Cleanup finished routines.
                for (int i = _runningRoutines.Count - 1; i >= 0; i--)
                {
                    Coroutine current = _runningRoutines[i];
                    if (current.Finished)
                        _runningRoutines.RemoveAt(i);
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

        private int CoroutineTimeSort(Coroutine x, Coroutine y)
        {
            return MathF.Sign(x.WaitingForTime - y.WaitingForTime);
        }

        private void RunCoroutinesWaitingForTime(List<Coroutine> timeWaitingRoutines, float timePassed)
        {
            if (timePassed == 0) return;

            float timeAdvanced = 0;
            timeWaitingRoutines.Sort(CoroutineTimeSort);

            while (timeAdvanced < timePassed)
            {
                // The smallest wait determines the timestamp to ensure time order.
                float timeStep = timePassed - timeAdvanced;
                for (int i = 0; i < timeWaitingRoutines.Count; i++)
                {
                    var routine = timeWaitingRoutines[i];
                    if (routine.WaitingForTime != 0)
                    {
                        // Step needs to be smaller.
                        if (routine.WaitingForTime < timeStep)
                            timeStep = routine.WaitingForTime;
                        break;
                    }
                }

                for (int i = 0; i < timeWaitingRoutines.Count; i++)
                {
                    var routine = timeWaitingRoutines[i];
                    if (routine.WaitingForTime == 0) continue;
                    routine.WaitingForTime_AdvanceTime(timeStep);
                }

                for (int i = timeWaitingRoutines.Count - 1; i >= 0; i--)
                {
                    var routine = timeWaitingRoutines[i];
                    if (routine.WaitingForTime == 0)
                    {
                        // Time passed, continue the routine.
                        Current = routine;
                        routine.Run();
                    }
                }

                timeAdvanced += timeStep;
            }
            Assert(timeAdvanced == timePassed);
        }
    }
}