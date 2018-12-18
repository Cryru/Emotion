// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Game.Layering;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Layers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with layering.
    /// </summary>
    [TestClass]
    public class Layering
    {
        /// <summary>
        /// Test whether loading and unloading of layers in addition to the update and draw hooks work.
        /// </summary>
        [TestMethod]
        public void LayerLoading()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Add layer.
            LayerLoading testLayer = new LayerLoading();
            Task loadingTask = Context.LayerManager.Add(testLayer, "test loading", 0);
            // Wait for loading to complete.
            while (!loadingTask.IsCompleted) host.RunCycle();

            // Run a cycle to ensure update and draw are called.
            host.RunCycle();

            // Get the layer, and ensure it is loaded. Name should be case insensitive.
            Assert.AreEqual(testLayer, Context.LayerManager.Get("tEsT LoAdInG"));
            Assert.AreEqual(1, Context.LayerManager.LoadedLayers.Length);
            Assert.IsTrue(testLayer.Active);

            // Remove layer.
            Task removingTask = Context.LayerManager.Remove(testLayer);
            // Wait for unloading to complete.
            while (!removingTask.IsCompleted) host.RunCycle();

            // Assert everything was called correctly.
            Assert.IsTrue(testLayer.LoadCalled);
            Assert.IsTrue(testLayer.UpdateCalled);
            Assert.IsFalse(testLayer.LightUpdateCalled);
            Assert.IsTrue(testLayer.DrawCalled);
            Assert.IsTrue(testLayer.UnloadCalled);

            // --- Negative ---

            // Load null. This shouldn't throw any errors.
            loadingTask = Context.LayerManager.Add(null, "no layer here", 0);
            host.RunCycle();
            Assert.IsTrue(loadingTask.IsCompleted);

            // Unload non-existing. This shouldn't throw any errors.
            removingTask = Context.LayerManager.Remove((Layer) null);
            host.RunCycle();
            Assert.IsTrue(removingTask.IsCompleted);

            removingTask = Context.LayerManager.Remove("non existing");
            host.RunCycle();
            Assert.IsTrue(removingTask.IsCompleted);

            // Ensure no layers left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);
        }

        /// <summary>
        /// Test whether light update is called when the host is not focused.
        /// </summary>
        [TestMethod]
        public void LayerLightUpdate()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Set focused to false.
            host.Focused = false;

            // Add layer.
            LayerLoading testLayer = new LayerLoading();
            Task loadingTask = Context.LayerManager.Add(testLayer, "test light update", 0);
            // Wait for loading to complete. It should run even when unfocused.
            while (!loadingTask.IsCompleted) host.RunCycle();

            // Run a cycle.
            host.RunCycle();

            // Remove layer.
            Task removingTask = Context.LayerManager.Remove(testLayer);
            // Wait for unloading to complete.
            while (!removingTask.IsCompleted) host.RunCycle();

            // Assert everything was called correctly.
            Assert.IsTrue(testLayer.LoadCalled);
            Assert.IsFalse(testLayer.UpdateCalled);
            Assert.IsTrue(testLayer.LightUpdateCalled);
            Assert.IsFalse(testLayer.DrawCalled);
            Assert.IsTrue(testLayer.UnloadCalled);

            // Restore focus.
            host.Focused = true;

            // Ensure no layers are left loaded.
            Assert.AreEqual(0, Context.LayerManager.LoadedLayers.Length);
        }
    }
}