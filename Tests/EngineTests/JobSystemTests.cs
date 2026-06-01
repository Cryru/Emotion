#nullable enable

using Emotion.Core;
using Emotion.Core.Systems.JobSystem;
using Emotion.Core.Utility.Coroutines;
using Emotion.Testing;
using System;
using System.Collections;
using System.Threading;

namespace Tests.EngineTests;

[Test]
public class JobSystemTests
{
    private const int WaitTimeoutMs = 30_000;

    [Test]
    public IEnumerator SimpleJobsComplete()
    {
        int jobCount = Math.Max(Engine.Jobs.ThreadCount * 8, 32);

        int completed = 0;
        for (int i = 0; i < jobCount; i++)
        {
            Engine.Jobs.Add(new ActionJob(() =>
            {
                Interlocked.Increment(ref completed);
            }));
        }

        yield return 100;
        Assert.Equal(jobCount, Volatile.Read(ref completed));
    }

    [Test]
    public IEnumerator JobTagCounterDrainsAfterJobsFinish()
    {
        int jobCount = Math.Max(Engine.Jobs.ThreadCount * 2, 4);
        string tag = $"{nameof(JobSystemTests)}.{nameof(JobTagCounterDrainsAfterJobsFinish)}.{Guid.NewGuid():N}";

        using ManualResetEventSlim gate = new(false);

        int completed = 0;
        for (int i = 0; i < jobCount; i++)
        {
            Engine.Jobs.Add(new ActionJob(() =>
            {
                gate.Wait();
                Interlocked.Increment(ref completed);
            }), false, tag);
        }

        Assert.False(Engine.Jobs.NotManyJobsWithTag(tag));
        yield return 100;
        Assert.Equal(completed, 0);

        gate.Set();
        yield return 100;
        Assert.Equal(jobCount, Volatile.Read(ref completed));
        Assert.True(Engine.Jobs.NotManyJobsWithTag(tag));
    }

    [Test]
    public IEnumerator PriorityJobsRunWhileNormalJobsAreBlocked()
    {
        int normalJobCount = Math.Max(Engine.Jobs.ThreadCount * 2, 4);
        int priorityJobCount = Math.Max(Engine.Jobs.ThreadCount * 4, 16);

        using ManualResetEventSlim normalGate = new(false);
        using CountdownEvent normalDone = new(normalJobCount);
        using CountdownEvent priorityDone = new(priorityJobCount);

        int normalStarted = 0;
        for (int i = 0; i < normalJobCount; i++)
        {
            Engine.Jobs.Add(new ActionJob(() =>
            {
                Interlocked.Increment(ref normalStarted);
                normalGate.Wait();
                normalDone.Signal();
            }));
        }

        yield return WaitUntil(() => Volatile.Read(ref normalStarted) > 0);

        for (int i = 0; i < priorityJobCount; i++)
        {
            Engine.Jobs.Add(new ActionJob(() =>
            {
                priorityDone.Signal();
            }), true);
        }

        Assert.True(priorityDone.Wait(WaitTimeoutMs));
        normalGate.Set();

        Assert.True(normalDone.Wait(WaitTimeoutMs));
    }

    [Test]
    public IEnumerator PriorityCoroutineIsRescheduledUntilFinished()
    {
        int steps = Math.Max(Engine.Jobs.ThreadCount * 4, 16);
        int observedSteps = 0;

        IRoutineWaiter waiter = Engine.Jobs.Add(MultiStepRoutine(steps, () =>
        {
            Interlocked.Increment(ref observedSteps);
        }), true);

        yield return WaitUntil(() => waiter.Finished);
        Assert.Equal(steps, Volatile.Read(ref observedSteps));
    }

    [Test]
    public IEnumerator YieldedCoroutineContinuationStaysOnWorker()
    {
        using ManualResetEventSlim gate = new(false);
        ManualResetWaiter gateWaiter = new(gate);
        int firstThread = 0;
        int threadChanges = 0;

        IRoutineWaiter waiter = Engine.Jobs.Add(WorkerAffinityRoutine(gateWaiter, () =>
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            int observedFirstThread = Volatile.Read(ref firstThread);
            if (observedFirstThread == 0)
            {
                Interlocked.CompareExchange(ref firstThread, threadId, 0);
                return;
            }

            if (threadId != observedFirstThread)
                Interlocked.Increment(ref threadChanges);
        }));

        yield return WaitUntil(() => Volatile.Read(ref firstThread) != 0);

        int jobCount = Math.Max(Engine.Jobs.ThreadCount * 16, 32);
        using CountdownEvent done = new(jobCount);
        for (int i = 0; i < jobCount; i++)
        {
            Engine.Jobs.Add(new ActionJob(() =>
            {
                Thread.Yield();
                done.Signal();
            }));
        }

        Assert.True(done.Wait(WaitTimeoutMs));
        gate.Set();

        yield return WaitUntil(() => waiter.Finished);
        Assert.Equal(0, Volatile.Read(ref threadChanges));
    }

    [Test]
    public IEnumerator CoroutineJobsCanWaitOnJobsQueuedBehindThem()
    {
        int jobCount = Math.Max(Engine.Jobs.ThreadCount * 8, 16);
        var holder = new NumberHolder();

        IRoutineWaiter[] waiters = new IRoutineWaiter[jobCount];
        for (int i = 0; i < jobCount; i++)
        {
            waiters[i] = Engine.Jobs.Add(AddNumberRoutineAsync(holder));
        }

        yield return new CombineWaitMany(waiters);
        Assert.Equal(jobCount, holder.Number);
    }

    #region Helpers

    private static IEnumerator WorkerAffinityRoutine(IRoutineWaiter gate, Action observeThread)
    {
        observeThread();
        yield return gate;

        for (int i = 0; i < 8; i++)
        {
            observeThread();
            yield return null;
        }
    }

    private static IEnumerator MultiStepRoutine(int steps, Action onStep)
    {
        for (int i = 0; i < steps; i++)
        {
            onStep();
            yield return null;
        }
    }

    private static IEnumerator AddNumberRoutineAsync(NumberHolder holder)
    {
        holder.Number++;
        yield break;
    }

    private static IEnumerator WaitUntil(Func<bool> condition)
    {
        int spins = 0;
        while (spins < 1000)
        {
            if (!condition())
                yield return null;
            spins++;
        }

        Assert.True(condition());
    }

    private sealed class ActionJob : ISimpleAsyncJob
    {
        private readonly Action _action;

        public ActionJob(Action action)
        {
            _action = action;
        }

        public void Run()
        {
            _action();
        }
    }

    private class NumberHolder
    {
        public int Number;
    }

    private sealed class ManualResetWaiter : IRoutineWaiter
    {
        private readonly ManualResetEventSlim _gate;

        public bool Finished => _gate.IsSet;

        public ManualResetWaiter(ManualResetEventSlim gate)
        {
            _gate = gate;
        }

        public void Update()
        {
        }
    }

    #endregion
}
