using Emotion.WIPUpdates.ThreeDee;
using static Emotion.WIPUpdates.One.Editor3D.TerrainEditor.TerrainEditorWindow;

namespace Emotion.WIPUpdates.One.Editor3D.TerrainEditor.Tools;

public class TerrainEditorRaiseLowerTool : TerrainEditorTool
{
    public TerrainEditorRaiseLowerTool()
    {
        Name = "RaiseLowerTerrain";
        IsPlacingTool = true;
        IsPrecisePaint = true;
        HotKey = Platform.Input.Key.B;
    }

    public override void ApplyTool(TerrainEditorWindow editor, TerrainMeshGrid terrain, TerrainBrushGridItem[] brushGrid)
    {
        for (int i = 0; i < brushGrid.Length; i++)
        {
            TerrainBrushGridItem tileInfo = brushGrid[i];
            float influence = tileInfo.Influence;
            if (influence == 0) continue;

            Vector2 tileCoord = tileInfo.TileCoord;

            float brushStrength = 5 * influence;

            float val = terrain.GetAt(tileCoord);
            terrain.ExpandingSetAt(tileCoord, val + brushStrength);
        }
    }
}
