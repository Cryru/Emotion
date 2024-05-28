#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.WIPUpdates.NewUIUpdate;

#endregion

namespace Emotion.Scenography;

public enum SceneStatus
{
    None,
    Loading,
    Loaded,
    Disposed
}

public abstract class Scene
{
    public SceneStatus Status { get; private set; } = SceneStatus.None;

    public UISystem UI { get; } = new();

    public IEnumerator LoadRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return LoadSceneRoutineAsync();
        yield return new TaskRoutineWaiter(UI.PreloadUI());
        Status = SceneStatus.Loaded;
    }

    public IEnumerator UnloadRoutineAsync()
    {
        UI.Dispose();
        Status = SceneStatus.Disposed;
        yield break;
    }

    public void Update()
    {
        UpdateScene(Engine.DeltaTime);
        UI.Update();
    }

    public void Draw(RenderComposer composer)
    {
        RenderScene(composer);

        composer.SetUseViewMatrix(false);
        composer.SetDepthTest(true);
        composer.ClearDepth();
        UI.Render(composer);
    }

    protected abstract IEnumerator LoadSceneRoutineAsync();

    protected abstract void UpdateScene(float dt);

    protected abstract void RenderScene(RenderComposer c);
}