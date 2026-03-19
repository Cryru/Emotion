extern alias SilkNet;
#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using SilkNet::Silk.NET.Assimp;

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
            if (_currentMap != null) return _currentMap;
            _currentMap = InitDefaultMap();
            return _currentMap;
        }
    }
    private GameMap? _currentMap;

    private AssetOwner<GameMapAsset, GameMapFactory> _factoryAssetOwner = new();

    protected SceneWithMap()
    {
        _factoryAssetOwner.SetOnChangeCallback(SceneWithMapFactoryAssetChanged, this);
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
        else
            yield return Map.PreloadAllObjects();

        yield return base.LoadSceneRoutineAsync();
    }

    protected abstract IEnumerator InternalLoadSceneRoutineAsync();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        Map.Dispose();
        _factoryAssetOwner.Done();

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

    #region Map Changing

    private static GameMap InitDefaultMap()
    {
        return new GameMap();
    }

    private static Coroutine SceneWithMapFactoryAssetChanged(AssetOwner<GameMapAsset, GameMapFactory> owner, SceneWithMap self)
    {
        GameMapFactory? newMapFactory = owner.GetCurrentObject();
        if (newMapFactory != null)
            return self.InternalSetMapFromFactory(newMapFactory);
        return Coroutine.CompletedRoutine;
    }

    public Coroutine SetCurrentMap(AssetObjectReference<GameMapAsset, GameMapFactory> factoryAsset)
    {
        return _factoryAssetOwner.Set(factoryAsset) ?? Coroutine.CompletedRoutine;
    }

    public Coroutine SetCurrentMap(GameMap map)
    {
        return InternalSetMapFromMap(map);
    }

    private Coroutine InternalSetMapFromFactory(GameMapFactory factory)
    {
        GameMap mapInstance = factory.CreateMapInstance();
        return InternalSetMapFromMap(mapInstance);
    }

    private Coroutine InternalSetMapFromMap(GameMap map)
    {
        if (_currentMap == null)
        {
            _currentMap = map;
            return Coroutine.CompletedRoutine;
        }

        // Setting map on a scene with an already loaded map
        return Engine.CoroutineManager.StartCoroutine(InternalSetMapFromMapRoutine(map));
    }

    private IEnumerator InternalSetMapFromMapRoutine(GameMap newMap)
    {
        if (newMap.State == GameMapState.Initialized)
            yield return newMap.PreloadAllObjects();
        else
            yield return newMap.InitRoutine();

        GameMap? oldMap = _currentMap;
        AssertNotNull(oldMap);

        _currentMap = newMap;

        oldMap.Dispose();
    }

    #endregion
}