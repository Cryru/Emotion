using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorBrushTool : TileEditorTool
{
    public TileEditorBrushTool()
    {
        Name = "Brush";
        IsPlacingTool = true;
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor)
    {
        Vector2? cursorTilePos = editor.CursorTilePos;
        AssertNotNull(cursorTilePos);

        TileMapLayerGrid currentLayer = editor.CurrentLayer;
        AssertNotNull(currentLayer);

        Vector2 cursorTile = cursorTilePos.Value;
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorTile);
        Vector2 tileSize = currentLayer.TileSize;

        Vector2 tileWorldOffset = new Vector2(0);
        c.RenderSprite((tileInWorld + tileWorldOffset).ToVec3(), tileSize, Color.Blue * 0.2f);
        c.RenderOutline((tileInWorld + tileWorldOffset).ToVec3(), tileSize, Color.PrettyBlue, 1f);
    }
}

