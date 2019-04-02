#region Using

using System.Collections;
using System.Threading.Tasks;
using Adfectus.Game.Time.Routines;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected to coroutines.
    /// </summary>
    [Collection("main")]
    public class Coroutines
    {
        /// <summary>
        /// Test coroutine time waiting.
        /// </summary>
        [Fact]
        public void CoroutineTimeWait()
        {
            CoroutineManager manager = new CoroutineManager();

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                ExtUpdate = () => { manager.Update(); }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutine());

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            Task.Delay(4000).Wait();

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);

            // Cleanup.
            Helpers.UnloadScene();
        }

        /// <summary>
        /// Test coroutine time waiting, in more than one increment.
        /// </summary>
        [Fact]
        public void CoroutineTimeWaitTwoIncrements()
        {
            CoroutineManager manager = new CoroutineManager();

            // Create scene for this test.
            TestScene extScene = new TestScene
            {
                ExtUpdate = () => { manager.Update(); }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            RoutineSwitch switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutineTwoIncrements(switchFlag));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            Task.Delay(4000).Wait();

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(switchFlag.Switch);

            // Cleanup.
            Helpers.UnloadScene();
        }

        /// <summary>
        /// Test coroutine loop waits.
        /// </summary>
        [Fact]
        public void CoroutineLoopWait()
        {
            CoroutineManager manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            RoutineSwitch switchFlag = new RoutineSwitch();
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
        [Fact]
        public void CoroutineWaitSubroutine()
        {
            CoroutineManager manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            RoutineSwitch switchFlag = new RoutineSwitch();
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
        [Fact]
        public void CoroutineStop()
        {
            CoroutineManager manager = new CoroutineManager();

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            RoutineSwitch switchFlag = new RoutineSwitch();
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

        public static IEnumerator TimeTestRoutine()
        {
            yield return new WaitForSeconds(2);
        }

        public static IEnumerator TimeTestRoutineTwoIncrements(RoutineSwitch flag)
        {
            yield return new WaitForSeconds(1);
            flag.Switch = true;
            yield return new WaitForSeconds(1);
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
    }
}