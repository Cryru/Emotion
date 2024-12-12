#region Using

using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;

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
        public event Action? OnSceneChanged;

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
            Current.UpdateScene(Engine.DeltaTime);
        }

        /// <summary>
        /// Run the scene drawing code.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Draw(RenderComposer composer)
        {
            Current.RenderScene(composer);
        }

        #endregion

        public PassiveRoutineObserver SetScene(Scene scene)
        {
            Engine.Log.Info($"Setting scene to [{scene}]", MessageSource.SceneManager);
            Coroutine coroutine = Engine.CoroutineManagerAsync.StartCoroutine(InternalLoadSceneRoutineAsync(scene));
            return new PassiveRoutineObserver(coroutine);
        }

        public PassiveRoutineObserver SetLoadingScreen(Scene loadingScene)
        {
            Coroutine coroutine = Engine.CoroutineManagerAsync.StartCoroutine(InternalLoadLoadingScreenRoutineAsync(loadingScene));
            return new PassiveRoutineObserver(coroutine);
        }

        private IEnumerator InternalLoadSceneRoutineAsync(Scene scene)
        {
            // Set the loading screen as the current scene.
            // This will unload the previous scene as well. 
            Coroutine swapToLoadingRoutine = Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(LoadingScreen));
            yield return new PassiveRoutineObserver(swapToLoadingRoutine);

            Engine.Log.Trace($"Loading scene [{scene}].", MessageSource.SceneManager);
            yield return scene.LoadSceneRoutineAsync();
            Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);

            // Swap current to new scene.
            Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(scene));
        }

        // Ensure the scene swap happens safely while the scene isn't executing.
        private IEnumerator SceneSwapSynchronized(Scene scene)
        {
            Scene oldScene = Current;
            Current = scene;
            OnSceneChanged?.Invoke();
            if (oldScene != LoadingScreen)
                Engine.CoroutineManagerAsync.StartCoroutine(oldScene.UnloadSceneRoutineAsync());

            yield break;
        }

        private IEnumerator InternalLoadLoadingScreenRoutineAsync(Scene scene)
        {
            yield return scene.LoadSceneRoutineAsync();
            Engine.CoroutineManager.StartCoroutine(LoadingScreenSceneSwapSynchronized(scene));
        }

        private IEnumerator LoadingScreenSceneSwapSynchronized(Scene scene)
        {
            Scene loadingLoadingScreen = LoadingScreen;
            LoadingScreen = scene;

            if (Current == loadingLoadingScreen)
                Current = scene;

            Engine.CoroutineManagerAsync.StartCoroutine(loadingLoadingScreen.UnloadSceneRoutineAsync());

            yield break;
        }
    }
}