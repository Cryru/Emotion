#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Components;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.ThreeDee;
using Emotion.Game.World.TileMap;
using Emotion.Graphics.Camera;
using Emotion.Primitives;

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
    public GameMapState State { get; private set; } = GameMapState.Uninitialized;

    public string MapFileName = string.Empty;

    private List<GameObject> _objectsOnMap = new(); // Objects the map was saved with, todo

    private List<GameObject> _objects = new(); // Objects currently on the map

    private Queue<GameObject> _objectsToLoad = new(); // Queue of objects to load

    //public List<IMapGrid> Grids = new();
    public GameMapTileData? TileMapData;

    [DontSerialize] // todo: investigate
    public ITerrainGrid3D? TerrainGrid;

    public LightModel LightModel = LightModel.DefaultLightModel;

    public GameMap()
    {
        _adapter = new GameMapToObjectFriendAdapter(this);
    }

    public IEnumerator InitRoutine()
    {
        if (TileMapData != null)
            yield return TileMapData.InitRuntimeDataRoutine();

        if (TerrainGrid != null)
            yield return TerrainGrid.InitRuntimeDataRoutine();

        State = GameMapState.LoadingObjects;

        for (int i = 0; i < _objectsOnMap.Count; i++)
        {
            GameObject obj = _objects[i];
            _objectsToLoad.Enqueue(obj);
        }

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

    private GameMapToObjectFriendAdapter _adapter;

    private IEnumerator LoadPendingObjectsRoutine()
    {
        while (_objectsToLoad.TryDequeue(out GameObject? obj))
        {
            yield return LoadSingularObjectRoutine(obj);
        }
    }

    private IEnumerator LoadSingularObjectRoutine(GameObject obj)
    {
        yield return obj.InitRoutine(_adapter);

        lock (_octTree)
        {
            _octTree.Add(obj);
            obj.OnMove += OnObjectMoved;
            obj.OnResize += OnObjectMoved;
            obj.OnRotate += OnObjectMoved;
        }

        lock (_objects)
        {
            _objects.Add(obj);
        }
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
        lock (_octTree)
        {
            obj.OnMove -= OnObjectMoved;
            obj.OnResize -= OnObjectMoved;
            obj.OnRotate -= OnObjectMoved;
            _octTree.Remove(obj);
        }

        _objects.Remove(obj);
        obj.Done();
    }

    private void OnObjectMoved(GameObject obj)
    {
        lock (_octTree)
        {
            _octTree.Update(obj);
        }
    }

    #endregion

    private Coroutine _loadNewObjectsRoutine = Coroutine.CompletedRoutine;

    public void Update(float dt)
    {
        TerrainGrid?.Update(dt);

        if (_loadNewObjectsRoutine.Finished)
        {
            _loadNewObjectsRoutine = Engine.Jobs.Add(LoadPendingObjectsRoutine());
        }

        lock (_objects)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                var obj = _objects[i];
                obj.Update(dt);
            }

            for (int i = 0; i < _objects.Count; i++)
            {
                GameObject obj = _objects[i];
                MeshComponent? meshComponent = obj.GetComponent<MeshComponent>();
                if (meshComponent != null)
                    meshComponent.Update(dt);
            }
        }
    }

    public void Render(Renderer r)
    {
        Rectangle clipArea = r.Camera.GetCameraView2D();
        TileMapData?.Render(r, clipArea);

        Frustum frustum = r.Camera.GetCameraView3D();
        TerrainGrid?.Render(r, frustum);

        // todo: octree
        bool cameraIs2D = r.Camera is Camera2D;
        if (cameraIs2D)
        {
            lock (_objects)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    GameObject obj = _objects[i];
                    MeshComponent? meshComponent = obj.GetComponent<MeshComponent>();
                    if (meshComponent != null)
                    {
                        if (!meshComponent.AlwaysRender && !obj.GetBoundingRect().Intersects(clipArea)) continue;
                        meshComponent.Render(r);
                    }
                }
            }
        }
        else
        {
            lock (_objects)
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    GameObject obj = _objects[i];
                    MeshComponent? meshComponent = obj.GetComponent<MeshComponent>() ?? obj.GetComponent<SkyBoxComponent>();
                    if (meshComponent != null)
                    {
                        if (!meshComponent.AlwaysRender && !frustum.IntersectsOrContainsCube(obj.GetBoundingCube())) continue;
                        meshComponent.Render(r);
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        TileMapData?.UnloadRuntimeData();
        TerrainGrid?.UnloadRuntimeData();
    }
}
