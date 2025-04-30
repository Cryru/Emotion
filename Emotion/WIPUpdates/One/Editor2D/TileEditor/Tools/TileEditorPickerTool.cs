#nullable enable

using Emotion.Common.Input;
using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorPickerTool : TileEditorTool
{
    public TileEditorPickerTool()
    {
        Name = "TilePicker";
        HotKey = Key.K;
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        TileMapTile tile = currentLayer.GetAt(cursorPos);
        if (tile == TileMapTile.Empty) return;

        TilesetId tsId = tile.TilesetId;
        TileTextureId tId = tile.TextureId;

        TileMapTileset? pickedTileset = null;
        IEnumerable<TileMapTileset> tilesets = editor.GetTilesets();
        int i = 0;
        foreach (var tileset in tilesets)
        {
            if (i == tsId)
            {
                pickedTileset = tileset;
                break;
            }

            i++;
        }
        if (pickedTileset == null) return;

        editor.SelectTileset(pickedTileset);
        editor.TileTextureSelector.AddTileToSelection(tId);
        editor.SetCurrentToolAsLastPlacingTool();
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.PrettyOrange * 0.2f);
        c.RenderOutline(tileInWorld.ToVec3(), tileSize, Color.PrettyOrange, 3f * editor.GetScale());
    }
}

