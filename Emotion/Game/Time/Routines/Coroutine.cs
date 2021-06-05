#region Using

using System;
using System.Collections;
#if DEBUG
using System.Reflection;

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
        /// <summary>
        /// Whether the routine has finished running.
        /// If the routine was stopped, it will also be considered finished as to
        /// continue the calling routine.
        /// </summary>
        public virtual bool Finished
        {
            get => _routine == null;
        }

        /// <summary>
        /// Whether the routine was stopped rather than finished.
        /// </summary>
        public bool Stopped { get; protected set; }

        /// <summary>
        /// The waiter the routine is currently waiting on.
        /// </summary>
        public IRoutineWaiter CurrentWaiter { get; protected set; }

        /// <summary>
        /// The manager the routine is running on. Subroutines do not run on a manager but are considered an
        /// extension of the routine.
        /// </summary>
        public CoroutineManager Parent;

        private IEnumerator _routine;

        /// <summary>
        /// Create a new subroutine. Can also be achieved by yielding an IEnumerator.
        /// </summary>
        /// <param name="enumerator">The routine's enumerator.</param>
        public Coroutine(IEnumerator enumerator)
        {
            _routine = enumerator;
#if DEBUG
            string stackTrace = Environment.StackTrace;
            int startCoroutineFuncIdx = stackTrace.IndexOf("StartCoroutine", StringComparison.Ordinal);
            if(startCoroutineFuncIdx == -1) startCoroutineFuncIdx = stackTrace.IndexOf("Coroutine..ctor", StringComparison.Ordinal);
            int newLineAfterThat = startCoroutineFuncIdx != -1 ? stackTrace.IndexOf("\n", startCoroutineFuncIdx, StringComparison.Ordinal) : -1;
            if (newLineAfterThat != -1) stackTrace = stackTrace.Substring(newLineAfterThat + 1);
            DebugCoroutineCreationStack = stackTrace;
#endif
        }

        /// <summary>
        /// Run the coroutine. This is called by either the manager or the parent routine if a subroutine.
        /// </summary>
        /// <returns>Whether the routine should be detached from the manager. Doesn't apply to subroutines.</returns>
        public virtual void Run()
        {
            if (Finished) return;

            if (CurrentWaiter != null) // No waiter, or routine finished
            {
                CurrentWaiter.Update();
                if (!CurrentWaiter.Finished) return; // Current waiter is not ready. Continue waiting
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
                        CurrentWaiter = routineDelay;
                        break;
                    // Check if adding a subroutine.
                    case IEnumerator subroutine:
                        var subRtn = new Coroutine(subroutine);
                        CurrentWaiter = subRtn;
#if DEBUG
                        subRtn.DebugCoroutineCreationStack = $"Unknown subroutine stack. Yield a Coroutine object for full stack. Yield statement index {GetCoroutineStack()}";
#endif
                        break;
                }

                // If the delay is any other object it will delay execution by one coroutine tick.
                return;
            }

            CurrentWaiter = null;
            _routine = null;
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
            CurrentWaiter = null;
            _routine = null;
            Stopped = true;
        }

#if DEBUG
        public string DebugCoroutineCreationStack;

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
    }
}