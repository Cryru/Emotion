#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TileMap;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Standard.Serialization.XML;

namespace Emotion.Game.World;

public enum GameMapState
{
    Uninitialized,
    LoadingObjects,
    Initialized
}

public partial class GameMap : IDisposable
{
    [DontSerialize]
    public string MapPath { get; internal set; } = "Untitled Map";

    [DontSerialize]
    public GameMapState State { get; private set; } = GameMapState.Uninitialized;

    public IMapGrid[] Grids { get; set; } = Array.Empty<IMapGrid>(); // private

    public LightModel LightModel = LightModel.DefaultLightModel;

    private List<GameObject> _objectsOnMap = new(); // Objects the map was saved with, todo

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

        yield return LoadPendingObjectsRoutine();

        State = GameMapState.Initialized;

        yield break;
    }

    #region Object Management

    public class ObjectFriendAdapter(GameMap map)
    {
        public GameMap Map = map;

        private uint _nextObjectId = 1;

        public uint GetNextObjectId()
        {
            return _nextObjectId++;
        }

        public void OnObjectComponentAdded<TComponent>(GameObject obj) where TComponent : class, IGameObjectComponent
        {

        }

        public void OnObjectComponentRemoved<TComponent>(GameObject obj) where TComponent : class, IGameObjectComponent
        {

        }
    }

    private ObjectFriendAdapter _gameObjectFriendAdapter;

    private IEnumerator LoadPendingObjectsRoutine()
    {
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            yield return LoadSingularObjectRoutine(obj);
        }
    }

    private IEnumerator LoadSingularObjectRoutine(GameObject obj)
    {
        yield return obj.InitRoutine(_gameObjectFriendAdapter);

        //lock (_octTree)
        //{
        //    _octTree.Add(obj);
        //    obj.OnMove += OnObjectMoved;
        //    obj.OnResize += OnObjectMoved;
        //    obj.OnRotate += OnObjectMoved;
        //}

        _objectStorage.AddObject(obj);
    }

    public void AddObject(GameObject obj)
    {
        _objectsToLoad.Enqueue(obj);
    }

    public IEnumerator AddAndInitObject(GameObject obj)
    {
        yield return LoadSingularObjectRoutine(obj);
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

    private IRoutineWaiter _loadNewObjectsRoutine = Coroutine.CompletedRoutine;

    public void Update(float dt)
    {
        foreach (IMapGrid grid in Grids)
        {
            grid.Update(dt);
        }

        // Load objects waiting to be loaded
        if (_loadNewObjectsRoutine.Finished && _objectsToLoad.Count > 0)
        {
            _loadNewObjectsRoutine = Engine.Jobs.Add(LoadPendingObjectsRoutine());
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
    }

    public void Dispose()
    {
        foreach (IMapGrid grid in Grids)
        {
            grid.Done();
        }

        _tileMapData?.Done();

        foreach (GameObject obj in ForEachObject())
        {
            obj.Done();
        }
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
}
