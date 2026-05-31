#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

internal sealed class JobTagCounter
{
    private CacheLinePaddedInt _count;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Volatile.Read(ref _count.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Increment()
    {
        Interlocked.Increment(ref _count.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Decrement()
    {
        Interlocked.Decrement(ref _count.Value);
    }
}
