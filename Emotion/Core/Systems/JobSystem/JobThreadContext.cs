#nullable enable

using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Emotion.Core.Systems.JobSystem;

public class JobThreadContext
{
    public bool ImportantJobsOnly { get; init; }

    public Channel<AsyncJobRoutine> PriorityQueue { get; init; }

    public Channel<AsyncJobRoutine> Queue { get; init; }

    public Thread Thread { get; init; }

    public ConcurrentDictionary<string, int> JobTagCount { get; init; }

    public int Metrics_JobCount = 0;

    public JobThreadContext(
        bool priorityOnly,
        Channel<AsyncJobRoutine> priorityQueue,
        Channel<AsyncJobRoutine> queue,
        Thread thread,
        ConcurrentDictionary<string, int> jobTagCount
    )
    {
        ImportantJobsOnly = priorityOnly;
        PriorityQueue = priorityQueue;
        Queue = queue;
        Thread = thread;
        JobTagCount = jobTagCount;
    }
}