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
        if (EngineEditor.IsOpen)
            dt = 0; // todo: do this via game time stop

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
        Engine.Log.Info($"Loading scene [{scene}].", MessageSource.SceneManager);

        // Change the current scene
        // We want to do this in a coroutine so that the scene swap
        // happens in between updates (on the main thread) and to avoid async troubles.
        Scene oldCurrent = Current;
        yield return Engine.CoroutineManager.StartCoroutine(SceneSwapSynchronized(scene));

        // Start loading the new scene
        yield return scene.LoadSceneRoutineAsync();
        yield return scene.Attach();

        Engine.Log.Info($"Loaded scene [{scene}].", MessageSource.SceneManager);

        // Start job of cleaning up old scene
        // We do this after the loading of the new one to allow reference counts to transfer ownership.
        Engine.Jobs.Add(oldCurrent.UnloadSceneRoutineAsync());
    }

    // Ensure the scene swap happens safely while the scene isn't executing.
    private IEnumerator SceneSwapSynchronized(Scene scene)
    {
        // yield once to avoid eager routines running us on another thread
        yield return null;
        Assert(GLThread.IsGLThread());

        Current.Detach();
        Current = scene;
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