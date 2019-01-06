// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Debug;

#endregion

namespace Emotion.Engine.Scenography
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

        #endregion

        internal SceneManager()
        {
            LoadingScreen = new DefaultLoadingScene();
            Current = LoadingScreen;
        }

        #region Loops

        /// <summary>
        /// Update all loaded layers.
        /// </summary>
        internal void Update()
        {
            lock (_swapMutex)
            {
                if (Context.Host.Focused)
                {
                    Current.FocusLossCalled = false;
                    Current.Update(Context.FrameTime);
                }
                else if (!Current.FocusLossCalled)
                {
                    Current.FocusLossCalled = true;
                    Current.FocusLoss();
                }
            }
        }

        /// <summary>
        /// Draw all loaded layers.
        /// </summary>
        internal void Draw()
        {
            lock (_swapMutex)
            {
                if (Context.Host.Focused) Current.Draw(Context.Renderer);
            }
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
            // Check if scene is null.
            if (scene == null) return Task.CompletedTask;

            Context.Log.Info($"Preparing to swap scene to [{scene}]", MessageSource.LayerManager);

            return Task.Run(() =>
            {
                Thread.CurrentThread.Name = "Scene Loading Task";

                // Set the current scene to be the loading screen, and get the old one.
                Scene old = SwapActive(LoadingScreen);

                // Unload the old if it isn't the loading screen.
                if (old != LoadingScreen) Unload(old);

                // Load the provided scene.
                Load(scene);

                // Swap from loading.
                SwapActive(scene);

                Context.Log.Info($"Swapped current scene to [{scene}]", MessageSource.LayerManager);
            });
        }

        /// <summary>
        /// Sets the provided scene as the loading screen.
        /// </summary>
        /// <param name="scene">The scene to set as a loading screen.</param>
        public void SetLoadingScreen(Scene scene)
        {
            // Check if scene is null.
            if (scene == null) return;

            scene.Load();
            LoadingScreen = scene;
        }

        #endregion

        #region Helpers

        private void Load(Scene scene)
        {
            try
            {
                Context.Log.Trace($"Loading scene [{scene}].", MessageSource.LayerManager);

                scene.Load();

                Context.Log.Info($"Loaded scene [{scene}].", MessageSource.LayerManager);
            }
            catch (Exception ex)
            {
                Context.Log.Error($"Error while loading scene {scene}.", new Exception("Couldn't load scene.", ex), MessageSource.LayerManager);
            }
        }

        private void Unload(Scene scene)
        {
            try
            {
                Context.Log.Trace($"Unloading scene [{scene}].", MessageSource.LayerManager);

                scene.Unload();

                Context.Log.Info($"Unloaded scene [{scene}].", MessageSource.LayerManager);
            }
            catch (Exception ex)
            {
                Context.Log.Error($"Error while unloading scene [{scene}].", new Exception("Couldn't unload scene.", ex), MessageSource.LayerManager);
            }
        }

        /// <summary>
        /// Swaps the currently active scene safely and returns the old one.
        /// </summary>
        /// <param name="toSwapTo">The scene to swap to.</param>
        /// <returns>The previously active scene.</returns>
        private Scene SwapActive(Scene toSwapTo)
        {
            Scene old;

            lock (_swapMutex)
            {
                old = Current;
                Current = toSwapTo;
            }

            return old;
        }

        #endregion
    }
}