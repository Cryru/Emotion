#nullable enable

using System.Collections.Concurrent;

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
    protected Lock _routinesToAddLock = new Lock();
    protected Lock _runningRoutinesLock = new Lock();

    /// <summary>
    /// Whether to run the routines inline when StartCoroutine is called. On by default.
    /// </summary>
    protected bool _eagerRoutines = true;

    /// <summary>
    /// Whether the manager supports time based waiting.
    /// </summary>
    public bool SupportsTime { get; set; } = true;

    public CoroutineManager(bool eagerRoutines = true)
    {
        _eagerRoutines = eagerRoutines;
    }

    /// <summary>
    /// Start a new coroutine.
    /// </summary>
    public Coroutine StartCoroutine(IEnumerator enumerator)
    {
        return StartCoroutine(new CoroutineScriptEnumerator(enumerator));
    }

    public Coroutine StartCoroutine<TScriptType>(TScriptType script)
        where TScriptType : ICoroutineScript
    {
        var routine = new CoroutineSpecialization<TScriptType>(script, this);
        if (_eagerRoutines) routine.Run(0);
        if (!routine.Finished)
        {
            lock (_routinesToAddLock)
            {
                _routinesToAdd.Add(routine);
            }
        }

        return routine;
    }

    private Dictionary<Type, ConcurrentQueue<Coroutine>> _coroutinePool = new();

    /// <summary>
    /// Start a new coroutine with returning it.
    /// This allows the coroutine to not be allocated but rather object pooled
    /// </summary>
    public void StartCoroutineUntracked<TScriptType>(TScriptType script)
        where TScriptType : ICoroutineScript
    {
        Type scriptType = typeof(TScriptType);
        if (!_coroutinePool.TryGetValue(scriptType, out ConcurrentQueue<Coroutine>? typePool))
        {
            typePool = new ConcurrentQueue<Coroutine>();
            _coroutinePool.Add(scriptType, typePool);
        }

        if (typePool.TryDequeue(out Coroutine? routine))
        {
            var spec = (CoroutineSpecialization<TScriptType>)routine;
            spec.Recycle(script);
        }
        else
        {
            var spec = new CoroutineSpecialization<TScriptType>(script, this);
            spec.AssignPool(typePool);
            routine = spec;
        }

        if (_eagerRoutines)
            routine.Run(0);

        if (!routine.Finished)
        {
            lock (_routinesToAddLock)
            {
                _routinesToAdd.Add(routine);
            }
        }
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
        lock (_routinesToAddLock)
        {
            foreach (Coroutine routine in _routinesToAdd)
            {
                routine.RequestStop();
            }
        }

        lock (_runningRoutinesLock)
        {
            foreach (Coroutine routine in _runningRoutines)
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
        lock (_runningRoutinesLock)
        {
            Update_ProcessRoutinesToBeAdded();

            // Run non-time waiting routines first!
            Update_RunTimeFrame(0, false);

            // Run time waiting routines in time frames until time is exhausted.
            float timeAccum = 0;
            while (timeAccum < timePassed)
            {
                Update_ProcessRoutinesToBeAdded();

                // Find the smallest time step needed.
                float smallestTimeStep = timePassed - timeAccum;
                foreach (Coroutine routine in _runningRoutines)
                {
                    if (routine.CurrentWaiter_Time != 0 && routine.CurrentWaiter_Time < smallestTimeStep)
                        smallestTimeStep = routine.CurrentWaiter_Time;
                }

                Update_RunTimeFrame(smallestTimeStep, true);
                timeAccum += smallestTimeStep;
            }

            _runningRoutines.RemoveAll(x => x.Finished);
        }
    }

    private void Update_ProcessRoutinesToBeAdded()
    {
        lock (_routinesToAddLock)
        {
            if (_routinesToAdd.Count == 0) return;
            _runningRoutines.AddRange(_routinesToAdd);
            _routinesToAdd.Clear();
        }
    }

    private void Update_RunTimeFrame(float timeStep, bool timeOnly = false)
    {
        // Run routines in added order.
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
        return _runningRoutines;
    }
}