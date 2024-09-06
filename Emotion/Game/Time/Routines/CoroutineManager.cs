#region Using

using System.Collections;

#endregion

#nullable enable

namespace Emotion.Game.Time.Routines;

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
    public int Count => _runningRoutines.Count + _routinesToAdd.Count;

    /// <summary>
    /// List of running routines.
    /// </summary>
    protected List<Coroutine> _runningRoutines = new(16);

    /// <summary>
    /// List of routines waiting on a specific time to pass, sorted by time left.
    /// This list is rebuilt on execution, the list here is just a cache to prevent reallocation.
    /// </summary>
    protected List<Coroutine> _timeWaitingRoutines = new(2);

    /// <summary>
    /// Whether currently running a routine update.
    /// </summary>
    private bool _inUpdate;

    private List<Coroutine> _routinesToAdd = new List<Coroutine>();

    /// <summary>
    /// Start a new coroutine.
    /// </summary>
    public virtual Coroutine StartCoroutine(IEnumerator enumerator)
    {
        var routine = new Coroutine(enumerator, this);
        if (!routine.Finished)
        {
            lock (_routinesToAdd)
            {
                _routinesToAdd.Add(routine);
            }
        }

        return routine;
    }

    /// <summary>
    /// Stop a coroutine from running.
    /// </summary>
    public void StopCoroutine(Coroutine? routine)
    {
        if (routine == null) return;
        routine.RequestStop();
    }

    /// <summary>
    /// Stop all running routines.
    /// </summary>
    public void StopAll()
    {
        lock (_routinesToAdd)
        {
            foreach (var routine in _routinesToAdd)
            {
                routine.RequestStop();
            }
        }

        lock (_runningRoutines)
        {
            foreach (var routine in _runningRoutines)
            {
                routine.RequestStop();
            }
        }
    }

    /// <summary>
    /// Update all running coroutines. Performs cleanup as well.
    /// </summary>
    /// <returns>Whether any routines were ran.</returns>
    public virtual bool Update(float timePassed = 0)
    {
        lock (_runningRoutines)
        {
            if (_routinesToAdd.Count > 0)
            {
                lock (_routinesToAdd)
                {
                    _runningRoutines.AddRange(_routinesToAdd);
                    _routinesToAdd.Clear();
                }
            }

            // Run routines in this order. Routines which add other routines will be added at the back of the queue.
            foreach (Coroutine routine in _runningRoutines)
            {
                Current = routine;
                routine.Run(timePassed);
                Current = null;
            }

            _runningRoutines.RemoveAll(x => x.Finished);
        }




        //lock (this)
        //{
        //    _timeWaitingRoutines.Clear();

        //    // If no routines are running, do nothing.
        //    if (_runningRoutines.Count == 0) return false;

        //    _inUpdate = true;

        //    // Run routines in this order. Routines which add other routines will be added at the back of the queue.
        //    for (var i = 0; i < _runningRoutines.Count; i++)
        //    {
        //        Coroutine current = _runningRoutines[i];
        //        if (current.Stopped) continue;

        //        // Don't run the routine waiter - it's waiting for precise time.
        //        if (current.WaitingForTime != 0)
        //        {
        //            _timeWaitingRoutines.Add(current);
        //            continue;
        //        }

        //        Current = current;
        //        current.Update(timePassed);
        //        Current = null;

        //        // Check if it just yielded a time wait.
        //        // Usually we don't run the same routine twice in a tick (such as like when yielding subroutines)
        //        // but that is considered a side effect that must be corrected. New features like the time precision wait
        //        // go around this to prevent precision issues.
        //        if (current.WaitingForTime != 0)
        //        {
        //            _timeWaitingRoutines.Add(current);
        //            continue;
        //        }
        //    }

        //    // Advance routines waiting for time.
        //    if (_timeWaitingRoutines.Count > 0)
        //        RunCoroutinesWaitingForTime(_timeWaitingRoutines, timePassed);

        //    // Cleanup finished routines.
        //    for (int i = _runningRoutines.Count - 1; i >= 0; i--)
        //    {
        //        Coroutine current = _runningRoutines[i];
        //        if (current.Finished)
        //            _runningRoutines.RemoveAt(i);
        //    }

        //    Current = null;
        //    _inUpdate = false;
        //}

        return true;
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

    //private int CoroutineTimeSort(Coroutine x, Coroutine y)
    //{
    //    return MathF.Sign(x.WaitingForTime - y.WaitingForTime);
    //}

    //private void RunCoroutinesWaitingForTime(List<Coroutine> timeWaitingRoutines, float timePassed)
    //{
    //    if (timePassed == 0) return;

    //    float timeAdvanced = 0;
    //    timeWaitingRoutines.Sort(CoroutineTimeSort);

    //    while (timeAdvanced < timePassed)
    //    {
    //        // The smallest wait determines the timestamp to ensure time order.
    //        float timeStep = timePassed - timeAdvanced;
    //        for (int i = 0; i < timeWaitingRoutines.Count; i++)
    //        {
    //            var routine = timeWaitingRoutines[i];
    //            if (routine.WaitingForTime != 0)
    //            {
    //                // Step needs to be smaller.
    //                if (routine.WaitingForTime < timeStep)
    //                    timeStep = routine.WaitingForTime;
    //                break;
    //            }
    //        }

    //        for (int i = 0; i < timeWaitingRoutines.Count; i++)
    //        {
    //            var routine = timeWaitingRoutines[i];
    //            if (routine.WaitingForTime == 0) continue;
    //            routine.WaitingForTime_AdvanceTime(timeStep);
    //        }

    //        bool resort = false;
    //        for (int i = timeWaitingRoutines.Count - 1; i >= 0; i--)
    //        {
    //            var routine = timeWaitingRoutines[i];
    //            if (routine.WaitingForTime == 0)
    //            {
    //                // Time passed, continue the routine.
    //                Current = routine;
    //                routine.Update(timeAdvanced);
    //                Current = null;

    //                // Waiting on time again!
    //                if (routine.WaitingForTime != 0)
    //                    resort = true;
    //            }
    //        }

    //        if (resort) timeWaitingRoutines.Sort(CoroutineTimeSort);

    //        timeAdvanced += timeStep;
    //    }
    //    Assert(timeAdvanced == timePassed);
    //}
}