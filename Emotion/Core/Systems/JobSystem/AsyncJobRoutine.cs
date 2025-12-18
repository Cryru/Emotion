#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.JobSystem;

public class AsyncJobRoutine : IRoutineWaiter
{
    // State
    public bool FromPool { get; private set; }

    public bool Finished { get; private set; }

    // Data
    public string? JobTag { get; private set; }

    private IEnumerator _routine;
    private IRoutineWaiter? _currentWaiter;
    private bool _isSubRoutine;
    private ISimpleAsyncJob? _functor;

    public AsyncJobRoutine(IEnumerator routine, bool subRoutine = false, string? tag = null)
    {
        _routine = routine;
        _isSubRoutine = subRoutine;
        JobTag = tag;
    }

    private static ObjectPoolManual<AsyncJobRoutine> _asyncRoutinePool = new ObjectPoolManual<AsyncJobRoutine>(
        static () => new AsyncJobRoutine(),
        static (rot) => rot.Reset()
    );

    public static AsyncJobRoutine CreateFromPool(IEnumerator routine, bool subRoutine = false, string? tag = null)
    {
        AsyncJobRoutine newRoutine = _asyncRoutinePool.Get();
        newRoutine._routine = routine;
        newRoutine._isSubRoutine = subRoutine;
        newRoutine.JobTag = tag;

        return newRoutine;
    }

    public static AsyncJobRoutine CreateFromPool(ISimpleAsyncJob functor, string? tag = null)
    {
        AsyncJobRoutine newRoutine = _asyncRoutinePool.Get();
        newRoutine._functor = functor;
        newRoutine.JobTag = tag;

        return newRoutine;
    }

    public static void ReturnToPoolIfFromPool(AsyncJobRoutine rot)
    {
        if (rot.FromPool)
            _asyncRoutinePool.Return(rot);
    }

    internal AsyncJobRoutine()
    {
        FromPool = true;
        _routine = null!;
    }

    internal void Reset()
    {
        _routine = null!;
        _currentWaiter = null;
        JobTag = null;
        Finished = false;
        _isSubRoutine = false;
        _functor = null;
    }

    public void Update()
    {
        if (_isSubRoutine)
            RunTask();
    }

    public void RunTask()
    {
        // Short circuit for simple functor
        if (_functor != null)
        {
            _functor.Run();
            Finished = true;
            return;
        }

        Assert(!Finished);

        if (_currentWaiter != null)
        {
            _currentWaiter.Update();
            if (!_currentWaiter.Finished) return;
            if (_currentWaiter is AsyncJobRoutine subRot) ReturnToPoolIfFromPool(subRot);
            _currentWaiter = null;
        }

        bool done = true;
        while (_routine.MoveNext())
        {
            object current = _routine.Current;
            if (current == null)
            {
                done = false;
                break;
            }

            if (current is IEnumerator subRoutine)
            {
                _currentWaiter = CreateFromPool(subRoutine, true);
                done = false;
                break;
            }
            else if (current is IRoutineWaiter waiter)
            {
                waiter.Update();
                if (!waiter.Finished)
                {
                    _currentWaiter = waiter;
                    done = false;
                    break;
                }
            }
        }

        if (done)
            Finished = true;
    }
}