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
    public TerrainMeshGrid? TerrainGrid;

    public IEnumerator LoadRoutine()
    {
        if (TileMapData != null)
            yield return TileMapData.InitRuntimeDataRoutine();

        yield break;
    }

    public List<MapObject> ForEachObject()
    {
        return _objects;
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
}
