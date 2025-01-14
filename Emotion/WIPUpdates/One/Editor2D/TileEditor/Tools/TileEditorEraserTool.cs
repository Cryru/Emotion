using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

#nullable enable

public class TileEditorEraserTool : TileEditorTool
{
    public TileEditorEraserTool()
    {
        Name = "Eraser";
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) return;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        if (tileData == null) return;

        bool success = currentLayer.EditorSetTileAt(cursorPos, TileMapTile.Empty, out bool layerBoundsChanged);
        if (success)
        {
            if (layerBoundsChanged)
            {
                tileData.EditorUpdateRenderCacheForLayer(currentLayer);
                editor.UpdateCursor();
            }
            else
            {
                tileData.EditorUpdateRenderCacheForTile(currentLayer, cursorPos);
            }
        }
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.PrettyPink * 0.2f);
        c.RenderOutline(tileInWorld.ToVec3(), tileSize, Color.PrettyPink, 3f * editor.GetScale());
    }
}

