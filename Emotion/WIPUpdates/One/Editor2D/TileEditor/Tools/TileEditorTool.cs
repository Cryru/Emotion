using Emotion.WIPUpdates.One.EditorUI.GridEditor;
using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public abstract class TileEditorTool : GridEditorTool
{
    public bool RequireTileSelection { get; protected set; }

    public abstract void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos);

    public abstract void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos);
}