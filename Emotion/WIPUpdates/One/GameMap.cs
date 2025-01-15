using Emotion.WIPUpdates.One.TileMap;
using Emotion.WIPUpdates.One.Work;

namespace Emotion.WIPUpdates.One;

#nullable enable

public class GameMap
{
    private List<MapObject> _objects = new();

    //public List<IMapGrid> Grids = new();
    public GameMapTileData? TileMapData;

    public IEnumerator LoadRoutine()
    {
        TileMapData = new GameMapTileData();
        TileMapData.Layers.Add(new TileMapLayerGrid() { TileSize = new Vector2(64) });
        TileMapData.Tilesets.Add(new TileMapTileset() { Texture = "Test/tinysword/Tilemap_Flat.png", TileSize = new Vector2(64) });
        TileMapData.Tilesets.Add(new TileMapTileset() { Texture = "Test/tinysword/Tilemap_Elevation.png", TileSize = new Vector2(64) });

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
        Rectangle clipArea = c.Camera.GetCameraFrustum();
        TileMapData?.RenderTileLayerRange(c, clipArea);

        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Render(c);
        }
    }
}
