#region Using

using System.Collections;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
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

[DontSerialize]
public abstract class Scene
{
    public SceneStatus Status { get; protected set; } = SceneStatus.None;

    public virtual IEnumerator LoadSceneRoutineAsync()
    {
        Status = SceneStatus.Loaded;
        yield break;
    }

    public virtual IEnumerator UnloadSceneRoutineAsync()
    {
        Status = SceneStatus.Disposed;
        yield break;
    }

    public virtual void UpdateScene(float dt)
    {

    }

    public virtual void RenderScene(RenderComposer c)
    {

    }
}

[DontSerialize]
public abstract class SceneWithMap : Scene
{
    public UIBaseWindow UIParent { get; } = new()
    {
        Id = "SceneRoot"
    };

    public GameMap Map { get; protected set; }

    public override IEnumerator LoadSceneRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return InternalLoadSceneRoutineAsync();
        Map ??= new GameMap()
        {
            MapFileName = "Maps/start.xml"
        };
        yield return Map.LoadRoutine();

        Engine.UI.AddChild(UIParent);
        yield return new TaskRoutineWaiter(Engine.UI.PreloadUI());

        Status = SceneStatus.Loaded;
    }
    protected abstract IEnumerator InternalLoadSceneRoutineAsync();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        UIParent.Close();
        Status = SceneStatus.Disposed;
        yield break;
    }

    public override void UpdateScene(float dt)
    {
        Map.Update(dt);
    }

    public override void RenderScene(RenderComposer c)
    {
        Map.Render(c);
    }
}