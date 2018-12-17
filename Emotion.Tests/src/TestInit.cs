// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Tests.Interop;
using Emotion.Tests.Layers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests
{
    [TestClass]
    public class TestInit
    {
        private static TestHost _testingHost = new TestHost();

        [ClassInitialize]
        public static void StartTest(TestContext _)
        {
            Context.Host = _testingHost;
            Context.Setup();
            Context.Run();
        }

        [ClassCleanup]
        public static void TestEnd()
        {
            Context.Quit();
        }

        /// <summary>
        /// Test whether the testing harness works.
        /// </summary>
        [TestMethod]
        public void TestHarnessTest()
        {
            _testingHost.RunCycle();
            Bitmap bitmap = _testingHost.TakeScreenshot();
            Assert.AreEqual(new System.Numerics.Vector2(bitmap.Size.Width, bitmap.Size.Height), _testingHost.Size);
        }

        /// <summary>
        /// Test whether loading and unloading of layers in addition to the update and draw hooks work.
        /// </summary>
        [TestMethod]
        public void TestLayerLoading()
        {
            // Add layer.
            LayerLoadingTest testLayer = new LayerLoadingTest();
            Task loadingTask = Context.LayerManager.Add(testLayer, "test", 0);
            // Wait for loading to complete.
            while (!loadingTask.IsCompleted)
            {
                _testingHost.RunCycle();
            }

            // Remove layer.
            Task removingTask = Context.LayerManager.Remove(testLayer);
            // Wait for unloading to complete.
            while (!removingTask.IsCompleted)
            {
                _testingHost.RunCycle();
            }

            // Assert everything was called correctly.
            Assert.IsTrue(testLayer.LoadCalled);
            Assert.IsTrue(testLayer.UpdateCalled);
            Assert.IsTrue(testLayer.DrawCalled);
            Assert.IsTrue(testLayer.UnloadCalled);

            // --- Negative

            // Load null. This shouldn't throw any errors.
            loadingTask = Context.LayerManager.Add(null, "no layer here", 0);
            _testingHost.RunCycle();
            Assert.IsTrue(loadingTask.IsCompleted);

            // Unload non-existing. This shouldn't throw any errors.
            removingTask = Context.LayerManager.Remove((Layer) null);
            _testingHost.RunCycle();
            Assert.IsTrue(removingTask.IsCompleted);

            removingTask = Context.LayerManager.Remove("non existing");
            _testingHost.RunCycle();
            Assert.IsTrue(removingTask.IsCompleted);
        }
    }
}