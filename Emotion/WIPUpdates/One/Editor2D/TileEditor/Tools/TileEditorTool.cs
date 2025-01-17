using Emotion.Platform.Input;
using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public abstract class TileEditorTool
{
    public string Name { get; protected set; }

    public bool IsPlacingTool { get; protected set; }

    public bool IsPrecisePaint { get; protected set; }

    public bool RequireTileSelection { get; protected set; }

    public Key HotKey;

    public abstract void ApplyTool(TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos);

    public abstract void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos);
}