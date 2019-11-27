#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Scripting;
using Emotion.Test;

// ReSharper disable AccessToModifiedClosure

#endregion

namespace Tests.Classes
{
    [Test("Coroutine", true)]
    public class CoroutineTest
    {
        private static int _time;

        private class WaitForTestTime : IRoutineWaiter
        {
            private int _waitTime;
            public WaitForTestTime(int time)
            {
                _waitTime = time;
            }

            public bool Finished { get => _time > _waitTime; }
            public void Update()
            {
                
            }
        }
        public static void ProgressTime(int seconds)
        {

        }

        public static IEnumerator TimeTestRoutine()
        {
            yield return new WaitForTestTime(2);
        }

        public static IEnumerator TimeTestRoutineTwoIncrements(RoutineSwitch flag)
        {
            yield return null;
            yield return new WaitForTestTime(1);
            flag.Switch = true;
            yield return new WaitForTestTime(1);
        }

        public static IEnumerator LoopTestRoutine(RoutineSwitch flag)
        {
            yield return null;
            yield return null;
            flag.Switch = true;
        }

        public static IEnumerator SubroutineTestRoutine(RoutineSwitch flag, CoroutineManager manager)
        {
            yield return manager.StartCoroutine(LoopTestRoutine(flag));
        }

        /// <summary>
        /// Mini class used for testing whether the routine has run.
        /// </summary>
        public class RoutineSwitch
        {
            public bool Switch;
        }

        /// <summary>
        /// Test coroutine time waiting.
        /// </summary>
        [Test]
        public void CoroutineTimeWait()
        {
            var manager = new CoroutineManager();
            manager.Update();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutine());

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            manager.Update();
            _time = 4;
            manager.Update();

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);

            _time = 0;
        }

        /// <summary>
        /// Test coroutine time waiting, in more than one increment.
        /// </summary>
        [Test]
        public void CoroutineTimeWaitTwoIncrements()
        {
            var manager = new CoroutineManager();
            manager.Update();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            var switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutineTwoIncrements(switchFlag));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            manager.Update();
            _time = 4;
            manager.Update();
            manager.Update();
            manager.Update();

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(switchFlag.Switch);

            _time = 0;
        }

        /// <summary>
        /// Test coroutine loop waits.
        /// </summary>
        [Test]
        public void CoroutineLoopWait()
        {
            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            var switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(LoopTestRoutine(switchFlag));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // First loop.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(switchFlag.Switch);

            // Second loop.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(switchFlag.Switch);

            // The routine should've finished.
            manager.Update();
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(switchFlag.Switch);
        }

        /// <summary>
        /// Test waiting of subroutine.
        /// </summary>
        [Test]
        public void CoroutineWaitSubroutine()
        {
            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            var switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(SubroutineTestRoutine(switchFlag, manager));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // First loop. This is when the second routine will be created.
            manager.Update();
            Assert.Equal(2, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(switchFlag.Switch);

            // Second loop.
            manager.Update();
            Assert.Equal(2, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(switchFlag.Switch);

            // The subroutine will have finished now, but since it was started after the first it is updated after it.
            // This means that the first routine will find out about it's child's status one loop after.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.True(switchFlag.Switch);

            // Now the parent routine will finish as well.
            manager.Update();
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(switchFlag.Switch);
        }

        /// <summary>
        /// Test stopping of a coroutine.
        /// </summary>
        [Test]
        public void CoroutineStop()
        {
            var manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            var switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(LoopTestRoutine(switchFlag));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // First loop.
            manager.Update();
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);
            Assert.False(switchFlag.Switch);

            routineHandle.Stop();

            // Second loop - the routine should be stopped.
            manager.Update();
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.False(switchFlag.Switch);
        }
    }
}