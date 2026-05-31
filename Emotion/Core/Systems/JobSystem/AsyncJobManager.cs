#nullable enable

using Emotion.Core.Utility.Coroutines;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

public class AsyncJobManager
{
    private const bool SINGLE_THREAD_DEBUG_MODE = false;

    public int ThreadCount { get => _threads?.Length ?? 1; }

    [ThreadStatic]
    private static JobThreadContext? _thisThreadContext;

    private JobThreadContext[]? _threads;
    private readonly JobWakeSignal _wakeSignal = new();
    private readonly ConcurrentDictionary<string, JobTagCounter> _jobTagCount = new();
    private CacheLinePaddedInt _submitCursor;

    public void Init()
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return;

        Engine.Log.Info("Initializing job system...", "Jobs");

        int threadCount = Environment.ProcessorCount - 1;
        threadCount = Math.Max(threadCount, 2);

        _threads = new JobThreadContext[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            Thread thread = new(JobSystemThreadProc)
            {
                IsBackground = true,
                Name = $"Job{i + 1}"
            };

            JobThreadContext threadContext = new(this, i, i == 0, thread);
            _threads[i] = threadContext;
        }

        for (int i = 0; i < threadCount; i++)
        {
            _threads[i].Thread.Start(_threads[i]);
        }

        Engine.Log.Info($"Started {threadCount} threads.", "Jobs");
    }

    private static void JobSystemThreadProc(object? context)
    {
        if (context is not JobThreadContext threadContext) return;
        _thisThreadContext = threadContext;

        AsyncJobManager manager = threadContext.Manager;
        int idleTicks = 0;
        while (Engine.Status != EngineState.Stopped)
        {
            threadContext.DrainInboxes();

            if (threadContext.TryGetJob(manager, out AsyncJobRoutine? job))
            {
                manager._wakeSignal.ResetIdle(ref idleTicks);
                RunJob(manager, threadContext, job);
                continue;
            }
            manager._wakeSignal.IdleWait(ref idleTicks);
        }

        _thisThreadContext = null;
    }

    #region Internal Job API

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TrySteal(AsyncJobManager manager, JobThreadContext threadContext, bool priority, [NotNullWhen(true)] out AsyncJobRoutine? job)
    {
        JobThreadContext[]? threads = manager._threads;
        if (threads == null || threads.Length <= 1)
        {
            job = null;
            return false;
        }

        int start = threadContext.StealCursor;
        for (int i = 0; i < threads.Length - 1; i++)
        {
            int victimIdx = (start + i + 1) % threads.Length;
            JobThreadContext victim = threads[victimIdx];
            if (victim == threadContext) continue;

            WorkStealingDeque queue = victim.GetJobQueue(priority);
            if (queue.TrySteal(out job))
            {
                threadContext.StealCursor = victimIdx;
                return true;
            }
        }

        threadContext.StealCursor = (start + 1) % threads.Length;
        job = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunJob(AsyncJobManager manager, JobThreadContext threadContext, AsyncJobRoutine job)
    {
        job.RunTask();

        if (!job.Finished)
        {
            WorkStealingDeque queue = threadContext.GetJobQueue(job.PriorityJob);
            queue.PushBottom(job);
            return;
        }

        manager.FinishJob(job);
    }

    #endregion

    #region External API

    public void Add(ISimpleAsyncJob func, bool priorityJob = false, string? jobTag = null)
    {
        if (SINGLE_THREAD_DEBUG_MODE)
        {
            func.Run();
            return;
        }

        RegisterJobTag(jobTag);
        AsyncJobRoutine job = AsyncJobRoutine.CreateFromPool(func, jobTag);
        InternalEnqueueJob(job, priorityJob);
    }

    public void AddNoFeedback(IEnumerator routineAsync, bool priorityJob = false, string? jobTag = null)
    {
        if (SINGLE_THREAD_DEBUG_MODE)
        {
            Engine.CoroutineManager.StartCoroutine(routineAsync);
            return;
        }

        RegisterJobTag(jobTag);
        AsyncJobRoutine job = AsyncJobRoutine.CreateFromPool(routineAsync, false, jobTag);
        InternalEnqueueJob(job, priorityJob);
    }

    public IRoutineWaiter Add(IEnumerator routineAsync, bool priorityJob = false, string? jobTag = null)
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return Engine.CoroutineManager.StartCoroutine(routineAsync);

        RegisterJobTag(jobTag);
        AsyncJobRoutine job = new(routineAsync, false, jobTag);
        InternalEnqueueJob(job, priorityJob);
        return job;
    }

    public bool NotManyJobsWithTag(string tag, int factor = 1)
    {
        if (!_jobTagCount.TryGetValue(tag, out JobTagCounter? counter)) return true;

        int many = Math.Max(ThreadCount, 2) * factor;
        return counter.Count < many;
    }

    #endregion

    private void InternalEnqueueJob(AsyncJobRoutine job, bool priorityJob)
    {
        JobThreadContext[]? threads = _threads;
        AssertNotNull(threads);
        job.PriorityJob = priorityJob;

        if (priorityJob)
        {
            JobThreadContext priorityWorker = threads[0];
            if (_thisThreadContext == priorityWorker)
                priorityWorker.GetJobQueue(true).PushBottom(job);
            else
                priorityWorker.PushToInbox(job, true);

            _wakeSignal.NotifyWork();
            return;
        }

        // If pushing a normal job from a worker thread, push it to the same thread.
        JobThreadContext? currentWorker = _thisThreadContext;
        if (currentWorker != null && !currentWorker.PriorityOnly)
        {
            currentWorker.GetJobQueue(false).PushBottom(job);
            _wakeSignal.NotifyWork();
            return;
        }

        int threadToPushTo = 0;
        if (threads.Length > 1)
        {
            int normalThreadCount = threads.Length - 1;
            threadToPushTo = 1 + (int)((uint)Interlocked.Increment(ref _submitCursor.Value) % (uint)normalThreadCount);
        }

        JobThreadContext target = threads[threadToPushTo];
        target.PushToInbox(job, false);
        _wakeSignal.NotifyWork();
    }

    private void RegisterJobTag(string? jobTag)
    {
        if (jobTag == null) return;

        JobTagCounter counter = _jobTagCount.GetOrAdd(jobTag, static (_) => new JobTagCounter());
        counter.Increment();
    }

    private void FinishJob(AsyncJobRoutine job)
    {
        if (job.JobTag != null && _jobTagCount.TryGetValue(job.JobTag, out JobTagCounter? counter))
            counter.Decrement();

        AsyncJobRoutine.ReturnToPoolIfFromPool(job);
    }

    #region Debug API

    internal int DebugOnly_GetThreadJobAmount(int threadId)
    {
        if (_threads == null) return -1;
        JobThreadContext thread = _threads[threadId];
        return thread.Debug_GetJobCount();
    }

    internal int DebugOnly_GetQueuedJobCount()
    {
        if (_threads == null) return 0;

        int total = 0;
        for (int i = 0; i < _threads.Length; i++)
        {
            JobThreadContext thread = _threads[i];
            total += thread.Debug_GetInboxCount();
        }

        return total;
    }

    #endregion
}
