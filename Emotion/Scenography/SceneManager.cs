#region Using

using System;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Scenography
{
    /// <summary>
    /// Manages scenes.
    /// </summary>
    public class SceneManager
    {
        #region Properties

        /// <summary>
        /// The current scene. If a scene is loading this is the loading scene.
        /// </summary>
        public Scene Current { get; private set; }

        /// <summary>
        /// The loading scene. This scene is active while another loads/unloads.
        /// </summary>
        public Scene LoadingScreen { get; private set; }

        #endregion

        #region Private Holders

        /// <summary>
        /// A mutex which ensures scene swaps don't happen during update and draw.
        /// </summary>
        private object _swapMutex = new object();

        /// <summary>
        /// The scene to swap to in the next tick.
        /// </summary>
        private Scene _swapScene;

        #endregion

        internal SceneManager()
        {
            LoadingScreen = new DefaultLoadingScene();
            Current = LoadingScreen;
        }

        #region Loops

        /// <summary>
        /// Run the scene's update code.
        /// </summary>
        internal void Update()
        {
            SwapCheck();
            Current.Update();
        }

        /// <summary>
        /// Run the scene drawing code.
        /// </summary>
        internal void Draw(RenderComposer composer)
        {
            Current.Draw(composer);
        }

        #endregion

        #region API

        /// <summary>
        /// Sets the provided scene as the current.
        /// </summary>
        /// <param name="scene">The scene to set as current.</param>
        /// <returns>A thread task which will handle the operations.</returns>
        public Task SetScene(Scene scene)
        {
            Engine.Log.Info($"Preparing to swap scene to [{scene}]", MessageSource.SceneManager);

            return Task.Run(() =>
            {
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "Scene Loading Task";

                // Set the current scene to be the loading screen, and get the old one.
                Scene old = SwapActive(LoadingScreen);

                // Unload the old if it isn't the loading screen.
                if (old != LoadingScreen) Unload(old);

                // Check if a new scene was provided.
                if (scene != null)
                {
                    // Load the provided scene.
                    Load(scene);

                    // Swap from loading.
                    SwapActive(scene);
                }
                else
                {
                    // If not set the current scene to the loading screen.
                    Current = LoadingScreen;
                }

                Engine.Log.Info($"Swapped current scene to [{scene}]", MessageSource.SceneManager);
            });
        }

        /// <summary>
        /// Sets the provided scene as the loading screen.
        /// </summary>
        /// <param name="loadingScene">The scene to set as a loading screen.</param>
        public void SetLoadingScreen(Scene loadingScene)
        {
            // Check if scene is null.
            if (loadingScene == null) return;

            Scene oldLoadingScreen = LoadingScreen;

            // Load the new loading screen.
            loadingScene.Load();
            LoadingScreen = loadingScene;

            // Unload the old loading screen.
            // If it is currently active, swap it with the new loading screen first.
            if (Current == oldLoadingScreen) SwapActive(LoadingScreen);
            Unload(oldLoadingScreen);
        }

        #endregion

        #region Helpers

        private void Load(Scene scene)
        {
            try
            {
                Engine.Log.Trace($"Loading scene [{scene}].", MessageSource.SceneManager);

                scene.Load();

                Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);
            }
            catch (Exception ex)
            {
                Engine.SubmitError(new Exception($"Couldn't load scene - {scene}.", ex));
            }
        }

        private void Unload(Scene scene)
        {
            // Check if trying to unload the current loading screen.
            if (scene == LoadingScreen) return;

            try
            {
                Engine.Log.Trace($"Unloading scene [{scene}].", MessageSource.SceneManager);

                scene.Unload();

                Engine.Log.Info($"Unloaded scene [{scene}].", MessageSource.SceneManager);
            }
            catch (Exception ex)
            {
                Engine.SubmitError(new Exception($"Couldn't unload scene - {scene}.", ex));
            }
        }

        /// <summary>
        /// Queues the scene to be swapped to on the next tick and returns the old one (which is the current one).
        /// </summary>
        /// <param name="toSwapTo">The scene to swap to.</param>
        /// <returns>The previously active scene (the current one).</returns>
        private Scene SwapActive(Scene toSwapTo)
        {
            lock (_swapMutex)
            {
                if (_swapScene != null)
                {
                    Scene old = _swapScene;
                    _swapScene = toSwapTo;
                    return old;
                }

                _swapScene = toSwapTo;

                return Current;
            }
        }

        /// <summary>
        /// Check whether swapping the scene is needed.
        /// </summary>
        private void SwapCheck()
        {
            lock (_swapMutex)
            {
                if (_swapScene == null) return;
                Current = _swapScene;
                _swapScene = null;
            }
        }

        #endregion
    }
}