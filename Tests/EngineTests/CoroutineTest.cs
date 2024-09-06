#region Using

using System.Collections;
using System.Collections.Generic;
using Emotion.Game.Time.Routines;
using Emotion.Testing;

// ReSharper disable AccessToModifiedClosure

#endregion

namespace Tests.EngineTests;

[Test]
//[TestClassRunParallel]
public class CoroutineTest
{
    private class CoroutineTestStateTracker
    {
        public int Step;
    }

    [Test]
    public void CoroutineBasicExecutionTest()
    {
        static IEnumerator RoutineFunc(CoroutineTestStateTracker state)
        {
            state.Step = 1;
            yield return null;
            state.Step = 2;
            yield return null;
            state.Step = 3;
            yield return null;
            state.Step = 4;
        }

        var state = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();

        var coroutine = manager.StartCoroutine(RoutineFunc(state));
        Assert.Equal(state.Step, 1);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 2);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 3);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 4);
        Assert.True(coroutine.Finished);
    }

    [Test]
    public void CoroutineStartingRoutineFromInside()
    {
        static IEnumerator RoutineFuncInner(CoroutineTestStateTracker state)
        {
            state.Step = 1;
            yield return null;
            state.Step = 2;
            yield return null;
            state.Step = 3;
            yield return null;
            state.Step = 4;
        }

        static IEnumerator RoutineFunc(CoroutineTestStateTracker state, CoroutineManager manager)
        {
            state.Step = 1;
            yield return null;

            var stateInner = new CoroutineTestStateTracker();
            manager.StartCoroutine(RoutineFuncInner(stateInner));
            Assert.Equal(stateInner.Step, 1);

            state.Step = 2;
            yield return null;

            Assert.Equal(stateInner.Step, 1); // Since this routine should update before the one started after it.
            state.Step = 3;
            yield return null;

            Assert.Equal(stateInner.Step, 2);
            state.Step = 4;
        }

        var state = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();

        var coroutine = manager.StartCoroutine(RoutineFunc(state, manager));
        Assert.Equal(state.Step, 1);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 2);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 3);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 4);
        Assert.True(coroutine.Finished);
    }

    [Test]
    public void CoroutineSubRoutine()
    {
        static IEnumerator RoutineFuncInner(CoroutineTestStateTracker state)
        {
            state.Step = 1;
            yield return null;
            state.Step = 2;
            yield return null;
            state.Step = 3;
            yield return null;
            state.Step = 4;
        }

        static IEnumerator RoutineFunc(CoroutineTestStateTracker state, CoroutineTestStateTracker stateInner, CoroutineManager manager)
        {
            state.Step = 1;
            yield return null;

            yield return RoutineFuncInner(stateInner);
            Assert.Equal(stateInner.Step, 4);

            state.Step = 2;
            yield return null;
            state.Step = 3;
            yield return null;
            state.Step = 4;
        }

        var state = new CoroutineTestStateTracker();
        var stateInner = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();

        Coroutine coroutine = manager.StartCoroutine(RoutineFunc(state, stateInner, manager));
        Assert.Equal(state.Step, 1);
        Assert.Equal(manager.Count, 1);
        Assert.False(coroutine.Finished);

        // Subroutine
        manager.Update(); // 0->1 (this adds the subroutine)
        Assert.Equal(manager.Count, 2);
        Assert.Equal(stateInner.Step, 1);
        manager.Update(); // 1->2
        Assert.Equal(stateInner.Step, 2);
        manager.Update(); // 2->3
        Assert.Equal(stateInner.Step, 3);
        manager.Update(); // 3->4+Finished
        Assert.Equal(stateInner.Step, 4);

        manager.Update();
        Assert.Equal(state.Step, 2);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 3);
        Assert.False(coroutine.Finished);

        manager.Update();
        Assert.Equal(state.Step, 4);
        Assert.True(coroutine.Finished);
    }

    [Test]
    public void CoroutineStop()
    {
        static IEnumerator YieldTwiceRoutine(CoroutineTestStateTracker state)
        {
            state.Step = 1;
            yield return null;
            state.Step = 2;
            yield return null;
            state.Step = 3;
        }

        var testState = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();

        // No routines should be running.
        Assert.Equal(0, manager.Count);

        Coroutine routineHandle = manager.StartCoroutine(YieldTwiceRoutine(testState));

        // One routine should be running.
        Assert.Equal(1, manager.Count);
        Assert.False(routineHandle.Finished);
        Assert.Equal(testState.Step, 1);

        // First yield null
        manager.Update();
        Assert.Equal(1, manager.Count);
        Assert.False(routineHandle.Finished);
        Assert.Equal(testState.Step, 2);

        routineHandle.RequestStop();
        manager.Update(); // The routine will be removed on the next manager update.

        // Check routine stopped
        Assert.Equal(0, manager.Count);
        Assert.True(routineHandle.Finished);
        Assert.Equal(testState.Step, 2); // Step should still be 2, since the routine was stopped beforehand
    }

    [Test]
    public void CoroutinePreciseTimeWait()
    {
        static IEnumerator CoroutinePreciseTimewait(float timeToWait)
        {
            yield return timeToWait;
        }

        var manager = new CoroutineManager();
        Coroutine routineHandle = manager.StartCoroutine(CoroutinePreciseTimewait(10f));

        // One routine should be running.
        Assert.Equal(1, manager.Count);
        Assert.False(routineHandle.Finished);

        manager.Update(0); // Zero time, nothing changed.

        // One routine should be running.
        Assert.Equal(1, manager.Count);
        Assert.False(routineHandle.Finished);

        // Pass 10 time in two increments.
        manager.Update(5);
        Assert.Equal(1, manager.Count);
        Assert.False(routineHandle.Finished);
        manager.Update(5);

        Assert.Equal(0, manager.Count);
        Assert.True(routineHandle.Finished);
    }

    [Test]
    public void CoroutinePreciseTimeWaitComplex()
    {
        static IEnumerator CoroutinePreciseTimewaitWithMagicNumberFlag(CoroutineTestStateTracker state, int routineIndex, float timeToWait)
        {
            yield return timeToWait;
            if (state.Step == 0)
                state.Step = routineIndex;
        }

        var testState = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();
        Coroutine routineHandle1 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 1, 5f));
        Coroutine routineHandle2 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 2, 2f));
        Coroutine routineHandle3 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 3, 10f));

        Assert.Equal(3, manager.Count);
        Assert.False(routineHandle1.Finished);
        Assert.False(routineHandle2.Finished);
        Assert.False(routineHandle3.Finished);

        manager.Update(8); // Pass time increment that would complete two routines at once.

        // Routine2 should've completed first.
        Assert.Equal(testState.Step, 2f);

        // One routine should be running.
        Assert.Equal(1, manager.Count);
        Assert.True(routineHandle1.Finished);
        Assert.True(routineHandle2.Finished);

        // Pass rest of time
        manager.Update(3);
        Assert.Equal(0, manager.Count);
        Assert.True(routineHandle3.Finished);
    }

    [Test]
    public void CoroutineTimeWaitRealTimeIncrement()
    {
        static IEnumerator CoroutinePreciseTimewaitWithMagicNumberFlag(CoroutineTestStateTracker state, int routineIndex, float timeToWait)
        {
            yield return timeToWait;
            if (state.Step == 0)
                state.Step = routineIndex;
        }

        var testState = new CoroutineTestStateTracker();
        var manager = new CoroutineManager();
        Coroutine routineHandle1 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 1, 500f));
        Coroutine routineHandle2 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 2, 200f));
        Coroutine routineHandle3 = manager.StartCoroutine(CoroutinePreciseTimewaitWithMagicNumberFlag(testState, 3, 1000f));

        float timePassed = 0;
        while (timePassed < 500)
        {
            float incr = 16.666f;
            manager.Update(incr);
            timePassed += incr;
        }

        Assert.Equal(1, manager.Count);
        Assert.True(routineHandle1.Finished);
        Assert.Equal(routineHandle1.CurrentWaiter_Time, 0);
        Assert.True(routineHandle2.Finished);
        Assert.Equal(routineHandle2.CurrentWaiter_Time, 0);
        Assert.False(routineHandle3.Finished);
        Assert.Equal(routineHandle3.CurrentWaiter_Time, 483.353516f);
    }
}