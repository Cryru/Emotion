#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.JobSystem;

public interface IKeepUpdated
{
    public float LastUpdateTime { get; set; }
}

public class LocalJobDistributorKeepUpdated<TContextObject> : LocalJobDistributor<TContextObject>
    where TContextObject : IKeepUpdated
{
    public LocalJobDistributorKeepUpdated(float percentOfThreads, Action<TContextObject>? preJobStart = null, Action<TContextObject>? onJobEnd = null) : base(percentOfThreads, preJobStart, onJobEnd)
    {
    }

    private List<TContextObject> _dirtyObjects = new List<TContextObject>();
    private HashSet<TContextObject> _dirtyHash = new HashSet<TContextObject>();

    public void MarkDirty(TContextObject obj)
    {
        if (_dirtyHash.Add(obj))
            _dirtyObjects.Add(obj);
    }

    public void Update(Func<TContextObject, IEnumerator> routineFactory)
    {
        if (_dirtyObjects.Count == 0) return;

        bool sorted = false;
        while (GetFreeThread(out int freeThreadId))
        {
            if (!sorted)
            {
                // Sort in reverse so we can pop out without rearranging the whole list
                _dirtyObjects.Sort(static (a, b) => MathF.Sign(b.LastUpdateTime - a.LastUpdateTime));
                sorted = true;
            }
            if (_dirtyObjects.Count == 0) break;

            TContextObject obj = _dirtyObjects[^1];
            obj.LastUpdateTime = Engine.TotalTime;
            _dirtyHash.Remove(obj);
            StartJobOnThread(freeThreadId, obj, routineFactory(obj));
            _dirtyObjects.RemoveAt(_dirtyObjects.Count - 1);
        }
    }
}

public class LocalJobDistributor<TContextObject>
{
    private Coroutine[] _currentJobs;
    private Action<TContextObject>? _preJobStart;
    private Action<TContextObject>? _onJobEnd;

    public LocalJobDistributor(float percentOfThreads, Action<TContextObject>? preJobStart = null, Action<TContextObject>? onJobEnd = null)
    {
        int threads = (int)(Engine.Jobs.ThreadCount * percentOfThreads);
        threads = Math.Max(threads, 1);

        _currentJobs = new Coroutine[threads];
        for (int i = 0; i < _currentJobs.Length; i++)
        {
            _currentJobs[i] = Coroutine.CompletedRoutine;
        }

        _preJobStart = preJobStart;
        _onJobEnd = onJobEnd;
    }

    public int GetFreeThread()
    {
        for (int i = 0; i < _currentJobs.Length; i++)
        {
            Coroutine routine = _currentJobs[i];
            if (routine.Finished)
                return i;
        }
        return -1;
    }

    public bool GetFreeThread(out int freeThreadId)
    {
        freeThreadId = GetFreeThread();
        return freeThreadId != -1;
    }

    public void StartJobOnThread(int threadId, TContextObject obj, IEnumerator routineAsync)
    {
        Assert(_currentJobs[threadId].Finished);
        _preJobStart?.Invoke(obj);
        if (_onJobEnd != null)
        {
            _currentJobs[threadId] = Engine.Jobs.Add(JobWrapper(obj, _onJobEnd, routineAsync));
        }
        else
        {
            _currentJobs[threadId] = Engine.Jobs.Add(routineAsync);
        }
    }

    private static IEnumerator JobWrapper(TContextObject obj, Action<TContextObject> onEnd, IEnumerator routineAsync)
    {
        yield return routineAsync;
        onEnd(obj);
    }
}
