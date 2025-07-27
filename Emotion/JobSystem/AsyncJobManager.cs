#nullable enable

using Emotion.Game.Routines;
using Emotion.Game.Time.Routines;
using Emotion.Utility;

namespace Emotion.JobSystem;

public class AsyncJobManager
{
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

        // Let's not get too greedy! :D
        threadCount = Maths.Clamp(threadCount, 1, 8);

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

        while (Engine.Status != EngineStatus.Stopped)
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
        Coroutine newRoutine = manager.StartCoroutine(routineAsync);

        _nextThreadToQueueOn++;
        _nextThreadToQueueOn = _nextThreadToQueueOn % _threads.Length;

        return newRoutine;
    }
}
