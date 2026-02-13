#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;

namespace Emotion.Core.Systems.Scenography;

public enum SceneStatus
{
    None,
    Loading,
    Loaded,
    Active,
    Disposed
}

[DontSerialize]
public abstract class Scene
{
    public SceneStatus Status { get; protected set; } = SceneStatus.None;

    public virtual IEnumerator Attach()
    {
        Status = SceneStatus.Active;
        yield break;
    }

    public virtual void Detach()
    {
        Status = SceneStatus.Loaded;
    }

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

    public override IEnumerator Attach()
    {
        yield return GLThread.ExecuteOnGLThreadAsync(static (ui) => Engine.UI.AddChild(ui), SceneUI);
        yield return SceneUI.WaitLoadingRoutine();
        yield return Engine.AssetLoader.WaitForAllAssetsToLoadRoutine();

        yield return base.Attach();
    }

    public override void Detach()
    {
        base.Detach();
        SceneUI.Close();
    }

    public override IEnumerator LoadSceneRoutineAsync()
    {
        Status = SceneStatus.Loading;
        yield return InternalLoadSceneRoutineAsync();
        if (Map.State != GameMapState.Initialized)
            yield return Map.InitRoutine();
        yield return base.LoadSceneRoutineAsync();
    }

    protected abstract IEnumerator InternalLoadSceneRoutineAsync();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        Map.Dispose();
        _mapOwner.Done();

        yield return base.UnloadSceneRoutineAsync();
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