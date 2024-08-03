#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Game.Time.Routines;
using Emotion.Graphics;
using Emotion.UI;
using Emotion.WIPUpdates.NewUIUpdate;
using Emotion.WIPUpdates.One;

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

    public UIBaseWindow UIParent { get; } = new()
    {
        Id = "SceneRoot"
    };

    public GameMap Map { get; protected set; }

    public IEnumerator LoadRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return LoadSceneRoutineAsync();
        Map ??= new GameMap();
        yield return Map.LoadRoutine();

        Engine.UI.AddChild(UIParent);
        yield return new TaskRoutineWaiter(Engine.UI.PreloadUI());

        Status = SceneStatus.Loaded;
    }

    public IEnumerator UnloadRoutineAsync()
    {
        UIParent.Close();
        Status = SceneStatus.Disposed;
        yield break;
    }

    public virtual void UpdateScene(float dt)
    {
        Map.Update(dt);
    }

    public virtual void RenderScene(RenderComposer c)
    {
        Map.Render(c);
    }

    protected abstract IEnumerator LoadSceneRoutineAsync();
}