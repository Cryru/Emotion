#region Using

using System.Collections;
using System.Reflection;

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

#if DEBUG
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
#endif

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