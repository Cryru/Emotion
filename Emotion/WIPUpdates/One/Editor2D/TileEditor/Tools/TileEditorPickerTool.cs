using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorPickerTool : TileEditorTool
{
    public TileEditorPickerTool()
    {
        Name = "TilePicker";
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
       
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.White * 0.2f);
        c.RenderOutline(tileInWorld.ToVec3(), tileSize, Color.White, 3f * editor.GetScale());
    }
}

