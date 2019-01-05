// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Scenes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with scenography.
    /// </summary>
    [TestClass]
    public class Scenography
    {
        /// <summary>
        /// Test whether loading and unloading of scenes in addition to the update and draw hooks work.
        /// </summary>
        [TestMethod]
        public void SceneLoading()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Add scene.
            SceneLoading testScene = new SceneLoading();
            Task loadingTask = Context.SceneManager.SetScene(testScene);
            // Wait for loading to complete.
            while (!loadingTask.IsCompleted) host.RunCycle();

            // Run a cycle to ensure update and draw are called.
            host.RunCycle();

            // The current scene should be switched.
            Assert.AreEqual(testScene, Context.SceneManager.Current);

            // Set loading screen.
            SceneLoading testLoadingScreen = new SceneLoading();
            Context.SceneManager.SetLoadingScreen(testLoadingScreen);

            // The loading screen should've been switched.
            Assert.AreEqual(testLoadingScreen, Context.SceneManager.LoadingScreen);

            // Create a new test scene.
            ExternalScene newTestScene = new ExternalScene();
            newTestScene.ExtLoad = () => { Task.Delay(500).Wait(); };
            loadingTask = Context.SceneManager.SetScene(newTestScene);
            // Wait for loading to complete.
            while (!loadingTask.IsCompleted) host.RunCycle();

            // The scene should be switched.
            Assert.AreEqual(newTestScene, Context.SceneManager.Current);

            // The loading screen should've been called while it was loading.
            // Loading screens load on the main layer.
            Assert.IsTrue(testLoadingScreen.SyncLoadCalled);
            Assert.IsTrue(testLoadingScreen.UpdateCalled);
            Assert.IsTrue(testLoadingScreen.DrawCalled);

            // Assert everything was called correctly on the first scene.
            Assert.IsTrue(testScene.LoadCalled);
            Assert.IsTrue(testScene.UpdateCalled);
            Assert.AreEqual(0, testScene.FocusLossCalled);
            Assert.IsTrue(testScene.DrawCalled);
            Assert.IsTrue(testScene.UnloadCalled);

            // --- Negative ---

            // Load null. This shouldn't throw any errors.
            loadingTask = Context.SceneManager.SetScene(null);
            host.RunCycle();
            Assert.IsTrue(loadingTask.IsCompleted);

            // Cleanup.
            Helpers.UnloadScene();
        }

        /// <summary>
        /// Test whether the focus loss event is triggered when the host is unfocused.
        /// </summary>
        [TestMethod]
        public void LayerLightUpdate()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            // Set focused to false.
            host.Focused = false;

            // Load scene.
            SceneLoading testScene = new SceneLoading();
            Helpers.LoadScene(testScene, false);

            // Run a cycle.
            host.RunCycle();

            // Assert everything was called correctly.
            Assert.IsTrue(testScene.LoadCalled);
            Assert.IsFalse(testScene.UpdateCalled);
            Assert.AreEqual(1, testScene.FocusLossCalled);
            Assert.IsFalse(testScene.DrawCalled);

            // Restore focus.
            host.Focused = true;

            // Cleanup.
            Helpers.UnloadScene();

            Assert.IsTrue(testScene.UnloadCalled);
        }
    }
}