#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Editor;

#endregion

namespace Emotion.Core.Systems.Scenography;

/// <summary>
/// Manages scenes.
/// </summary>
public class SceneManager
{
    #region Properties

    /// <summary>
    /// The current scene.
    /// </summary>
    public Scene Current { get; private set; }

    /// <summary>
    /// The loading scene. This scene is active while another loads/unloads.
    /// </summary>
    public Scene LoadingScreen { get; private set; }

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
    internal void Update(float dt)
    {
        if (Current.Status == SceneStatus.Active)
            Current.UpdateScene(dt);
        else
            LoadingScreen.UpdateScene(dt);
    }

    /// <summary>
    /// Run the scene drawing code.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Draw(Renderer composer)
    {
        if (Current.Status == SceneStatus.Active)
            Current.RenderScene(composer);
        else
            LoadingScreen.RenderScene(composer);
    }

    #endregion

    public IRoutineWaiter SetScene(Scene scene)
    {
        Engine.Log.Info($"Set scene: [{scene}]", MessageSource.SceneManager);
        return Engine.Jobs.Add(InternalLoadSceneRoutineAsync(scene));
    }

    public IRoutineWaiter SetLoadingScreen(Scene loadingScene)
    {
        return Engine.Jobs.Add(InternalLoadLoadingScreenRoutineAsync(loadingScene));
    }

    private IEnumerator InternalLoadSceneRoutineAsync(Scene scene)
    {
        // Load the new scene
        Engine.Log.ONE_Info(nameof(SceneManager), $"Loading scene [{scene}].");
        yield return scene.LoadSceneRoutineAsync();
        Engine.Log.ONE_Info(nameof(SceneManager), $"Loaded scene [{scene}].");

        // Swap the new scene as the current - we do this on the
        // main thread to avoid async troubles.
        Scene oldCurrent = Current;
        yield return Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(scene));

        // Start job of cleaning up old scene
        // Reference counts should have transferred ownership to the new scene (for any shared resources)
        Engine.Jobs.AddNoFeedback(oldCurrent.UnloadSceneRoutineAsync());
    }

    // Ensure the scene swap happens safely while the scene isn't executing.
    private IEnumerator SceneSwapSynchronized(Scene scene)
    {
        // yield once to avoid eager routines running us on another thread
        yield return null;
        Assert(GLThread.IsGLThread());

        Current.Detach();
        Current = scene;
        scene.Attach();
        Engine.Log.ONE_Trace(nameof(SceneManager), $"Scene swapped.");
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
        Engine.Jobs.Add(loadingLoadingScreen.UnloadSceneRoutineAsync());

        yield break;
    }
}