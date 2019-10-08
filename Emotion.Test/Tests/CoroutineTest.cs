#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.Scripting;
// ReSharper disable AccessToModifiedClosure

#endregion

namespace Emotion.Test.Tests
{
    [Test("Coroutine", true)]
    public class CoroutineTest
    {
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

        /// <summary>
        /// Test coroutine time waiting.
        /// </summary>
        [Test]
        public void CoroutineTimeWait()
        {
            var manager = new CoroutineManager();

            var runLoop = true;
            Task.Run(() =>
            {
                while (runLoop) 
                    manager.Update();
            });

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutine());

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            Task.Delay(4000).Wait();
            runLoop = false;

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
        }

        /// <summary>
        /// Test coroutine time waiting, in more than one increment.
        /// </summary>
        [Test]
        public void CoroutineTimeWaitTwoIncrements()
        {
            var manager = new CoroutineManager();

            var runLoop = true;
            Task.Run(() =>
            {
                while (runLoop) manager.Update();
            });

            // No routines should be running.
            Assert.Equal(0, manager.Count);

            // Start a time waiting routine.
            var switchFlag = new RoutineSwitch();
            Coroutine routineHandle = manager.StartCoroutine(TimeTestRoutineTwoIncrements(switchFlag));

            // One routine should be running.
            Assert.Equal(1, manager.Count);
            Assert.False(routineHandle.Finished);

            // Wait for time.
            Task.Delay(4000).Wait();
            runLoop = false;

            // The routine should've finished.
            Assert.Equal(0, manager.Count);
            Assert.True(routineHandle.Finished);
            Assert.True(switchFlag.Switch);
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

        [Test]
        public void TestScriptArgs()
        {
            var script = Engine.AssetLoader.Get<CSharpScriptAsset>("Scripts/CustomMainArgsScript.cs");
            const string objectPass = "yo";
            object result = CSharpScriptEngine.RunScript(script, objectPass).Result;
            Assert.True((int) result == objectPass.GetHashCode());
            Engine.AssetLoader.Destroy("Scripts/CustomMainArgsScript.cs");
        }
    }
}