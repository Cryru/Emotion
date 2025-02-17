#region Using

using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World;
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
    public Map2DTileMapData Tiles = new();

    // legacy property name for tiles, dont use
    [SerializeNonPublicGetSet]
    [DontShowInEditor]
    public Map2DTileMapData TileData { protected get => null; set => Tiles = value; }

    public Map2D(Vector2 size, string mapName = "Unnamed Map") : base(size, mapName)
    {
    }

    // Serialization constructor
    protected Map2D()
    {
    }

    public override void EditorCreateInitialize()
    {
        base.EditorCreateInitialize();

        // Initialize at least one layer.
        Tiles.Layers.Add(new Map2DTileMapLayer("First Layer", Array.Empty<uint>()));
    }

    public override List<Type> GetValidObjectTypes()
    {
        return EditorUtility.GetTypesWhichInherit<GameObject2D>();
    }

    protected override async Task InitAsyncInternal()
    {
        // Initialize tile map data, this will load assets and generate
        // data that depends on the map.
        // During this time object loading is running async.
        await Tiles.InitRuntimeState(this);

        // todo: setup runtime info, which includes:
        // (maybe hold that info in the tile data class, so it only holds data that is serialized and then
        // calculates runtime data that can be queried from it?)
        // load tileset textures, cache tileset sizes
        // tileset firsttid
        // layer unpack data and setup size if not full
    }

    // 2d objects are sorted by Z before rendered
    protected static int ObjectSort(BaseGameObject x, BaseGameObject y)
    {
        return MathF.Sign(x.Position.Z - y.Position.Z);
    }

    // comparison is cached to prevent delegate allocation
    public static Comparison<BaseGameObject> ObjectComparison = ObjectSort;

    // list is cached to prevent list allocation, ObjectsGet clears it automatically
    protected List<BaseGameObject> _objectsRenderedThisFrame = new();

    public override void Render(RenderComposer c)
    {
        if (!Initialized) return;

        Rectangle clipArea = c.Camera.GetCameraView2D();
        Tiles.RenderTileMap(c, clipArea);

        _objectsRenderedThisFrame = ObjectsGet(_objectsRenderedThisFrame, clipArea);
        _objectsRenderedThisFrame.Sort(ObjectComparison);
        for (var i = 0; i < _objectsRenderedThisFrame.Count; i++)
        {
            BaseGameObject obj = _objectsRenderedThisFrame[i];
            obj.Render(c);
        }
    }
}