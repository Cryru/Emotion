#nullable enable

using System.Threading.Tasks;

namespace Emotion.Core.Utility.Coroutines2;

/// <summary>
/// This is an experiment to transition coroutines from iterators to async/await to take advantage of
/// runtime-async / async2 functionality coming in .net11 which includes no/less allocations for state machines
/// and better debugging - Hopefully the API doesn't need to change too much, but all routines will need to be rewritten :)
/// </summary>
public class CoroutineManager2
{
    /// <summary>
    /// The current time in this coroutine manage.
    /// This is a sum of values passed into Update.
    /// </summary>
    public float Time;

    private List<Coroutine2> _runningRoutines = new(16);
    private List<RoutineWaiter2> _currentWaiters = new(16); // todo: in this sytem we store waiters rather than routines :/

    /// <summary>
    /// Whether to run the routines inline when StartCoroutine is called. On by default.
    /// </summary>
    protected bool _eagerRoutines = true;

    public void StartCoroutine(Func<CoroutineManager2, ValueTask> routineFunc)
    {
        ValueTask task = CoroutineWrapper(routineFunc);
        _runningRoutines.Add(new Coroutine2() { CurrentWaiter = task }); // hmmm
    }

    private async ValueTask CoroutineWrapper(Func<CoroutineManager2, ValueTask> routineFunc)
    {
        if (!_eagerRoutines)
            await WaitTick();

        try
        {
            await routineFunc(this);
        }
        catch (Exception ex)
        {
            Engine.Log.Error(ex);
        }
    }

    public void Update(float timePassed)
    {
        // Run non-time waiting routines first!
        RunTimeFrame(0, false);

        // Run time waiting routines in time frames until time is exhausted.
        float timeAccum = 0;
        while (timeAccum < timePassed)
        {
            // Find the smallest time step needed.
            float smallestTimeStep = timePassed - timeAccum;
            foreach (RoutineWaiter2 waiter in _currentWaiters)
            {
                if (waiter.Finished) continue; // We need to check this as cleanup occurs after the end of an update
                if (waiter.CurrentWaiter_Time != 0 && waiter.CurrentWaiter_Time < smallestTimeStep)
                    smallestTimeStep = waiter.CurrentWaiter_Time;
            }

            RunTimeFrame(smallestTimeStep, true);
            timeAccum += smallestTimeStep;
        }

        // Remove inanctive waiters (back to the pool)
        foreach (RoutineWaiter2 waiter in _currentWaiters)
        {
            if (waiter.Finished) _pool.Push(waiter);
        }
        _currentWaiters.RemoveAll(static (x) => x.Finished);
    }

    private void RunTimeFrame(float timeStep, bool timeOnly)
    {
        Time += timeStep;

        // This needs to be a /for/ unlike the original CoroutineManager implementation
        // due to waiters being added by a waiter continuation - this is unlike the old
        // coroutinesToAdd since here we keep track of waiters and not coroutines.
        for (int i = 0; i < _currentWaiters.Count; i++)
        {
            RoutineWaiter2 waiter = _currentWaiters[i];
            if (waiter.Finished) continue; // We need to check this as cleanup occurs after the end of an update
            if (timeOnly && waiter.CurrentWaiter_Time == 0) continue;

            //Current = routine;
            waiter.CoroutineRun(timeStep);
            //Current = null;
        }
    }

    #region Waiter Creation (Temp)

    private Stack<RoutineWaiter2> _pool = new(16);

    public ValueTask<bool> WaitTime(float ms)
    {
        if (!_pool.TryPop(out RoutineWaiter2? waiter))
            waiter = new RoutineWaiter2();

        waiter.Initialize(ms);
        _currentWaiters.Add(waiter);

        return new ValueTask<bool>(waiter, waiter.Version);
    }

    public ValueTask<bool> WaitTick()
    {
        return WaitTime(0);
    }

    #endregion
}

public static class NewCoroutineDemo
{
    public static void Run()
    {
        CoroutineManager2 manager = new CoroutineManager2();

        manager.StartCoroutine(MyNewCoroutine);

        Thread.Sleep(1000);
        for (int i = 0; i < 1000; i++)
        {
            manager.Update(16);
        }
        bool a = true;
    }

    public static async ValueTask MyNewCoroutine(CoroutineManager2 manager)
    {
        await manager.WaitTime(100f);
        await SomeOtherAsyncMethod(manager);
        await manager.WaitTick();
    }

    public static async ValueTask SomeOtherAsyncMethod(CoroutineManager2 manager)
    {
        await manager.WaitTime(55);
    }
}