#nullable enable

using Emotion.Core.Utility.Coroutines;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Emotion.Core.Systems.JobSystem;

public class AsyncJobManager
{
    private const bool SINGLE_THREAD_DEBUG_MODE = false;

    public int ThreadCount { get => _threads?.Length ?? 1; }

    private JobThreadContext[]? _threads;
    private Channel<AsyncJobRoutine> _taskQueuePriority = null!;
    private Channel<AsyncJobRoutine> _taskQueue = null!;

    private ConcurrentDictionary<string, int> _jobTagCount = new();

    public void Init()
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return;

        _taskQueuePriority = Channel.CreateUnbounded<AsyncJobRoutine>();
        _taskQueue = Channel.CreateUnbounded<AsyncJobRoutine>();

        Engine.Log.Info("Initializing job system...", "Jobs");

        int threadCount = Environment.ProcessorCount - 1;
        threadCount = Math.Max(threadCount, 2);

        _threads = new JobThreadContext[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            Thread thread = new(JobSystemThreadProc)
            {
                IsBackground = true,
                Name = $"JobThread{i + 1}"
            };

            JobThreadContext threadContext = new(i == 0, _taskQueuePriority, _taskQueue, thread, _jobTagCount);
            thread.Start(threadContext);
            _threads[i] = threadContext;
        }

        Engine.Log.Info($"Started {threadCount} threads.", "Jobs");
    }

    private static void JobSystemThreadProc(object? context)
    {
        if (context is not JobThreadContext threadContext) return;

        Channel<AsyncJobRoutine> priorityChannel = threadContext.PriorityQueue;
        ChannelReader<AsyncJobRoutine> priorityReader = priorityChannel.Reader;

        Channel<AsyncJobRoutine> channel = threadContext.Queue;
        ChannelReader<AsyncJobRoutine> reader = channel.Reader;

        LinkedList<AsyncJobRoutine> jobs = new LinkedList<AsyncJobRoutine>();
        while (Engine.Status != EngineState.Stopped)
        {
            bool addedThisTick = false;

            if (priorityReader.TryRead(out AsyncJobRoutine? newPriorityJob))
            {
                addedThisTick = true;
                jobs.AddLast(newPriorityJob);
            }

            if (!threadContext.ImportantJobsOnly && !addedThisTick)
            {
                if (reader.TryRead(out AsyncJobRoutine? newJob))
                {
                    jobs.AddLast(newJob);
                }
            }

            // Run queued jobs
            int jobCount = 0;
            LinkedListNode<AsyncJobRoutine>? currentNode = jobs.First;
            while (currentNode != null)
            {
                jobCount++;
                AsyncJobRoutine val = currentNode.Value;
                val.RunTask();
                if (val.Finished)
                {
                    jobs.Remove(currentNode);

                    if (val.JobTag != null)
                    {
                        threadContext.JobTagCount.AddOrUpdate(
                           val.JobTag,
                           addValue: 0,
                           updateValueFactory: (_, oldValue) => oldValue - 1
                       );
                    }
                }
                currentNode = currentNode.Next;
            }

            threadContext.Metrics_JobCount = jobCount;

            // No jobs were ran, sleep.
            if (jobCount == 0)
                Thread.Sleep(10);
        }
    }

    public IRoutineWaiter Add(IEnumerator routineAsync, bool priorityJob = false, string? jobTag = null)
    {
        if (SINGLE_THREAD_DEBUG_MODE)
            return Engine.CoroutineManager.StartCoroutine(routineAsync);

        if (jobTag != null)
        {
            _jobTagCount.AddOrUpdate(
               jobTag,
               addValue: 1,
               updateValueFactory: (_, oldValue) => oldValue + 1
           );
        }

        AsyncJobRoutine job = new AsyncJobRoutine(routineAsync, false, jobTag);

        // Spin wait until we manage to write to the channel
        if (priorityJob)
        {
            while (!_taskQueuePriority.Writer.TryWrite(job))
            {
                Engine.Log.Warning("Failed to write task to priority job queue?", "Jobs", true);
                Thread.Yield();
            }
        }
        else
        {
            while (!_taskQueue.Writer.TryWrite(job))
            {
                Engine.Log.Warning("Failed to write task to job queue?", "Jobs", true);
                Thread.Yield();
            }
        }

        return job;
    }

    public bool NotManyJobsWithTag(string tag, int factor = 1)
    {
        if (!_jobTagCount.TryGetValue(tag, out int count)) return true;

        int many = Math.Max(ThreadCount, 2) * factor;
        return count < many;
    }

    public int DebugOnly_GetThreadJobAmount(int threadId)
    {
        if (_threads == null) return -1;
        return _threads[threadId].Metrics_JobCount;
    }

    public int DebugOnly_GetQueuedJobCount()
    {
        if (_taskQueue == null) return 0;
        return _taskQueue.Reader.Count;
    }
}
