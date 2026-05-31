#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

public sealed class JobThreadContext
{
    public AsyncJobManager Manager { get; init; }
    public int WorkerId { get; init; }
    public bool PriorityOnly { get; init; }
    public int StealCursor;

    public Thread Thread { get; init; }

    private MpscJobQueue _inbox { get; } = new();
    private MpscJobQueue _priorityInbox { get; } = new();
    private WorkStealingDeque _priorityJobs { get; } = new();
    private WorkStealingDeque _jobs { get; } = new();

    public JobThreadContext(AsyncJobManager manager, int workerId, bool priorityOnly, Thread thread)
    {
        Manager = manager;
        WorkerId = workerId;
        PriorityOnly = priorityOnly;
        Thread = thread;
    }

    public int Debug_GetInboxCount()
    {
        return _inbox.Count + _priorityInbox.Count;
    }

    public int Debug_GetJobCount()
    {
        return _inbox.Count + _priorityInbox.Count + _priorityJobs.Count + _jobs.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrainInboxes()
    {
        _priorityInbox.DrainTo(_priorityJobs);
        if (PriorityOnly) return;

        _inbox.DrainTo(_jobs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WorkStealingDeque GetJobQueue(bool priority)
    {
        return priority ? _priorityJobs : _jobs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushToInbox(AsyncJobRoutine job, bool priority)
    {
        if (priority)
            _priorityInbox.Enqueue(job);
        else
            _inbox.Enqueue(job);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetJob(AsyncJobManager manager, [NotNullWhen(true)] out AsyncJobRoutine? job)
    {
        if (_priorityJobs.TryPopBottom(out job))
            return true;

        if (AsyncJobManager.TrySteal(manager, this, true, out job))
            return true;

        if (PriorityOnly)
            return false;

        if (_jobs.TryPopBottom(out job))
            return true;

        if (AsyncJobManager.TrySteal(manager, this, false, out job))
            return true;

        return false;
    }
}
