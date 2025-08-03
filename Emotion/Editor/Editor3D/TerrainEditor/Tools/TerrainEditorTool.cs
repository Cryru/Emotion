#nullable enable

using Emotion.Editor.EditorUI.GridEditor;
using Emotion.Game.World.Terrain;
using static Emotion.Editor.Editor3D.TerrainEditor.TerrainEditorWindow;

namespace Emotion.Editor.Editor3D.TerrainEditor.Tools;

public abstract class TerrainEditorTool : GridEditorTool
{
    public abstract void ApplyTool(TerrainEditorWindow editor, TerrainMeshGrid terrain, TerrainBrushGridItem[] brushGrid);
}
