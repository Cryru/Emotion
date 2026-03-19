#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Game.World.Systems;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TileMap;
using Emotion.Graphics.Camera;

namespace Emotion.Game.World;

public enum GameMapState
{
    Uninitialized,
    LoadingObjects,
    Initialized
}

public partial class GameMap : IDisposable
{
    public GameMapFactory? FactoryCreatedFrom { get; set; }

    [DontSerialize]
    public GameMapState State { get; private set; } = GameMapState.Uninitialized;

    public IMapGrid[] Grids { get; set; } = Array.Empty<IMapGrid>(); // private

    public LightConfig LightConfig = LightConfig.Default;

    private Queue<GameObject> _objectsToLoad = new(); // Queue of objects to load

    public GameMap()
    {
        _gameObjectFriendAdapter = new ObjectFriendAdapter(this);
        _gridFriendAdapter = new GridFriendAdapter(this);
    }

    public IEnumerator InitRoutine()
    {
        var gridLoadingRoutines = new Coroutine[Grids.Length];
        for (int i = 0; i < Grids.Length; i++)
        {
            IMapGrid grid = Grids[i];
            Coroutine routine = Engine.CoroutineManager.StartCoroutine(grid.InitRoutine(_gridFriendAdapter));
            gridLoadingRoutines[i] = routine;
        }

        // Load tile map data if any
        if (_tileMapData != null)
            yield return Engine.CoroutineManager.StartCoroutine(_tileMapData.InitRoutine());

        yield return Coroutine.WhenAll(gridLoadingRoutines);

        State = GameMapState.LoadingObjects;

        //for (int i = 0; i < _objectsOnMap.Count; i++)
        //{
        //    GameObject obj = _objectsOnMap[i];
        //    _objectsToLoad.Enqueue(obj);
        //}

        // Load pending objects, and wait for them to fully initialize
        foreach (GameObject obj in _objectsToLoad)
        {
            obj.Init(_gameObjectFriendAdapter);
        }
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            while (obj.State != GameObjectState.Initialized)
                yield return null;
        }

        State = GameMapState.Initialized;
    }

    public IEnumerator PreloadAllObjects()
    {
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            obj.Init(_gameObjectFriendAdapter);
        }
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            while (obj.State != GameObjectState.Initialized)
                yield return null;
        }
    }

    #region Systems

    private List<IWorldSimulationSystem> _simulationSystems = new();
    private List<IWorldRenderSystem> _renderSystems = new();
    private Dictionary<Type, WorldSystem> _worldSystems = new();

    public TSystem? GetSystem<TSystem>()
        where TSystem : WorldSystem
    {
        if (_worldSystems.TryGetValue(typeof(TSystem), out WorldSystem? val))
        {
            return (TSystem)val;
        }
        return null;
    }

    public void AddSystem<TSystem>(TSystem system)
        where TSystem : WorldSystem
    {
        system.AttachToMap(this);
        _worldSystems.Add(typeof(TSystem), system);
        if (system is IWorldSimulationSystem simSys) _simulationSystems.Add(simSys);
        if (system is IWorldRenderSystem renderSys) _renderSystems.Add(renderSys);
    }

    #endregion

    #region Object Management

    private Dictionary<uint, GameObject> _idToGameObject = new(); 

    public class ObjectFriendAdapter(GameMap map)
    {
        public GameMap Map = map;

        private uint _nextObjectId = 1;

        public uint GetNextObjectId(GameObject obj)
        {
            uint id = _nextObjectId;
            _nextObjectId++;

            Map._idToGameObject.Add(id, obj);

            return id;
        }

        public void OnObjectComponentAdded(GameObject obj, IGameObjectComponent component)
        {
            if (component is ISystemicComponent systemic)
                systemic.Attach(Map, obj);
        }

        public void OnObjectComponentRemoved(GameObject obj, IGameObjectComponent component)
        {
            if (component is ISystemicComponent systemic)
                systemic.Dettach(Map, obj);
        }

        public void AddObjectToWorld(GameObject obj)
        {
            Map._objectStorage.AddObject(obj);
        }
    }

    private ObjectFriendAdapter _gameObjectFriendAdapter;

    public void AddObject(GameObject obj)
    {
        _objectsToLoad.Enqueue(obj);
    }

    public GameObject CreateObject()
    {
        GameObject obj = new GameObject(_gameObjectFriendAdapter);
        AddObject(obj);
        return obj;
    }

    public void RemoveObject(GameObject obj)
    {
        //lock (_octTree)
        //{
        //    obj.OnMove -= OnObjectMoved;
        //    obj.OnResize -= OnObjectMoved;
        //    obj.OnRotate -= OnObjectMoved;
        //    _octTree.Remove(obj);
        //}

        _idToGameObject.Remove(obj.ObjectId);
        _objectStorage.RemoveObject(obj); // Will call obj.Done()
    }

    private void OnObjectMoved(GameObject obj)
    {
        //lock (_octTree)
        //{
        //    _octTree.Update(obj);
        //}
    }

    #endregion

    public void Update(float dt)
    {
        // Load objects waiting to be loaded
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            obj.Init(_gameObjectFriendAdapter);
        }

        foreach (IMapGrid grid in Grids)
        {
            grid.Update(dt);
        }

        for (int i = 0; i < _simulationSystems.Count; i++)
        {
            IWorldSimulationSystem sim = _simulationSystems[i];
            sim.Update(dt);
        }

        foreach (GameObject obj in ForEachObject())
        {
            obj.Update(dt);
            obj.ForEachComponentOfType<IUpdateableComponent, float>(static (component, dt) => component.Update(dt), dt);
        }

        _objectStorage.AssertNoActiveEnumerations();
    }

    public void Render(Renderer r)
    {
        r.MeshEntityRenderer.StartScene(LightConfig);

        var culling = new CameraCullingContext(r.Camera);
        foreach (IMapGrid grid in Grids)
        {
            grid.Render(this, r, culling);
        }

        // todo: quadtree/octree query
        if (culling.Is2D)
        {
            Rectangle rect = culling.Rect2D;
            foreach (GameObject obj in ForEachObject())
            {
                if (!obj.Visible) continue;
                if (!obj.AlwaysRender && !obj.GetBoundingRect().Intersects(rect)) continue;
                obj.ForEachComponentOfType<IRenderableComponent, Renderer>(static (component, r) => component.Render(r), r);
            }
        }
        else
        {
            Frustum frustum = culling.Frustum;
            foreach (GameObject obj in ForEachObject())
            {
                if (!obj.Visible) continue;
                if (!obj.AlwaysRender && !frustum.IntersectsOrContainsCube(obj.GetBoundingCube())) continue;
                obj.ForEachComponentOfType<IRenderableComponent, Renderer>(static (component, r) => component.Render(r), r);
            }
        }

        for (int i = 0; i < _renderSystems.Count; i++)
        {
            IWorldRenderSystem renderSys = _renderSystems[i];
            renderSys.Render(r, culling);
        }

        // In case any component pushed to it
        r.MeshEntityRenderer.EndScene();
    }

    public void Dispose()
    {
        foreach (GameObject obj in ForEachObject())
        {
            obj.Done();
        }

        foreach (KeyValuePair<Type, WorldSystem> sys in _worldSystems)
        {
            sys.Value.Dispose();
        }
        _worldSystems.Clear();
        _simulationSystems.Clear();
        _renderSystems.Clear();

        foreach (IMapGrid grid in Grids)
        {
            grid.Done();
        }

        _tileMapData?.Done();
    }

    #region Grids

    public class GridFriendAdapter(GameMap map)
    {
        public GameMap Map = map;
    }

    private GridFriendAdapter _gridFriendAdapter;

    /// <summary>
    /// Map-wide data for all tile map grids in the map
    /// </summary>
    public TileMapData TileMapData
    {
        get
        {
            _tileMapData ??= new TileMapData();
            return _tileMapData;
        }
        set // remove and use private reflector member for tileMapData
        {
            _tileMapData = value;
        }
    }

    private TileMapData? _tileMapData;

    public void AddGrid(IMapGrid grid)
    {
        if (State == GameMapState.Initialized)
            Coroutine.RunInline(grid.InitRoutine(_gridFriendAdapter));

        Grids = Grids.AddToArray(grid);
    }

    public TGrid? GetFirstGridOfType<TGrid>()
    {
        foreach (IMapGrid grid in Grids)
        {
            if (grid is TGrid tGrid) return tGrid;
        }
        return default;
    }

    #endregion

    #region Object Creation Shortcuts

    public GameObject NewMeshObject(MeshReference meshEntity)
    {
        GameObject gameObject = CreateObject();
        gameObject.AddComponent(new MeshComponent(meshEntity));
        return gameObject;
    }

    public GameObject NewSpriteObject(SpriteReference spriteEntity)
    {
        GameObject gameObject = CreateObject();
        gameObject.AddComponent(new SpriteComponent(spriteEntity));
        return gameObject;
    }

    public GameObject NewSimpleSpriteObject(TextureReference texture)
    {
        GameObject gameObject = CreateObject();
        gameObject.AddComponent(new SimpleSpriteComponent(texture));
        return gameObject;
    }

    public GameObject CreateSkyBox(CubeMapTextureReference cubeTexture)
    {
        GameObject gameObject = CreateObject();
        gameObject.Name = "Skybox";
        gameObject.AlwaysRender = true;
        gameObject.AddComponent(new SkyBoxComponent(cubeTexture));
        return gameObject;
    }

    #endregion
}
