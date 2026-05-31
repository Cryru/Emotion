#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

public sealed class MpscJobQueue
{
    private AsyncJobRoutine? _head;
    private CacheLinePaddedInt _count;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref _count.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(AsyncJobRoutine job)
    {
        AsyncJobRoutine? oldHead;
        do
        {
            oldHead = Volatile.Read(ref _head);
            job.QueueNext = oldHead;
        } while (Interlocked.CompareExchange(ref _head, job, oldHead) != oldHead);

        Interlocked.Increment(ref _count.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void DrainTo(WorkStealingDeque deque)
    {
        AsyncJobRoutine? list = Interlocked.Exchange(ref _head, null);
        if (list == null) return;

        int drained = 0;
        while (list != null)
        {
            AsyncJobRoutine node = list;
            list = node.QueueNext;
            node.QueueNext = null;
            deque.PushBottom(node);
            drained++;
        }

        Interlocked.Add(ref _count.Value, -drained);
    }
}
