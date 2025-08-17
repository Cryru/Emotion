#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.JobSystem;

public class AsyncJobManager
{
    public const int MANY_JOBS = 10;

    public int ThreadCount { get => _threads?.Length ?? 0; }

    private Thread[]? _threads;
    private AsyncJobCoroutineManager[]? _threadRoutineManagers;

    private int _nextThreadToQueueOn = 0;

    private const bool SINGLE_THREAD_DEBUG_MODE = false;

    public void Init()
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return;

        Engine.Log.Info("Initializing job system...", "Jobs");

        int threadCount = Environment.ProcessorCount - 1;
        threadCount = Math.Max(threadCount, 1);

        _threads = new Thread[threadCount];
        _threadRoutineManagers = new AsyncJobCoroutineManager[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            AsyncJobCoroutineManager manager = new AsyncJobCoroutineManager(i);
            _threadRoutineManagers[i] = manager;

            Thread thread = new(JobSystemThreadProc)
            {
                IsBackground = true
            };
            thread.Start(manager);
            _threads[i] = thread;
        }

        Engine.Log.Info($"Started {threadCount} threads.", "Jobs");
    }

    private static void JobSystemThreadProc(object? managerObject)
    {
        var manager = managerObject as AsyncJobCoroutineManager;
        if (manager == null) return;

        int threadId = manager.ThreadId;
        if (Engine.Host.NamedThreads) Thread.CurrentThread.Name ??= $"JobThread{threadId + 1}";

        while (Engine.Status != EngineState.Stopped)
        {
            manager.Update(0);
        }
    }

    public Coroutine Add(IEnumerator routineAsync)
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return Engine.CoroutineManager.StartCoroutine(routineAsync);

        AssertNotNull(_threadRoutineManagers);
        AssertNotNull(_threads);

        AsyncJobCoroutineManager manager = _threadRoutineManagers[_nextThreadToQueueOn];

        // Try to schedule on another thread if this one is too busy.
        if (manager.Count > MANY_JOBS)
        {
            int start = _nextThreadToQueueOn;
            for (int i = 1; i < _threadRoutineManagers.Length; i++)
            {
                AsyncJobCoroutineManager nextManager = _threadRoutineManagers[(start + i) % _threads.Length];
                if (nextManager.Count < MANY_JOBS)
                    manager = nextManager;
            }
        }

        Coroutine newRoutine = manager.StartCoroutine(routineAsync);

        _nextThreadToQueueOn++;
        _nextThreadToQueueOn = _nextThreadToQueueOn % _threads.Length;

        return newRoutine;
    }

    public int DebugOnly_GetThreadJobAmount(int threadId)
    {
        if (_threadRoutineManagers == null) return -1;
        return _threadRoutineManagers[threadId].Count;
    }
}
