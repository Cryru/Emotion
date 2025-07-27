#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.UI;
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

    public GameMap Map
    {
        get
        {
            if (_map == null) _map = InitDefaultMap();
            return _map;
        }
        protected set
        {
            // unload old?
            _map = value;
        }
    }
    private GameMap _map;

    public override IEnumerator LoadSceneRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return InternalLoadSceneRoutineAsync();
        yield return Map.LoadRoutine();

        // todo: preload ui assets or something
        //yield return new TaskRoutineWaiter(Engine.UI.PreloadUI());

        Status = SceneStatus.Loaded;
    }
    protected abstract IEnumerator InternalLoadSceneRoutineAsync();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
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

    #region Map Helpers

    private static GameMap InitDefaultMap()
    {
        return new GameMap()
        {
            MapFileName = "Maps/start.xml"
        };
    }

    #endregion
}