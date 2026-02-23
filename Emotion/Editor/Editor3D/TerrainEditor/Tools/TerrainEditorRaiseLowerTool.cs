#nullable enable

using Emotion.Game.World.Terrain;
using static Emotion.Editor.Editor3D.TerrainEditor.TerrainEditorWindow;

namespace Emotion.Editor.Editor3D.TerrainEditor.Tools;

public class TerrainEditorRaiseLowerTool : TerrainEditorTool
{
    public TerrainEditorRaiseLowerTool()
    {
        Name = "RaiseLowerTerrain";
        IsPlacingTool = true;
        IsPrecisePaint = true;
        HotKey = Key.B;
    }

    public override void ApplyTool(TerrainEditorWindow editor, TerrainMeshGridNew terrain, TerrainBrushGridItem[] brushGrid)
    {
        //terrain.ApplyBrushHeight(
        //    Engine.Host.IsAltModifierHeld() ?
        //        TerrainMeshGridNew.BrushOperation.Rise :
        //        TerrainMeshGridNew.BrushOperation.Lower
        //);
    }
}
