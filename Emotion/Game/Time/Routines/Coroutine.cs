#region Using

using System;
using System.Collections;
using System.Diagnostics;
#if DEBUG
using System.Reflection;
using System.Collections.Generic;

#endif

#endregion

namespace Emotion.Game.Time.Routines
{
    /// <summary>
    /// An object representing a coroutine created by the CoroutineManager to keep track of your routine.
    /// To create a coroutine yourself, make a function which returns an IEnumerator and pass it to StartCoroutine.
    /// This class is also used for subroutines in which case you want to construct it or yield to an IEnumerator.
    /// </summary>
    public class Coroutine : IRoutineWaiter
    {
        private IEnumerator _routine;
        private IRoutineWaiter _currentWaiter;

#if DEBUG
        public string DebugCoroutineCreationStack;
        public string DebugStackTrace;

        private int GetCoroutineStack()
        {
            FieldInfo[] fields = _routine.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo stateField = null;
            for (var i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name != "<>1__state") continue;
                stateField = fields[i];
                break;
            }

            if (stateField == null) return -1;

            return (int) (stateField.GetValue(_routine) ?? 1);
        }

        public static HashSet<Coroutine> CoroutinesRanThisTick = new HashSet<Coroutine>();
#endif

        /// <summary>
        /// Create a new subroutine. Can also be achieved by yielding an IEnumerator.
        /// </summary>
        /// <param name="enumerator">The routine's enumerator.</param>
        public Coroutine(IEnumerator enumerator)
        {
            _routine = enumerator;
#if DEBUG
            DebugCoroutineCreationStack = Environment.StackTrace;
#endif
        }

        /// <summary>
        /// Run the coroutine. This is called by either the manager or the parent routine if a subroutine.
        /// </summary>
        /// <returns>Whether the routine should be detached from the manager. Doesn't apply to subroutines.</returns>
        public virtual void Run()
        {
            if (Finished) return;

#if DEBUG
            Debug.Assert(CoroutinesRanThisTick.Add(this), "Each coroutine should run only once per tick. It's been added twice or yield added a second time.");
#endif

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
                        var subRtn = new Coroutine(subroutine);
                        _currentWaiter = subRtn;
#if DEBUG
                        subRtn.DebugStackTrace = $"At {GetCoroutineStack()} branched subroutine\n" + DebugStackTrace;
#endif
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
        public virtual bool Finished
        {
            get => _routine == null;
        }

        /// <summary>
        /// IRoutineWaiter API for subroutines
        /// </summary>
        public virtual void Update()
        {
            Run();
        }

        /// <summary>
        /// Stop running the routine. It will be thought of as finished.
        /// </summary>
        public virtual void Stop()
        {
            _currentWaiter = null;
            _routine = null;
        }
    }
}