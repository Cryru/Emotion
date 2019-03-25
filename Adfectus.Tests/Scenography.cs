#region Using

using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Scenography;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with scenography.
    /// </summary>
    [Collection("main")]
    public class Scenography
    {
        /// <summary>
        /// Test whether loading and unloading of scenes in addition to the update and draw hooks work.
        /// </summary>
        [Fact]
        public void TestScene()
        {
            // Add scene.
            TestScene testScene = new TestScene();
            Task loadingTask = Engine.SceneManager.SetScene(testScene);
            // Wait for loading to complete.
            loadingTask.Wait();
            testScene.WaitFrames(1).Wait();

            // The current scene should be switched.
            Assert.Equal(testScene, Engine.SceneManager.Current);

            // Save the original engine default loading screen.
            Scene originalLoadingScreen = Engine.SceneManager.LoadingScreen;

            // Set loading screen to one which will be changed.
            TestScene oldLoadingScreen = new TestScene();
            Engine.SceneManager.SetLoadingScreen(oldLoadingScreen);

            // Set loading screen.
            TestScene testLoadingScreen = new TestScene();
            Engine.SceneManager.SetLoadingScreen(testLoadingScreen);

            // Check if the old loading screen was unloaded.
            Assert.True(oldLoadingScreen.UnloadCalled);

            // The loading screen should've been switched.
            Assert.Equal(testLoadingScreen, Engine.SceneManager.LoadingScreen);

            // Create a new test scene.
            TestScene newTestScene = new TestScene {ExtLoad = () => { Task.Delay(500).Wait(); }};
            loadingTask = Engine.SceneManager.SetScene(newTestScene);
            // Wait for loading to complete.
            loadingTask.Wait();
            newTestScene.WaitFrames(1).Wait();

            // The scene should be switched.
            Assert.Equal(newTestScene, Engine.SceneManager.Current);

            // The loading screen should've been run while the scene was being loaded
            Assert.True(testLoadingScreen.UpdateCalled);
            Assert.True(testLoadingScreen.DrawCalled);

            // Everything should've been called correctly on the scene.
            Assert.True(testScene.LoadCalled);
            Assert.True(testScene.UpdateCalled);
            Assert.Equal(0, testScene.FocusLossCalled);
            Assert.True(testScene.DrawCalled);
            Assert.True(testScene.UnloadCalled);

            // --- Negative ---

            // Load null. This shouldn't throw any errors.
            loadingTask = Engine.SceneManager.SetScene(null);
            // Wait for loading to complete.
            loadingTask.Wait();
            Assert.True(loadingTask.IsCompleted);

            // Cleanup.
            Helpers.UnloadScene();

            // Return the original loading screen.
            Engine.SceneManager.SetLoadingScreen(originalLoadingScreen);
        }

        /// <summary>
        /// Test whether the focus loss event is triggered when the host is unfocused.
        /// </summary>
        [Fact]
        public void LayerLightUpdate()
        {
            // Set focused to false.
            Engine.ForceUnfocus(true);

            Task.Delay(100).Wait();

            // Load scene.
            TestScene testScene = new TestScene();
            Engine.SceneManager.SetScene(testScene).Wait();

            Task.Delay(100).Wait();

            // Assert everything was called correctly.
            Assert.True(testScene.LoadCalled);
            Assert.False(testScene.UpdateCalled);
            Assert.True(testScene.FocusLossCalled > 0);
            Assert.False(testScene.Focused);
            Assert.False(testScene.DrawCalled);

            // Restore focus.
            Engine.ForceUnfocus(false);

            // Cleanup.
            Helpers.UnloadScene();

            Assert.True(testScene.UnloadCalled);
        }
    }
}