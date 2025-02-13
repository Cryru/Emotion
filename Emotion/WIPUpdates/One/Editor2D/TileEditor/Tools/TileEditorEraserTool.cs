#nullable enable

using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorEraserTool : TileEditorTool
{
    public TileEditorEraserTool()
    {
        Name = "Eraser";
        IsPrecisePaint = true;
        HotKey = Platform.Input.Key.R;
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) return;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        if (tileData == null) return;

        bool success = currentLayer.EditorSetTileAt(cursorPos, TileMapTile.Empty);
        if (success)
            editor.UpdateCursor();
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.PrettyPink * 0.2f);
        c.RenderOutline(tileInWorld.ToVec3(), tileSize, Color.PrettyPink, 3f * editor.GetScale());
    }
}

