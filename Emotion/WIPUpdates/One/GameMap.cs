using Emotion.Common.Serialization;
using Emotion.Game.World3D;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.TileMap;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;

namespace Emotion.WIPUpdates.One;

#nullable enable

public partial class GameMap : IDisposable
{
    public string MapFileName = string.Empty;

    private List<MapObject> _objects = new();

    //public List<IMapGrid> Grids = new();
    public GameMapTileData? TileMapData;

    [DontSerialize] // todo: investigate
    public ITerrainGrid3D? TerrainGrid;

    public LightModel LightModel = LightModel.DefaultLightModel;

    public IEnumerator LoadRoutine()
    {
        if (TileMapData != null)
            yield return TileMapData.InitRuntimeDataRoutine();

        if (TerrainGrid != null)
            yield return TerrainGrid.InitRuntimeDataRoutine();

        yield break;
    }

    #region Object Management

    public void AddObject(MapObject obj)
    {
        obj.Map = this;
        _objects.Add(obj);
        obj.Init();

        lock (_octTree)
        {
            _octTree.Add(obj);
            obj.OnMove += OnObjectMoved;
            obj.OnResize += OnObjectMoved;
            obj.OnRotate += OnObjectMoved;
        }
    }

    public IEnumerator AddAndInitObject(MapObject obj)
    {
        AddObject(obj);

        yield break;
    }

    public void RemoveObject(MapObject obj)
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

    private void OnObjectMoved(MapObject obj)
    {
        lock (_octTree)
        {
            _octTree.Update(obj);
        }
    }

    #endregion

    public void Update(float dt)
    {
        TerrainGrid?.Update(dt);

        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Update(dt);
        }
    }

    public void Render(RenderComposer c)
    {
        Rectangle clipArea = c.Camera.GetCameraView2D();
        TileMapData?.Render(c, clipArea);

        Frustum frustum = c.Camera.GetCameraView3D();
        TerrainGrid?.Render(c, frustum);

        // todo: octree
        bool cameraIs2D = c.Camera is Camera2D;
        if (cameraIs2D)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                MapObject obj = _objects[i];
                if (!obj.BoundingRect.Intersects(clipArea)) continue;
                obj.Render(c);
            }
        }
        else
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                MapObject obj = _objects[i];
                if (!frustum.IntersectsOrContainsCube(obj.BoundingCube)) continue;
                obj.Render(c);
            }
        }
    }

    public void Dispose()
    {
        TileMapData?.UnloadRuntimeData();
        TerrainGrid?.UnloadRuntimeData();
    }
}
