#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        /// Whether currently loading.
        /// </summary>
        public bool Loading
        {
            get => Current == LoadingScreen;
        }

        /// <summary>
        /// The current scene. If a scene is loading this is the loading scene.
        /// </summary>
        public IScene Current { get; private set; }

        /// <summary>
        /// The loading scene. This scene is active while another loads/unloads.
        /// </summary>
        public IScene LoadingScreen { get; private set; }

        /// <summary>
        /// When the scene changes.
        /// </summary>
        public EventHandler SceneChanged;

        #endregion

        #region Private Holders

        /// <summary>
        /// A mutex which ensures scene swaps don't happen during update and draw.
        /// </summary>
        private object _swapMutex = new object();

        /// <summary>
        /// The scene to swap to in the next tick.
        /// </summary>
        private IScene _swapScene;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public Task SetScene(IScene scene)
        {
            Engine.Log.Info($"Preparing to swap scene to [{scene}]", MessageSource.SceneManager);

            return Task.Run(() =>
            {
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "Scene Loading";

                // Set the current scene to be the loading screen, and get the old one.
                IScene old = SwapActive(LoadingScreen);

                PerfProfiler.ProfilerEventStart("SceneUnload", "Loading");

                // Unload the old if it isn't the loading screen.
                if (old != LoadingScreen) Unload(old);

                PerfProfiler.ProfilerEventEnd("SceneUnload", "Loading");
                PerfProfiler.ProfilerEventStart("SceneLoad", "Loading");

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

                PerfProfiler.ProfilerEventEnd("SceneLoad", "Loading");
                Engine.Log.Info($"Swapped current scene to [{scene}]", MessageSource.SceneManager);
            });
        }

        /// <summary>
        /// Sets the provided scene as the loading screen.
        /// </summary>
        /// <param name="loadingScene">The scene to set as a loading screen.</param>
        public void SetLoadingScreen(IScene loadingScene)
        {
            // Check if scene is null.
            if (loadingScene == null) return;

            IScene oldLoadingScreen = LoadingScreen;

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

        private static void Load(IScene scene)
        {
            try
            {
                Engine.Log.Trace($"Loading scene [{scene}].", MessageSource.SceneManager);

                scene.Load();

                Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) throw;
                Engine.CriticalError(new Exception($"Couldn't load scene - {scene}.", ex));
            }
        }

        private void Unload(IScene scene)
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
                if (Debugger.IsAttached) throw;
                Engine.CriticalError(new Exception($"Couldn't unload scene - {scene}.", ex));
            }
        }

        /// <summary>
        /// Swaps the current scene and returns the old one (which is the current one).
        /// Used for swapping to a pre-loaded scene. If you haven't loaded your scene yourself use SetScene.
        /// This also won't unload the old scene.
        /// </summary>
        /// <param name="toSwapTo">The scene to swap to.</param>
        /// <returns>The previously active scene (the current one).</returns>
        public IScene SwapActive(IScene toSwapTo)
        {
            lock (_swapMutex)
            {
                // The scene is swapped on the next update.
                if (_swapScene != null)
                {
                    IScene old = _swapScene;
                    _swapScene = toSwapTo;
                    return old;
                }

                _swapScene = toSwapTo;

                return Current;
            }
        }

        /// <summary>
        /// Check whether swapping the scene is needed, and perform it.
        /// </summary>
        private void SwapCheck()
        {
            lock (_swapMutex)
            {
                if (_swapScene == null) return;
                Current = _swapScene;
                _swapScene = null;
            }

            SceneChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}