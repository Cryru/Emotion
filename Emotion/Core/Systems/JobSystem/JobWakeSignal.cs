#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

internal sealed class JobWakeSignal
{
    private const int SpinBeforeYield = 24;
    private const int YieldBeforePark = 48;

    private readonly SemaphoreSlim _semaphore = new(0, int.MaxValue);
    private CacheLinePaddedInt _sleepingWorkers;
    private CacheLinePaddedInt _wakeTokens;
    private CacheLinePaddedInt _wakeVersion;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void NotifyWork()
    {
        Interlocked.Increment(ref _wakeVersion.Value);

        while (true)
        {
            int sleepers = Volatile.Read(ref _sleepingWorkers.Value);
            if (sleepers <= 0) return;

            int tokens = Volatile.Read(ref _wakeTokens.Value);
            if (tokens >= sleepers) return;

            if (Interlocked.CompareExchange(ref _wakeTokens.Value, tokens + 1, tokens) != tokens)
                continue;

            _semaphore.Release();
            return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IdleWait(ref int idleTicks)
    {
        idleTicks++;
        if (idleTicks < SpinBeforeYield)
        {
            Thread.SpinWait(32 << Math.Min(idleTicks, 8));
            return;
        }

        if (idleTicks < SpinBeforeYield + YieldBeforePark)
        {
            Thread.Yield();
            return;
        }

        int observedWakeVersion = Volatile.Read(ref _wakeVersion.Value);
        Interlocked.Increment(ref _sleepingWorkers.Value);
        if (Volatile.Read(ref _wakeVersion.Value) != observedWakeVersion)
        {
            Interlocked.Decrement(ref _sleepingWorkers.Value);
            return;
        }

        if (_semaphore.Wait(2))
            Interlocked.Decrement(ref _wakeTokens.Value);

        Interlocked.Decrement(ref _sleepingWorkers.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetIdle(ref int idleTicks)
    {
        idleTicks = 0;
    }
}
