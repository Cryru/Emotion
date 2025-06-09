#nullable enable

using Emotion.Common.Serialization;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One.Editor3D.TerrainEditor.Tools;
using Emotion.WIPUpdates.One.EditorUI.GridEditor;
using Emotion.WIPUpdates.ThreeDee;

namespace Emotion.WIPUpdates.One.Editor3D.TerrainEditor;

[DontSerialize]
public sealed class TerrainEditorWindow : GridEditorWindow
{
    private float _brushSize = 10;

    public TerrainEditorWindow() : base()
    {
    }

    protected override GridEditorTool[] GetTools()
    {
        return [
            new TerrainEditorRaiseLowerTool()
        ];
    }

    protected override void OnOpen()
    {
    }

    protected override void OnClose()
    {
        TerrainMeshGrid? terrain = GetCurrentMapTerrain();
        terrain?.SetEditorBrush(false, 0);
    }

    protected override string GetGridName()
    {
        return "Terrain";
    }

    protected override IGridWorldSpaceTiles? GetCurrentGrid()
    {
        return GetCurrentMapTerrain();
    }

    protected override bool CanEdit()
    {
        return true;
    }

    public struct TerrainBrushGridItem
    {
        public Vector2 TileCoord;
        public float Influence;
    }

    public TerrainBrushGridItem[] BrushGrid { get => _brushGrid; private set => _brushGrid = value; }

    private TerrainBrushGridItem[] _brushGrid = Array.Empty<TerrainBrushGridItem>();

    protected override Vector2 UpdateCursor()
    {
        TerrainMeshGrid? terrain = GetCurrentMapTerrain();
        AssertNotNull(terrain);

        terrain.SetEditorBrush(true, _brushSize);

        // Construct brush grid
        Vector2 tileSize = terrain.TileSize;
        Vector2 brushPosWorld = terrain.GetEditorBrushWorldPosition();

        Rectangle brushRect = new Rectangle(0, 0, new Vector2(_brushSize * 2));
        brushRect.Center = brushPosWorld - tileSize / 2f;

        Rectangle brushRectSnapped = brushRect;
        brushRectSnapped.SnapToGrid(tileSize);
        brushRectSnapped.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        min /= tileSize;
        max /= tileSize;

        min = min.Round();
        max = max.Round();

        int brushRectSizeY = (int)(max.Y - min.Y);
        int brushRectSizeX = (int)(max.X - min.X);

        int brushInfluenceSize = brushRectSizeX * brushRectSizeY;
        if (brushInfluenceSize > BrushGrid.Length)
            Array.Resize(ref _brushGrid, brushInfluenceSize);

        int tile = 0;
        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileWorldPos = terrain.GetWorldPosOfTile(tileCoord);

                float distToTile = Vector2.Distance(tileWorldPos, brushPosWorld);
                float falloff = MathF.Exp(-MathF.Pow(distToTile, 2) / (2 * MathF.Pow(_brushSize * 0.5f, 2)));
                falloff = MathF.Max(falloff, 0f);

                BrushGrid[tile] = new TerrainBrushGridItem()
                {
                    Influence = falloff,
                    TileCoord = tileCoord
                };
                tile++;
            }
        }

        return terrain.GetTilePosOfWorldPos(brushPosWorld);
    }

    protected override void UseCurrentToolAtPosition(Vector2 tilePos)
    {
        TerrainMeshGrid? terrain = GetCurrentMapTerrain();
        AssertNotNull(terrain);

        if (CurrentTool is TerrainEditorTool terrainTool)
            terrainTool.ApplyTool(this, terrain, _brushGrid);

        // Smooth
        //float averageVal = 0;
        //int values = 0;
        //for (int i = 0; i < _brushGrid.Length; i++)
        //{
        //    BrushGrid tileInfo = _brushGrid[i];
        //    float influence = tileInfo.Influence;
        //    if (influence == 0) continue;

        //    Vector2 tileCoord = tileInfo.TileCoord;

        //    float val = terrain.GetAt(tileCoord);
        //    averageVal += val;
        //    values++;
        //}

        //averageVal = averageVal / values;
        //for (int i = 0; i < _brushGrid.Length; i++)
        //{
        //    BrushGrid tileInfo = _brushGrid[i];
        //    float influence = tileInfo.Influence;
        //    if (influence == 0) continue;

        //    Vector2 tileCoord = tileInfo.TileCoord;
        //    float val = terrain.GetAt(tileCoord);

        //    float brushStrength = 5 * influence;

        //    float diff = averageVal - val;
        //    float diffAbs = MathF.Abs(diff);
        //    val += MathF.Min(diffAbs, brushStrength) * MathF.Sign(diff);
        //    terrain.ExpandingSetAt(tileCoord, val);
        //}
    }

    public TerrainMeshGrid? GetCurrentMapTerrain()
    {
        GameMap? map = EngineEditor.GetCurrentMap();
        return map?.TerrainGrid as TerrainMeshGrid;
    }
}
