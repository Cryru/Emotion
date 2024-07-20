#region Using

using System.Collections;
using Emotion.Utility;
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
        /// A routine considered completed.
        /// </summary>
        public static Coroutine CompletedRoutine = new Coroutine(null);

        /// <summary>
        /// True if the routine is neither finished nor stopped.
        /// Note that this doesn't guarantee it's ever been ran (or added to a coroutine manager at all)
        /// </summary>
        public bool Active { get => !Finished && !Stopped; }

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
        /// The amount of time the coroutine is waiting to pass.
        /// This is more precise than the "current waiter" yielding a timer as it ensures
        /// that the coroutines will be called in succession.
        /// </summary>
        public float WaitingForTime { get; protected set; }

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
#if DEBUG_STACKS
            string stackTrace = Environment.StackTrace;
            int startCoroutineFuncIdx = stackTrace.IndexOf("StartCoroutine", StringComparison.Ordinal);
            if (startCoroutineFuncIdx == -1) startCoroutineFuncIdx = stackTrace.IndexOf("Coroutine..ctor", StringComparison.Ordinal);
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
            
            while (true)
            {
                if (CurrentWaiter != null) // No waiter, or routine finished
                {
                    CurrentWaiter.Update();

                    // If the current waiter is a subroutine, assign its waiting time to the parent routine so that the
                    // coroutine manager doesn't have to walk down each coroutine.
                    if (CurrentWaiter is Coroutine subRtn && subRtn.WaitingForTime != 0)
                        WaitingForTime = subRtn.WaitingForTime;

                    if (!CurrentWaiter.Finished) break; // Current waiter is not ready. Continue waiting
                }

                // Currently waiting on time
                if (WaitingForTime != 0) break;

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

                        // Time waiting.
                        case int timeWaiting:
                            CurrentWaiter = null;
                            WaitingForTime = timeWaiting;
                            break;
                        case float timeWaitingF:
                            CurrentWaiter = null;
                            WaitingForTime = timeWaitingF;
                            break;
                    }

                    // Yielding null means wait one tick.
                    if (currentYield == null) break;
                }
                else
                {
                    // It's over!
                    CurrentWaiter = null;
                    _routine = null;
                    break;
                }
            }
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

        public void WaitingForTime_AdvanceTime(float time)
        {
            // Check if waiting on a subroutine which is waiting for time
            if (CurrentWaiter is Coroutine subRtn && subRtn.WaitingForTime != 0)
            {
                subRtn.WaitingForTime_AdvanceTime(time);
                WaitingForTime = subRtn.WaitingForTime;
            }
            // I am myself waiting for time
            else
            {
                WaitingForTime -= time;
            }

            // Handle EPSILON floats
            if (Maths.Approximately(WaitingForTime, 0.0f))
                WaitingForTime = 0;

            Assert(WaitingForTime >= 0);

            // error handling just in case, cuz otherwise this will loop forever
            if (WaitingForTime < 0) WaitingForTime = 0;
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