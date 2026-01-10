#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.UI;

namespace Emotion.Core.Systems.Scenography;

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

    public virtual void RenderScene(Renderer r)
    {

    }
}

[DontSerialize]
public abstract class SceneWithMap : Scene
{
    public UIBaseWindow SceneUI { get; } = new()
    {
        Name = "SceneRoot"
    };

    public GameMap Map
    {
        get
        {
            GameMap? gameMap = _mapOwner.GetCurrentObject();
            if (gameMap == null)
            {
                _mapOwner.Set(InitDefaultMap());
                gameMap = _mapOwner.GetCurrentObject();
                AssertNotNull(gameMap);
            }

            return gameMap;
        }
    }

    private AssetOwner<GameMapAsset, GameMap> _mapOwner = new AssetOwner<GameMapAsset, GameMap>();

    protected SceneWithMap()
    {
        _mapOwner = new AssetOwner<GameMapAsset, GameMap>();
        _mapOwner.SetOnChangeCallback(static (owner, scene) =>
        {
            GameMap? newMapObj = owner.GetCurrentObject();
            if (newMapObj != null && scene is SceneWithMap sc)
                sc.OnMapChanged(newMapObj);
        }, this);
    }

    public override IEnumerator LoadSceneRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return InternalLoadSceneRoutineAsync();
        if (Map.State != GameMapState.Initialized)
            yield return Map.InitRoutine();
        yield return SceneUI.WaitLoadingRoutine();

        Status = SceneStatus.Loaded;
    }
    protected abstract IEnumerator InternalLoadSceneRoutineAsync();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        _mapOwner.Done();
        Status = SceneStatus.Disposed;
        yield break;
    }

    public override void UpdateScene(float dt)
    {
        Map.Update(dt);
    }

    public override void RenderScene(Renderer r)
    {
        Map.Render(r);
    }

    #region Map Helpers

    private static GameMap InitDefaultMap()
    {
        return new GameMap()
        {
            MapPath = "Maps/start.xml"
        };
    }

    public Coroutine SetCurrentMap(AssetObjectReference<GameMapAsset, GameMap> asset)
    {
        return _mapOwner.Set(asset) ?? Coroutine.CompletedRoutine;
    }

    private void OnMapChanged(GameMap newMap)
    {

    }

    #endregion
}