#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;

#endregion

namespace Emotion.Core.Systems.Scenography;

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
    internal void Draw(Renderer composer)
    {
        Current.RenderScene(composer);
    }

    #endregion

    public Coroutine SetScene(Scene scene)
    {
        Engine.Log.Info($"Set scene: [{scene}]", MessageSource.SceneManager);
        return Engine.Jobs.Add(InternalLoadSceneRoutineAsync(scene));
    }

    public Coroutine SetLoadingScreen(Scene loadingScene)
    {
        return Engine.Jobs.Add(InternalLoadLoadingScreenRoutineAsync(loadingScene));
    }

    private IEnumerator InternalLoadSceneRoutineAsync(Scene scene)
    {
        Engine.Log.Info($"Loading scene [{scene}].", MessageSource.SceneManager);

        // Set the loading screen as the current scene.
        // This will unload the previous scene as well.

        // We want to do this in a coroutine so that the scene swap
        // happens in between updates (on the main thread) and to avoid async troubles.
        yield return Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(LoadingScreen));

        yield return scene.LoadSceneRoutineAsync();

        // We're in a loading screen - might as well wait for all assets to load :P
        yield return Engine.AssetLoader.WaitForAllAssetsToLoadRoutine();

        // Swap current to new scene.
        Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(scene));

        Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);
    }

    // Ensure the scene swap happens safely while the scene isn't executing.
    private IEnumerator SceneSwapSynchronized(Scene scene)
    {
        // yield once to avoid eager routines running us another thread
        yield return null;
        Assert(GLThread.IsGLThread());

        Scene oldScene = Current;
        if (oldScene is SceneWithMap oldSceneWithMap)
            oldSceneWithMap.SceneUI.Close();
        Current = scene;
        if (scene is SceneWithMap newSceneWithMap)
            Engine.UI.AddChild(newSceneWithMap.SceneUI);

        OnSceneChanged?.Invoke();

        // Start job of cleaning up old scene.
        if (oldScene != LoadingScreen)
            Engine.Jobs.Add(oldScene.UnloadSceneRoutineAsync());

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

        Engine.Jobs.Add(loadingLoadingScreen.UnloadSceneRoutineAsync());

        yield break;
    }
}