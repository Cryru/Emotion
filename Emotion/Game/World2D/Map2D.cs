#region Using

using System.Threading.Tasks;
using Emotion.Editor;
using Emotion.Game.World;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World2D.Tile;
using Emotion.Graphics;

#endregion

#nullable enable

namespace Emotion.Game.World2D;

public class Map2D : BaseMap
{
    /// <summary>
    /// Contains tile information, if the map has a tile map portion.
    /// </summary>
    [DontShowInEditor] public Map2DTileMapData TileData = new();

    public Map2D(Vector2 size, string mapName = "Unnamed Map") : base(size, mapName)
    {
    }

    // Serialization constructor
    protected Map2D()
    {
    }

    public override List<Type> GetValidObjectTypes()
    {
        return EditorUtility.GetTypesWhichInherit<GameObject2D>();
    }

    protected override async Task InitAsyncInternal()
    {
        // During this time object loading is running async.

        // todo: setup runtime info, which includes:
        // (maybe hold that info in the tile data class, so it only holds data that is serialized and then
        // calculates runtime data that can be queried from it?)
        // load tileset textures, cache tileset sizes
        // tileset firsttid
        // layer unpack data and setup size if not full
        if (TileData.SizeInTiles == Vector2.Zero)
        {
            TileData.SizeInTiles = (MapSize / TileData.TileSize).Floor();
        }

        await TileData.LoadTilesetTextures();
    }

    public static Comparison<BaseGameObject> ObjectComparison = ObjectSort; // Prevent delegate allocation

    protected static int ObjectSort(BaseGameObject x, BaseGameObject y)
    {
        return MathF.Sign(x.Position.Z - y.Position.Z);
    }

    public override void Render(RenderComposer c)
    {
        if (!Initialized) return;

        Rectangle clipArea = c.Camera.GetCameraFrustum();
        TileData?.RenderTileMap(c, clipArea);

        var renderObjectsList = new List<BaseGameObject>();
        GetObjects(renderObjectsList, 0, clipArea);
        renderObjectsList.Sort(ObjectComparison);
        for (var i = 0; i < renderObjectsList.Count; i++)
        {
            BaseGameObject obj = renderObjectsList[i];
            obj.Render(c);
        }
    }
}