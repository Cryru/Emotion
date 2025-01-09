using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorBucketTool : TileEditorTool
{
    public TileEditorBucketTool()
    {
        Name = "Bucket";
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

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.Green * 0.2f);
        c.RenderOutline(tileInWorld.ToVec3(), tileSize, Color.Green, 1f);
    }
}

