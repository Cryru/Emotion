#nullable enable

using Emotion.WIPUpdates.One.EditorUI.GridEditor;
using Emotion.WIPUpdates.ThreeDee;
using static Emotion.WIPUpdates.One.Editor3D.TerrainEditor.TerrainEditorWindow;

namespace Emotion.WIPUpdates.One.Editor3D.TerrainEditor.Tools;

public abstract class TerrainEditorTool : GridEditorTool
{
    public abstract void ApplyTool(TerrainEditorWindow editor, TerrainMeshGrid terrain, TerrainBrushGridItem[] brushGrid);
}
