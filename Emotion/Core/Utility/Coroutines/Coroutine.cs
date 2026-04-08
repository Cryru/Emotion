#nullable enable

namespace Emotion.Core.Utility.Coroutines;

public enum CoroutineStatus
{
    None,
    Running,
    RequestStop,
    Stopped,
    Finished
}

public abstract class Coroutine : IRoutineWaiter
{
    /// <summary>
    /// A value representing a completed routine.
    /// </summary>
    public static Coroutine CompletedRoutine { get; } = new CoroutineAlreadyCompleted();

    public CoroutineStatus Status = CoroutineStatus.None;

    public bool Stopped => Status == CoroutineStatus.Stopped;

    /// <summary>
    /// The waiter the routine is currently waiting on.
    /// </summary>
    public IRoutineWaiter? CurrentWaiter { get; internal set; }

    /// <summary>
    /// If non 0 then waiting for time.
    /// This is more precise than the "current waiter" yielding a timer as it ensures
    /// that the coroutines will be called exactly when this time passes and in the right order.
    /// </summary>
    public float CurrentWaiter_Time { get; internal set; }

    /// <summary>
    /// If not null then waiting for this routine to finish.
    /// </summary>
    public Coroutine? CurrentWaiter_SubRoutine { get; internal set; }

    /// <summary>
    /// The manager the routine is running on. Subroutines do not run on a
    /// manager but are considered an extension of the parent routine.
    /// </summary>
    public CoroutineManager Parent { get; init; }

    public abstract void Run(float dt);

    public void RequestStop()
    {
        if (Status != CoroutineStatus.Running) return;
        Status = CoroutineStatus.RequestStop;
    }

    #region IRoutineWaiter

    public bool Finished => Status == CoroutineStatus.Stopped || Status == CoroutineStatus.Finished;

    public void Update()
    {
        // nop
    }

    #endregion

    public static void RunInline(IEnumerator routine)
    {
        while (routine.MoveNext())
        {
            object current = routine.Current;
            if (current != null)
            {
                if (current is IEnumerator subRoutine)
                {
                    RunInline(subRoutine);
                }
                else if (current is IRoutineWaiter waiter)
                {
                    while (!waiter.Finished)
                        waiter.Update();
                }
            }
        }
    }

    public static Coroutine? CombineRoutines(Coroutine? a, Coroutine? b)
    {
        if (a == null) return b;
        if (b == null) return a;
        return Engine.CoroutineManager.StartCoroutine(CombineRoutineWaitRoutine(a, b));
    }

    private static IEnumerator CombineRoutineWaitRoutine(Coroutine a, Coroutine b)
    {
        yield return a;
        yield return b;
    }

    public static IEnumerator WhenAll(Coroutine[] routines)
    {
        for (int i = 0; i < routines.Length; i++)
        {
            yield return routines[i];
        }
    }

    public static IEnumerator WhenAll(List<Coroutine> routines)
    {
        for (int i = 0; i < routines.Count; i++)
        {
            yield return routines[i];
        }
    }
}

public sealed class CoroutineAlreadyCompleted : Coroutine
{
    public CoroutineAlreadyCompleted()
    {
        Parent = null!;
        Status = CoroutineStatus.Finished;
    }

    public override void Run(float dt)
    {
    }
}

/// <summary>
/// An object representing a coroutine created by the CoroutineManager to keep track of your routine.
/// To create a coroutine yourself, make a function which returns an IEnumerator and pass it to StartCoroutine.
/// This class is also used for subroutines in which case you want to construct it or yield to an IEnumerator.
/// </summary>
public sealed class CoroutineSpecialization<TScriptType> : Coroutine
    where TScriptType : ICoroutineScript
{
    private ICoroutineScript _routineScript;

    /// <summary>
    /// Create a new subroutine. Can also be achieved by yielding an IEnumerator.
    /// </summary>
    public CoroutineSpecialization(TScriptType routineScript, CoroutineManager parent)
    {
        Status = CoroutineStatus.Running;
        Parent = parent;
        _routineScript = routineScript;
    }

    /// <summary>
    /// Run the coroutine, this is called by the manager.
    /// </summary>
    public override void Run(float dt)
    {
        if (Status != CoroutineStatus.Running) return;
        if (Status == CoroutineStatus.RequestStop)
        {
            Status = CoroutineStatus.Stopped;
            CurrentWaiter_Time = 0;
            CurrentWaiter_SubRoutine = null;
            CurrentWaiter = null;
            return;
        }

        // One coroutine tick can end up advancing the coroutine script multiple times.
        while (RunInternal(ref dt))
        {
        }
    }

    private bool RunInternal(ref float timePassed)
    {
        AssertNotNull(_routineScript);

        if (CurrentWaiter_Time != 0)
        {
            CurrentWaiter_Time -= timePassed;

            // Handle EPSILON floats
            if (Maths.Approximately(CurrentWaiter_Time, 0.0f)) CurrentWaiter_Time = 0;

            // error handling just in case, cuz otherwise this will loop forever
            Assert(CurrentWaiter_Time >= 0);
            CurrentWaiter_Time = Math.Max(CurrentWaiter_Time, 0);

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

        CoroutineScriptRunResult runResult = _routineScript.RunStep(this);
        if (runResult == CoroutineScriptRunResult.Finished)
        {
            // It's over!
            Status = CoroutineStatus.Finished;
            return false;
        }

        return runResult == CoroutineScriptRunResult.ContinueRunning;
    }
}