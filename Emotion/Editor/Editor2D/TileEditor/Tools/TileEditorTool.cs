#nullable enable

using Emotion.Editor.EditorUI.GridEditor;
using Emotion.Game.World.TileMap;

namespace Emotion.Editor.Editor2D.TileEditor.Tools;

public abstract class TileEditorTool : GridEditorTool
{
    public bool RequireTileSelection { get; protected set; }

    public abstract void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos);

    public abstract void RenderCursor(Renderer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos);
}