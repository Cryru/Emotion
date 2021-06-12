#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Standard.Logging;

// ReSharper disable PossibleUnintendedReferenceComparison

#endregion

#nullable enable

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
        public Scene Current { get; private set; }

        /// <summary>
        /// The loading scene. This scene is active while another loads/unloads.
        /// </summary>
        public Scene LoadingScreen { get; private set; }

        /// <summary>
        /// When the scene changes.
        /// </summary>
        public event Action? SceneChanged;

        #endregion

        #region Private Holders

        /// <summary>
        /// A mutex which ensures scene swaps don't happen during update and draw.
        /// </summary>
        private object _swapMutex = new();

        /// <summary>
        /// The scene to swap to in the next tick.
        /// </summary>
        private Scene? _swapScene;

        /// <summary>
        /// The task loading the next scene or loading screen.
        /// </summary>
        private Task? _sceneLoadingTask;

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
        public Task SetScene(Scene scene)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (scene == null) return Task.CompletedTask;

            if (_sceneLoadingTask != null && !_sceneLoadingTask.IsCompleted)
            {
                Engine.Log.Info("Tried to swap scene while a scene swap is in progress.", MessageSource.SceneManager);
                return _sceneLoadingTask;
            }

            Engine.Log.Info($"Preparing to swap scene to [{scene}]", MessageSource.SceneManager);

            _sceneLoadingTask = Task.Run(() => LoadInternal(scene));

            return _sceneLoadingTask;
        }

        private async Task LoadInternal(Scene scene)
        {
            // Set the current scene to be the loading screen, and get the old one.
            Scene old = QueueSceneSwap(LoadingScreen);

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

            try
            {
                // Load the new scene.
                Engine.Log.Trace($"Loading scene [{scene}].", MessageSource.SceneManager);
                await scene.LoadAsync();
                Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);
                QueueSceneSwap(scene); // Swap from loading screen.
                while (_swapScene != null) await Task.Delay(1);
                Engine.Log.Info($"Swapped current scene to [{scene}]", MessageSource.SceneManager);
            }
            catch (Exception ex)
            {
                Engine.CriticalError(new Exception($"Couldn't load scene - {scene}.", ex));
            }

            PerfProfiler.ProfilerEventEnd("SceneLoad", "Loading");
        }

        /// <summary>
        /// Sets the provided scene as the loading screen.
        /// </summary>
        /// <param name="loadingScene">The scene to set as a loading screen.</param>
        public Task SetLoadingScreen(Scene loadingScene)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (loadingScene == null) return Task.CompletedTask;

            if (_sceneLoadingTask != null && !_sceneLoadingTask.IsCompleted)
            {
                Engine.Log.Info("Tried to set loading screen while a scene is loading.", MessageSource.SceneManager);
                return Task.CompletedTask;
            }

            _sceneLoadingTask = Task.Run(() => SetLoadingScreenInternal(loadingScene));

            return _sceneLoadingTask;
        }

        private async Task SetLoadingScreenInternal(Scene loadingScene)
        {
            Scene oldLoadingScreen = LoadingScreen;

            await loadingScene.LoadAsync();
            LoadingScreen = loadingScene;

            // Unload the old loading screen.
            // If it is currently active, swap it with the new loading screen first.
            if (Current == oldLoadingScreen) QueueSceneSwap(LoadingScreen);

            // Wait for swap.
            while (Current != LoadingScreen) await Task.Delay(1);
            Unload(oldLoadingScreen);
        }

        #endregion

        #region Legacy API

        /// <inheritdoc cref="SetScene(Scene)" />
        public Task SetScene(IScene scene)
        {
            return SetScene(new LegacySceneWrapper(scene));
        }

        /// <inheritdoc cref="SetLoadingScreen(Scene)" />
        public void SetLoadingScreen(IScene loadingScene)
        {
            Task loadingSceneTask = SetLoadingScreen(new LegacySceneWrapper(loadingScene));
#if !WEB
            loadingSceneTask.Wait();
#endif
        }

        #endregion

        #region Helpers

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
                if (Debugger.IsAttached) throw;
                Engine.CriticalError(new Exception($"Couldn't unload scene - {scene}.", ex));
            }
        }

        /// <summary>
        /// Queues the provides scene to be swapped on the next update and returns the old one (which is the current one).
        /// Used for swapping to a pre-loaded scene. If you haven't loaded your scene yourself use SetScene, as otherwise the old
        /// one won't be unloaded.
        /// </summary>
        /// <param name="toSwapTo">The scene to queue a swap to.</param>
        /// <returns>The previously active scene (the current one).</returns>
        public Scene QueueSceneSwap(Scene toSwapTo)
        {
            // Engine is setup but not running. Perform instant swap. This can happen if SetScene loads before Run.
            // And is usually not a problem unless the SetScene is awaited.
            if (Engine.Status == EngineStatus.Setup)
            {
                Scene old = Current;
                Current = toSwapTo;
                return old;
            }

            lock (_swapMutex)
            {
                // Check if already waiting on a swap.
                // Can happen if a scene loads in before the loading screen swap is complete.
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

            SceneChanged?.Invoke();
        }

        #endregion
    }
}