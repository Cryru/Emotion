using Emotion.Common.Input;
using Emotion.WIPUpdates.One.TileMap;

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

#nullable enable

public class TileEditorBrushTool : TileEditorTool
{
    public TileEditorBrushTool()
    {
        Name = "Brush";
        IsPlacingTool = true;
        IsPrecisePaint = true;
        RequireTileSelection = true;
        HotKey = Key.B;
    }

    private IEnumerable<(TileMapTile tile, Vector2 tilePos)> ForEachTileInPlacement(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorTilePos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) yield break;

        Vector2 tileSize = currentLayer.TileSize;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        if (tileData == null) yield break;

        TilesetId currentTilesetId = editor.GetCurrentTilesetIndex();
        if (currentTilesetId == TilesetId.Invalid) yield break;

        (TileTextureId, Vector2)[] placementPattern = editor.TileTextureSelector.GetSelectedTileTextures(out Vector2 center);
        Vector2 centerInWorldSpace = (center / tileSize).Floor() * tileSize;

        for (int i = 0; i < placementPattern.Length; i++)
        {
            (TileTextureId tId, Vector2 tileSpaceOffset) = placementPattern[i];
            tileSpaceOffset -= center;
            Vector2 currentTilePos = cursorTilePos + tileSpaceOffset;

            var tile = new TileMapTile(tId, currentTilesetId);
            yield return (tile, currentTilePos);

            // Check if the cursor has changed (such as when the map has changed)
            Vector2? newCursorPos = editor.CursorTilePos;
            if (!newCursorPos.HasValue) yield break;
            if (newCursorPos != cursorTilePos)
                cursorTilePos = newCursorPos.Value;
        }
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        AssertNotNull(tileData);
        if (tileData == null) return;

        foreach ((TileMapTile tile, Vector2 tilePos) in ForEachTileInPlacement(editor, currentLayer, cursorPos))
        {
            currentLayer.ExpandingSetAt(tilePos, tile);
        }
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        Vector2 tileSize = currentLayer.TileSize;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        AssertNotNull(tileData);
        if (tileData == null) return;

        foreach ((TileMapTile tile, Vector2 tilePos) in ForEachTileInPlacement(editor, currentLayer, cursorPos))
        {
            Vector2 worldPos = currentLayer.GetWorldPosOfTile(tilePos);
            if (tile != TileMapTile.Empty)
            {
                (Texture texture, Rectangle uv) = tileData.GetTileRenderData(tile);
                c.RenderSprite(worldPos, tileSize, Color.White, texture, uv);
            }

            c.RenderSprite(worldPos, tileSize, Color.Blue * 0.2f);
            c.RenderRectOutline(worldPos, tileSize, Color.PrettyBlue, 3f * editor.GetScale());
        }
    }
}

