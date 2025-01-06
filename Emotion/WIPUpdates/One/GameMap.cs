using Emotion.Game.World.Grid;
using Emotion.WIPUpdates.One.Work;

namespace Emotion.WIPUpdates.One;

public class GameMap
{
    private List<MapObject> _objects = new();

    public List<IMapGrid> Grids = new();

    public IEnumerator LoadRoutine()
    {
        Grids.Add(new TileDataLayerGrid() { TileSize = new Vector2(32) });

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
        for (int i = 0; i < _objects.Count; i++)
        {
            var obj = _objects[i];
            obj.Render(c);
        }
    }
}
