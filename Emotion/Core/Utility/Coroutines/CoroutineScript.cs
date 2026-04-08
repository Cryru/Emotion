#nullable enable

namespace Emotion.Core.Utility.Coroutines;

public enum CoroutineScriptRunResult
{
    SkipOneTick,
    ContinueRunning,
    Finished
}

public interface ICoroutineScript
{
    public CoroutineScriptRunResult RunStep(Coroutine hostingRoutine);
}

public struct CoroutineScriptEnumerator : ICoroutineScript
{
    private IEnumerator _routine;

    public CoroutineScriptEnumerator(IEnumerator routine)
    {
        _routine = routine;
    }

    public CoroutineScriptRunResult RunStep(Coroutine hostingRoutine)
    {
        bool hasNext = _routine.MoveNext();
        if (!hasNext) return CoroutineScriptRunResult.Finished;

        object? currentYield = _routine.Current;
        switch (currentYield)
        {
            // Yielding a coroutine will wait for it to complete.
            // Note that this other routine can be in another CoroutineManager.
            case Coroutine routine:
                hostingRoutine.CurrentWaiter_SubRoutine = routine;
                break;
            // Check if a delay, and add it as the routine's delay.
            case IRoutineWaiter routineDelay:
                hostingRoutine.CurrentWaiter = routineDelay;
                break;
            // Check if adding a subroutine.
            case IEnumerator subroutine:
                hostingRoutine.CurrentWaiter_SubRoutine = hostingRoutine.Parent.StartCoroutine(subroutine);
                break;
            // Time waiting.
            case int timeWaiting when hostingRoutine.Parent.SupportsTime:
                hostingRoutine.CurrentWaiter_Time = timeWaiting;
                break;
            case uint timeWaitingU when hostingRoutine.Parent.SupportsTime:
                hostingRoutine.CurrentWaiter_Time = timeWaitingU;
                break;
            case float timeWaitingF when hostingRoutine.Parent.SupportsTime:
                hostingRoutine.CurrentWaiter_Time = timeWaitingF;
                break;
        }

        // Yielding null means wait one tick
        return currentYield == null ? CoroutineScriptRunResult.SkipOneTick : CoroutineScriptRunResult.ContinueRunning;
    }
}