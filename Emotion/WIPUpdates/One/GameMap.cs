using Emotion.Common.Serialization;
using Emotion.Graphics.Camera;
using Emotion.WIPUpdates.One.TileMap;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;

namespace Emotion.WIPUpdates.One;

#nullable enable

public class GameMap
{
    public string MapFileName = string.Empty;

    private List<MapObject> _objects = new();

    //public List<IMapGrid> Grids = new();
    public GameMapTileData? TileMapData;

    [DontSerialize] // todo: investigate
    public ITerrainGrid3D? TerrainGrid;

    public IEnumerator LoadRoutine()
    {
        if (TileMapData != null)
            yield return TileMapData.InitRuntimeDataRoutine();

        if (TerrainGrid != null)
            yield return TerrainGrid.InitRuntimeDataRoutine();

        yield break;
    }

    public List<MapObject> ForEachObject()
    {
        return _objects;
    }

    public IEnumerable<T> ForEachObject<T>() where T : MapObject
    {
        // todo: convert to Ienumerable struct with lazy eval
        foreach (MapObject obj in _objects)
        {
            if (obj is T objT)
                yield return objT;
        }
    }

    public void AddObject(MapObject obj)
    {
        obj.Map = this;
        _objects.Add(obj);
        obj.LoadAssets(Engine.AssetLoader);
        obj.Init();
    }

    public void AddAndInitObject(MapObject obj)
    {
        AddObject(obj);
    }

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
        TerrainGrid?.Render(c, clipArea);

        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Render(c);
        }
    }

    #region Collision

    public bool CollideRayWithObjects(Ray2D ray, MapObject? exclude, out Vector2 collisionPoint)
    {
        foreach (MapObject obj in ForEachObject())
        {
            if (obj == exclude) continue;

            Rectangle bounds = obj.BoundingRect;
            if (ray.IntersectWithRectangle(bounds, out collisionPoint))
                return true;
        }

        collisionPoint = Vector2.Zero;
        return false;
    }

    public bool CollideRayWithObjects<T>(Ray2D ray, out Vector2 collisionPoint) where T : MapObject
    {
        return CollideRayWithObjects<T>(ray, null, out collisionPoint);
    }

    public bool CollideRayWithObjects<T>(Ray2D ray, MapObject? exclude, out Vector2 collisionPoint) where T: MapObject
    {
        foreach (MapObject obj in ForEachObject())
        {
            if (obj is not T) continue;
            if (obj == exclude) continue;

            Rectangle bounds = obj.BoundingRect;
            if (ray.IntersectWithRectangle(bounds, out collisionPoint))
                return true;
        }

        collisionPoint = Vector2.Zero;
        return false;
    }

    #endregion
}
