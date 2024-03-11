#region Using

using System.Collections;
using Emotion.Game.Time.Routines;
using Emotion.Testing;

// ReSharper disable AccessToModifiedClosure

#endregion

namespace Tests.EngineTests
{
    [Test]
    [TestClassRunParallel]
    public class CoroutineTest
    {
        /// <summary>
        /// Test coroutine time waiting.
        /// </summary>
        [Test]
        public void CoroutineMagicNumberWait()
        {
            var testState = new CoroutineTestingState();

            var manager = new CoroutineManager();
            manager.Update();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a routine that will wait for the magic number to be larger than 2
            Coroutine routineHandle = manager.StartCoroutine(MagicNumberWaitRoutine(testState, 2));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            manager.Update();

            // One routine should still be running, as magic number hasn't changed.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Advance magic number and update again.
            testState.MagicNumber = 4;
            manager.Update();

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
        }

        /// <summary>
        /// Test coroutine time waiting, in more than one increment.
        /// </summary>
        [Test]
        public void CoroutineMagicNumberWaitTwoIncrements()
        {
            var testState = new CoroutineTestingState();

            var manager = new CoroutineManager();
            manager.Update();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            Coroutine routineHandle = manager.StartCoroutine(MagicNumberWaitRoutineTwoIncrements(testState, 1f));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            manager.Update(); // No change, since magic number hasn't changed.
            testState.MagicNumber = 4;
            manager.Update(); // This will get past the first wait, set the flag, and get past the second wait.

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(testState.FlagSetByRoutine);
        }

        /// <summary>
        /// Test coroutine loop waits.
        /// </summary>
        [Test]
        public void CoroutineNullYieldTest()
        {
            var testState = new CoroutineTestingState();

            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a routine that will yield twice before setting the flag.
            Coroutine routineHandle = manager.StartCoroutine(YieldTwiceRoutine(testState));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // First yield null
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            // Second yield null
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            // Finishing update, no yield so the routine is done.
            manager.Update();

            // The routine should now be finished
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(testState.FlagSetByRoutine);
        }

        /// <summary>
        /// Test waiting of subroutine.
        /// </summary>
        [Test]
        public void CoroutineWaitSubroutine()
        {
            var testState = new CoroutineTestingState();

            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            Coroutine routineHandle = manager.StartCoroutine(SubroutineTestRoutine(testState));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // This is when the second routine will be created.
            // However the number of running routines will remain the same as it is a subroutine.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            // First yield null in the subroutine
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            // Second yield null in the subroutine.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            // The finishing update will run the parent routine as well
            manager.Update();

            // The subroutine will have finished now.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(testState.FlagSetByRoutine);
        }

        /// <summary>
        /// Test stopping of a coroutine.
        /// </summary>
        [Test]
        public void CoroutineStop()
        {
            var testState = new CoroutineTestingState();

            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            Coroutine routineHandle = manager.StartCoroutine(YieldTwiceRoutine(testState));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // First yield null
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine);

            routineHandle.Stop();
            manager.Update(); // The routine will be removed on the next manager update.

            // Check routine stopped
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.False(testState.FlagSetByRoutine); // flag shouldn't be set since the routine was stopped beforehand
        }

        [Test]
        public void CoroutinePreciseTimeWait()
        {
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
            var testState = new CoroutineTestingState();

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
            Assert.Equal(testState.MagicNumber, 2f);

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
            var testState = new CoroutineTestingState();

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
            Assert.Equal(routineHandle1.WaitingForTime, 0);
            Assert.True(routineHandle2.Finished);
            Assert.Equal(routineHandle2.WaitingForTime, 0);
            Assert.False(routineHandle3.Finished);
            Assert.Equal(routineHandle3.WaitingForTime, 483.353516f);
        }

        private class CoroutineTestingState
        {
            public int MagicNumber;
            public bool FlagSetByRoutine;
        }

        private static IEnumerator MagicNumberWaitRoutine(CoroutineTestingState state, float magicNumberGoal)
        {
            while (state.MagicNumber < magicNumberGoal)
                yield return null;
        }

        private static IEnumerator MagicNumberWaitRoutineTwoIncrements(CoroutineTestingState state, float magicNumberGoal)
        {
            yield return null;
            while (state.MagicNumber < magicNumberGoal)
                yield return null;
            state.FlagSetByRoutine = true;
            while (state.MagicNumber < magicNumberGoal)
                yield return null;
        }

        private static IEnumerator YieldTwiceRoutine(CoroutineTestingState state)
        {
            yield return null;
            yield return null;
            state.FlagSetByRoutine = true;
        }

        private static IEnumerator SubroutineTestRoutine(CoroutineTestingState state)
        {
            yield return YieldTwiceRoutine(state);
        }

        private static IEnumerator CoroutinePreciseTimewait(float timeToWait)
        {
            yield return timeToWait;
        }

        private static IEnumerator CoroutinePreciseTimewaitWithMagicNumberFlag(CoroutineTestingState state, int flagNumber, float timeToWait)
        {
            yield return timeToWait;
            if (state.MagicNumber == 0)
                state.MagicNumber = flagNumber;
        }
    }
}