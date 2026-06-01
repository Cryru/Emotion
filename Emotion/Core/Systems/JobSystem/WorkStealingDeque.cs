#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.JobSystem;

public sealed class WorkStealingDeque
{
    private const int InitialCapacity = 256;

    private AsyncJobRoutine?[] _buffer = new AsyncJobRoutine?[InitialCapacity];
    private CacheLinePaddedLong _top;
    private CacheLinePaddedLong _bottom;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            long bottom = Volatile.Read(ref _bottom.Value);
            long top = Volatile.Read(ref _top.Value);
            long count = bottom - top;
            if (count <= 0) return 0;
            return count > int.MaxValue ? int.MaxValue : (int)count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushBottom(AsyncJobRoutine job)
    {
        long bottom = _bottom.Value;
        long top = Volatile.Read(ref _top.Value);
        AsyncJobRoutine?[] buffer = Volatile.Read(ref _buffer);

        if (bottom - top >= buffer.Length - 1)
        {
            buffer = Grow(buffer, top, bottom);
        }

        buffer[(int)(bottom & (buffer.Length - 1))] = job;
        Volatile.Write(ref _bottom.Value, bottom + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushTop(AsyncJobRoutine job)
    {
        while (true)
        {
            long top = Volatile.Read(ref _top.Value);
            long bottom = Volatile.Read(ref _bottom.Value);
            AsyncJobRoutine?[] buffer = Volatile.Read(ref _buffer);

            if (bottom - top >= buffer.Length - 1)
                buffer = Grow(buffer, top, bottom);

            long newTop = top - 1;
            int index = (int)(newTop & (buffer.Length - 1));
            buffer[index] = job;

            if (Interlocked.CompareExchange(ref _top.Value, newTop, top) == top)
                return;

            buffer[index] = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPopBottom([NotNullWhen(true)] out AsyncJobRoutine? job)
    {
        long bottom = _bottom.Value - 1;
        Volatile.Write(ref _bottom.Value, bottom);
        Thread.MemoryBarrier();

        long top = Volatile.Read(ref _top.Value);
        if (top <= bottom)
        {
            AsyncJobRoutine?[] buffer = Volatile.Read(ref _buffer);
            job = buffer[(int)(bottom & (buffer.Length - 1))];

            if (top == bottom)
            {
                if (Interlocked.CompareExchange(ref _top.Value, top + 1, top) != top)
                    job = null;

                Volatile.Write(ref _bottom.Value, bottom + 1);
            }

            return job != null;
        }

        Volatile.Write(ref _bottom.Value, bottom + 1);
        job = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySteal([NotNullWhen(true)] out AsyncJobRoutine? job)
    {
        long top = Volatile.Read(ref _top.Value);
        Thread.MemoryBarrier();
        long bottom = Volatile.Read(ref _bottom.Value);

        if (top >= bottom)
        {
            job = null;
            return false;
        }

        AsyncJobRoutine?[] buffer = Volatile.Read(ref _buffer);
        job = buffer[(int)(top & (buffer.Length - 1))];
        if (job == null) return false;

        if (Interlocked.CompareExchange(ref _top.Value, top + 1, top) == top)
            return true;

        job = null;
        return false;
    }

    private AsyncJobRoutine?[] Grow(AsyncJobRoutine?[] oldBuffer, long top, long bottom)
    {
        AsyncJobRoutine?[] newBuffer = new AsyncJobRoutine?[oldBuffer.Length * 2];
        for (long i = top; i < bottom; i++)
        {
            newBuffer[(int)(i & (newBuffer.Length - 1))] = oldBuffer[(int)(i & (oldBuffer.Length - 1))];
        }

        Volatile.Write(ref _buffer, newBuffer);
        return newBuffer;
    }
}
