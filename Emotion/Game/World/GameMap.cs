#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TileMap;
using Emotion.Graphics.Camera;
using System.Runtime.InteropServices;

namespace Emotion.Game.World;

public enum GameMapState
{
    Uninitialized,
    LoadingObjects,
    Initialized
}

public interface IMapGrid
{
    public IEnumerator InitRuntimeDataRoutine();

    public void Done();

    public void Update(float dt);

    public void Render(Renderer r, CameraCullingContext culling);
}

public partial class GameMap : IDisposable
{
    [DontSerialize]
    public GameMapState State { get; private set; } = GameMapState.Uninitialized;

    public string MapFileName = string.Empty;

    public IMapGrid[] Grids = Array.Empty<IMapGrid>();

    public LightModel LightModel = LightModel.DefaultLightModel;

    private List<GameObject> _objectsOnMap = new(); // Objects the map was saved with, todo

    private Queue<GameObject> _objectsToLoad = new(); // Queue of objects to load

    public GameMap()
    {
        _gameObjectFriendAdapter = new GameMapToObjectFriendAdapter(this);
    }

    public IEnumerator InitRoutine()
    {
        var gridLoadingRoutines = new Coroutine[Grids.Length];
        for (int i = 0; i < Grids.Length; i++)
        {
            IMapGrid grid = Grids[i];
            Coroutine routine = Engine.CoroutineManager.StartCoroutine(grid.InitRuntimeDataRoutine());
            gridLoadingRoutines[i] = routine;
        }

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

    public class GameMapToObjectFriendAdapter
    {
        public GameMap Map;

        public GameMapToObjectFriendAdapter(GameMap map)
        {
            Map = map;
        }

        public void OnObjectComponentAdded<TComponent>(GameObject obj) where TComponent : class, IGameObjectComponent
        {

        }

        public void OnObjectComponentRemoved<TComponent>(GameObject obj) where TComponent : class, IGameObjectComponent
        {

        }
    }

    private GameMapToObjectFriendAdapter _gameObjectFriendAdapter;

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

        _enumeration.AddObject(obj);
    }

    public void AddObject(GameObject obj)
    {
        _objectsToLoad.Enqueue(obj);
    }

    public IEnumerator AddAndInitObject(GameObject obj)
    {
        yield return LoadSingularObjectRoutine(obj);
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

        _enumeration.RemoveObject(obj); // Will call obj.Done()
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
    }

    public void Render(Renderer r)
    {
        var culling = new CameraCullingContext(r.Camera);
        foreach (IMapGrid grid in Grids)
        {
            grid.Render(r, culling);
        }

        // todo: quadtree/octree query
        if (culling.Is2D)
        {
            Rectangle rect = culling.Rect2D;
            foreach (GameObject obj in ForEachObject())
            {
                if (!obj.AlwaysRender) continue;
                if (!obj.GetBoundingRect().Intersects(rect)) continue;
                obj.ForEachComponentOfType<IRenderableComponent, Renderer>(static (component, r) => component.Render(r), r);
            }
        }
        else
        {
            Frustum frustum = culling.Frustum;
            foreach (GameObject obj in ForEachObject())
            {
                if (!obj.AlwaysRender) continue;
                if (!frustum.IntersectsOrContainsCube(obj.GetBoundingCube())) continue;
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
    }

    #region Grids

    public void AddGrid(IMapGrid grid)
    {
        if (State == GameMapState.Initialized)
            Coroutine.RunInline(grid.InitRuntimeDataRoutine());

        Array.Resize(ref Grids, Grids.Length + 1);
        Grids[^1] = grid;
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
