#nullable enable

using Emotion.Game.Time.Routines;

namespace Emotion.JobSystem;

public class AsyncJobManager
{
    private Thread[]? _threads;
    private AsyncJobCoroutineManager[]? _threadRoutineManagers;

    private int _nextThreadToQueueOn = 0;

    private const bool SINGLE_THREAD_DEBUG_MODE = false;

    public void Init()
    {
        Engine.Log.Info("Initializing job system...", "Jobs");

        int threadCount = 1;
        if (Environment.ProcessorCount > 2)
            threadCount = Environment.ProcessorCount - 1;

        // Let's not get too greedy! :D
        if (threadCount > 8)
            threadCount = 8;

        if (SINGLE_THREAD_DEBUG_MODE)
            threadCount = 0;

        _threads = new Thread[threadCount];
        _threadRoutineManagers = new AsyncJobCoroutineManager[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            AsyncJobCoroutineManager manager = new AsyncJobCoroutineManager();
            _threadRoutineManagers[i] = manager;

            int threadId = i;
            Thread thread = new Thread(() => JobSystemThreadProc(threadId, manager));
            thread.IsBackground = true;
            thread.Start();
            _threads[i] = thread;
        }

        Engine.Log.Info($"Started {threadCount} threads.", "Jobs");
    }

    private void JobSystemThreadProc(int threadId, AsyncJobCoroutineManager manager)
    {
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
