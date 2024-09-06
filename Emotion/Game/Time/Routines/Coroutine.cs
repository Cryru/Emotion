#region Using

using System.Collections;
using Emotion.Utility;
#if DEBUG
using System.Reflection;
#endif

#endregion

#nullable enable

namespace Emotion.Game.Time.Routines;

public enum CoroutineStatus
{
    None,
    Running,
    RequestStop,
    Stopped,
    Finished
}

/// <summary>
/// An object representing a coroutine created by the CoroutineManager to keep track of your routine.
/// To create a coroutine yourself, make a function which returns an IEnumerator and pass it to StartCoroutine.
/// This class is also used for subroutines in which case you want to construct it or yield to an IEnumerator.
/// </summary>
public sealed class Coroutine : IRoutineWaiter
{
    /// <summary>
    /// A value representing a completed routine.
    /// </summary>
    public static Coroutine CompletedRoutine = new Coroutine(null!, null!);

    public CoroutineStatus Status = CoroutineStatus.None;

    public bool Finished => Status == CoroutineStatus.Stopped || Status == CoroutineStatus.Finished;

    public bool Stopped => Status == CoroutineStatus.Stopped;

    /// <summary>
    /// The waiter the routine is currently waiting on.
    /// </summary>
    public IRoutineWaiter? CurrentWaiter { get; private set; }

    /// <summary>
    /// If non 0 then waiting for time.
    /// This is more precise than the "current waiter" yielding a timer as it ensures
    /// that the coroutines will be called exactly when this time passes and in the right order.
    /// </summary>
    public float CurrentWaiter_Time { get; private set; }

    /// <summary>
    /// If not null then waiting for this routine to finish.
    /// </summary>
    public Coroutine? CurrentWaiter_SubRoutine { get; private set; }

    /// <summary>
    /// The manager the routine is running on. Subroutines do not run on a
    /// manager but are considered an extension of the parent routine.
    /// </summary>
    public CoroutineManager Parent { get; }

    /// <summary>
    /// Actual C# iterator that is the routine.
    /// </summary>
    private IEnumerator? _routine;

    /// <summary>
    /// Create a new subroutine. Can also be achieved by yielding an IEnumerator.
    /// </summary>
    public Coroutine(IEnumerator enumerator, CoroutineManager parent)
    {
        if (enumerator == null)
            Status = CoroutineStatus.Finished; // CompletedRoutine
        else
            Status = CoroutineStatus.Running;

        Parent = parent;
        _routine = enumerator;
        Run(0);

#if DEBUG_STACKS
        string stackTrace = Environment.StackTrace;
        int startCoroutineFuncIdx = stackTrace.IndexOf("StartCoroutine", StringComparison.Ordinal);
        if (startCoroutineFuncIdx == -1) startCoroutineFuncIdx = stackTrace.IndexOf("Coroutine..ctor", StringComparison.Ordinal);
        int newLineAfterThat = startCoroutineFuncIdx != -1 ? stackTrace.IndexOf("\n", startCoroutineFuncIdx, StringComparison.Ordinal) : -1;
        if (newLineAfterThat != -1) stackTrace = stackTrace.Substring(newLineAfterThat + 1);
        DebugCoroutineCreationStack = stackTrace;
#endif
    }

    public void Update()
    {
        // nop
    }

    /// <summary>
    /// Run the coroutine, this is called by the manager.
    /// </summary>
    public void Run(float dt)
    {
        if (Status == CoroutineStatus.RequestStop)
        {
            Status = CoroutineStatus.Stopped;
            _routine = null;
            CurrentWaiter_Time = 0;
            CurrentWaiter_SubRoutine = null;
            CurrentWaiter = null;
            return;
        }

        if (Status != CoroutineStatus.Running) return;

        while (RunInternal(ref dt))
        {
        }
    }

    private bool RunInternal(ref float timePassed)
    {
        AssertNotNull(_routine);

        if (CurrentWaiter_Time != 0)
        {
            CurrentWaiter_Time -= timePassed;

            // Handle EPSILON floats
            if (Maths.Approximately(CurrentWaiter_Time, 0.0f)) CurrentWaiter_Time = 0;

            // error handling just in case, cuz otherwise this will loop forever
            Assert(CurrentWaiter_Time >= 0);
            if (CurrentWaiter_Time < 0) CurrentWaiter_Time = 0;

            if (CurrentWaiter_Time != 0)
                return false;

            // Reset the timer as we might come to a time yield again!
            timePassed = 0;
        }

        if (CurrentWaiter_SubRoutine != null)
        {
            if (CurrentWaiter_SubRoutine.Finished)
                CurrentWaiter_SubRoutine = null;
            else
                return false;
        }

        if (CurrentWaiter != null)
        {
            CurrentWaiter.Update();
            if (CurrentWaiter.Finished)
                CurrentWaiter = null;
            else
                return false;
        }

        Assert(CurrentWaiter_Time == 0);
        Assert(CurrentWaiter_SubRoutine == null);
        Assert(CurrentWaiter == null);

        // Increment the routine.
        bool incremented = _routine.MoveNext();
        if (!incremented)
        {
            // It's over!

            _routine = null;
            Status = CoroutineStatus.Finished;
            return false;
        }

        object? currentYield = _routine.Current;
        switch (currentYield)
        {
            // Check if a delay, and add it as the routine's delay.
            case IRoutineWaiter routineDelay:
                CurrentWaiter = routineDelay;
                break;
            // Check if adding a subroutine.
            case IEnumerator subroutine:
                CurrentWaiter_SubRoutine = Parent.StartCoroutine(subroutine);
                break;
            // Time waiting.
            case int timeWaiting:
                CurrentWaiter_Time = timeWaiting;
                break;
            case float timeWaitingF:
                CurrentWaiter_Time = timeWaitingF;
                break;
        }

        // Yielding null means wait one tick.
        if (currentYield == null) return false;

        // Run more!
        return true;
    }

    public void RequestStop()
    {
        if (Status != CoroutineStatus.Running) return;
        Status = CoroutineStatus.RequestStop;
    }

    public string DebugCoroutineCreationStack = string.Empty;
#if DEBUG

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