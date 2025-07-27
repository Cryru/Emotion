#nullable enable

using Emotion.Game.Terrain;
using Emotion.WIPUpdates.One.EditorUI.GridEditor;
using static Emotion.WIPUpdates.One.Editor3D.TerrainEditor.TerrainEditorWindow;

namespace Emotion.WIPUpdates.One.Editor3D.TerrainEditor.Tools;

public abstract class TerrainEditorTool : GridEditorTool
{
    public abstract void ApplyTool(TerrainEditorWindow editor, TerrainMeshGrid terrain, TerrainBrushGridItem[] brushGrid);
}
