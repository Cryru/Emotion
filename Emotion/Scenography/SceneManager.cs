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
        public event EventHandler SceneChanged;

        private Task _sceneLoadingTask;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            if (_sceneLoadingTask != null && !_sceneLoadingTask.IsCompleted)
            {
                Engine.Log.Info("Tried to swap scene while a scene swap is in progress.", MessageSource.SceneManager);
                return _sceneLoadingTask;
            }

            Engine.Log.Info($"Preparing to swap scene to [{scene}]", MessageSource.SceneManager);

            _sceneLoadingTask = new Task(async () =>
            {
                if (Engine.Host?.NamedThreads ?? false) Thread.CurrentThread.Name ??= "Scene Loading";

#if WEB
                // Make sure assets have loaded.
                if (AssetBlobLoadingTask != null) await AssetBlobLoadingTask;
#endif

                // Set the current scene to be the loading screen, and get the old one.
                IScene old = SwapActive(LoadingScreen);

                PerfProfiler.ProfilerEventStart("SceneUnload", "Loading");

                // Unload the old if it isn't the loading screen.
                if (old != LoadingScreen)
                {
                    // Wait for the scene to swap to the loading screen.
                    // We don't want to unload it while it is still being updated/drawn.
                    while (Current != LoadingScreen) await Task.Delay(1);

                    Unload(old);
                }

                PerfProfiler.ProfilerEventEnd("SceneUnload", "Loading");
                PerfProfiler.ProfilerEventStart("SceneLoad", "Loading");

                // Check if a new scene was provided.
                if (scene != null)
                {
                    // Load the provided scene.
                    await Load(scene);

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
            return _sceneLoadingTask;
        }

#if WEB
        public static Task AssetBlobLoadingTask;
#endif

        /// <summary>
        /// Sets the provided scene as the loading screen.
        /// </summary>
        /// <param name="loadingScene">The scene to set as a loading screen.</param>
        public void SetLoadingScreen(IScene loadingScene)
        {
            // Check if scene is null.
            if (loadingScene == null) return;

            IScene oldLoadingScreen = LoadingScreen;

#if WEB
            Task.Run(async () =>
            {
                // Wait for asset blobs to load.
                if (AssetBlobLoadingTask != null) await AssetBlobLoadingTask;

                await loadingScene.Load();
                LoadingScreen = loadingScene;

                if (Current == oldLoadingScreen) SwapActive(LoadingScreen);
                Unload(oldLoadingScreen);
            });
#else
            // Load the new loading screen.
            loadingScene.Load();
            LoadingScreen = loadingScene;

            // Unload the old loading screen.
            // If it is currently active, swap it with the new loading screen first.
            if (Current == oldLoadingScreen) SwapActive(LoadingScreen);
            Unload(oldLoadingScreen);
#endif
        }

        #endregion

        #region Helpers

        private static async Task Load(IScene scene)
        {
#pragma warning disable 1998
            async Task LoadFunc()
#pragma warning restore 1998
            {
                Engine.Log.Trace($"Loading scene [{scene}].", MessageSource.SceneManager);
#if WEB
                await scene.Load();
#else
                scene.Load();
#endif
                Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);
            }

            if (Engine.Configuration.DebugMode && Debugger.IsAttached)
                await LoadFunc();
            else
                // This try doesn't actually prevent a game crash. It just makes sure the exception is logged.
                try
                {
                    await LoadFunc();
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
#if WEB
            if (AssetBlobLoadingTask != null && !AssetBlobLoadingTask.IsCompleted) return;
#endif
            if (_sceneLoadingTask != null && _sceneLoadingTask.Status == TaskStatus.Created) _sceneLoadingTask.Start();

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