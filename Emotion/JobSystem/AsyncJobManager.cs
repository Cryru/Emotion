#nullable enable

using Emotion.Game.Time.Routines;

namespace Emotion.JobSystem;

public class AsyncJobManager
{
    private Thread[]? _threads;
    private AsyncJobCoroutineManager[]? _threadRoutineManagers;

    private int _nextThreadToQueueOn = 0;

    public void Init()
    {
        Engine.Log.Info("Initializing job system...", "Jobs");

        int threadCount = 1;
        if (Environment.ProcessorCount > 2)
            threadCount = Environment.ProcessorCount - 1;

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
        if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= $"Job Thread {threadId}";

        while (Engine.Status != EngineStatus.Stopped)
        {
            manager.Update(0);
            Thread.Yield();
        }
    }

    public Coroutine Add(IEnumerator routineAsync)
    {
        AssertNotNull(_threadRoutineManagers);
        AssertNotNull(_threads);

        AsyncJobCoroutineManager manager = _threadRoutineManagers[_nextThreadToQueueOn];
        Coroutine newRoutine = manager.StartCoroutine(routineAsync);

        _nextThreadToQueueOn++;
        _nextThreadToQueueOn = _nextThreadToQueueOn % _threads.Length;

        return newRoutine;
    }
}
