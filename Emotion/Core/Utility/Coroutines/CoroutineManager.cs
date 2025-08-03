#nullable enable

using Emotion;

namespace Emotion.Core.Utility.Coroutines;

/// <summary>
/// Manages coroutines and their execution.
/// </summary>
[DontSerialize]
public class CoroutineManager
{
    /// <summary>
    /// The current time in this coroutine manage.
    /// This is a sum of values passed into Update.
    /// </summary>
    public float Time;

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
    protected List<Coroutine> _routinesToAdd = new(4);

    /// <summary>
    /// Whether to run the routines inline when StartCoroutine is called. On by default.
    /// </summary>
    protected bool _eagerRoutines = true;

    /// <summary>
    /// Whether the manager supports time based waiting.
    /// </summary>
    public bool SupportsTime { get; protected set; } = true;

    public CoroutineManager(bool eagerRoutines = true)
    {
        _eagerRoutines = eagerRoutines;
    }

    /// <summary>
    /// Start a new coroutine.
    /// </summary>
    public virtual Coroutine StartCoroutine(IEnumerator enumerator)
    {
        var routine = new Coroutine(enumerator, this);
        if (_eagerRoutines) routine.Run(0);
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
    public virtual void Update(float timePassed = 0)
    {
        lock (_runningRoutines)
        {
            // Run non-time waiting routines first!
            RunTimeFrame(0, false);

            // Run time waiting routines in time frames until time is exhausted.
            float timeAccum = 0;
            while (timeAccum < timePassed)
            {
                // Find the smallest time step needed.
                float smallestTimeStep = timePassed - timeAccum;
                foreach (Coroutine routine in _runningRoutines)
                {
                    if (routine.CurrentWaiter_Time != 0 && routine.CurrentWaiter_Time < smallestTimeStep)
                        smallestTimeStep = routine.CurrentWaiter_Time;
                }

                RunTimeFrame(smallestTimeStep, true);
                timeAccum += smallestTimeStep;
            }

            _runningRoutines.RemoveAll(x => x.Finished);
        }
    }

    private void RunTimeFrame(float timeStep, bool timeOnly = false)
    {
        // Add routines that need adding.
        if (_routinesToAdd.Count > 0)
        {
            lock (_routinesToAdd)
            {
                _runningRoutines.AddRange(_routinesToAdd);
                _routinesToAdd.Clear();
            }
        }

        // Run routines in this order.
        // Routines which add other routines will be added next tick.
        Time += timeStep;
        foreach (Coroutine routine in _runningRoutines)
        {
            if (timeOnly && routine.CurrentWaiter_Time == 0) continue;

            Current = routine;
            routine.Run(timeStep);
            Current = null;
        }
    }

    public List<Coroutine> DbgGetRunningRoutines()
    {
        return new List<Coroutine>();
    }
}