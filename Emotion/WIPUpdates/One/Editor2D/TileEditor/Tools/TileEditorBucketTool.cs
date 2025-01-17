using Emotion.Graphics.Camera;
using Emotion.WIPUpdates.One.TileMap;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public class TileEditorBucketTool : TileEditorTool
{
    public TileEditorBucketTool()
    {
        Name = "Bucket";
        IsPlacingTool = true;
        RequireTileSelection = true;
    }

    private Stack<(Vector2, TileMapTile)> _spanFillStack = new Stack<(Vector2, TileMapTile)>();
    protected HashSet<Vector2> _bucketTilesToSet = new();

    private HashSet<Vector2> SpanFill(Vector2 cursorPosition, TileMapTile previousTileData, TileMapLayerGrid layer)
    {
        _bucketTilesToSet.Clear();
        _spanFillStack.Clear();
        _spanFillStack.Push((cursorPosition, previousTileData));

        while (_spanFillStack.Count > 0)
        {
            var (cursorpos, currentTile) = _spanFillStack.Pop();
            var lx = cursorpos.X;
            while (SpanFill_IsValid(new Vector2(lx - 1, cursorpos.Y), currentTile, layer))
            {
                _bucketTilesToSet.Add(new Vector2(lx - 1, cursorpos.Y));
                lx--;
            }

            var rx = cursorpos.X;
            while (SpanFill_IsValid(new Vector2(rx, cursorpos.Y), currentTile, layer))
            {
                _bucketTilesToSet.Add(new Vector2(rx, cursorpos.Y));
                rx++;
            }

            SpanFill_Scan(lx, rx - 1, cursorpos.Y + 1, currentTile, layer);
            SpanFill_Scan(lx, rx - 1, cursorpos.Y - 1, currentTile, layer);
        }

        return _bucketTilesToSet;
    }

    private bool SpanFill_IsValid(Vector2 tilePosToCheck, TileMapTile currentTile, TileMapLayerGrid layerToPlaceIn)
    {
        TileMapTile tileOneD = layerToPlaceIn.GetTileAt(tilePosToCheck);
        Vector2 sizeInTiles = layerToPlaceIn.SizeInTiles;

        if (_bucketTilesToSet.Contains(new Vector2(tilePosToCheck.X, tilePosToCheck.Y))) return false;
        else return tilePosToCheck.X >= 0 && tilePosToCheck.X < sizeInTiles.X && tilePosToCheck.Y >= 0 && tilePosToCheck.Y < sizeInTiles.Y && currentTile == tileOneD;
    }

    private void SpanFill_Scan(float lx, float rx, float y, TileMapTile currentTile, TileMapLayerGrid layerToPlaceIn)
    {
        bool spanAdded = false;
        for (float i = lx; i <= rx; i++)
        {
            if (!SpanFill_IsValid(new Vector2(i, y), currentTile, layerToPlaceIn)) spanAdded = false;
            else if (!spanAdded)
            {
                _spanFillStack.Push((new Vector2(i, y), currentTile));
                spanAdded = true;
            }
        }
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) return;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        AssertNotNull(tileData);
        if (tileData == null) return;

        (TileTextureId, Vector2)[] placementPattern = editor.TileTextureSelector.GetSelectedTileTextures(out Vector2 center);
        (TileTextureId tIdToPlace, Vector2 _) = placementPattern[0];

        TilesetId currentTilesetId = editor.GetCurrentTilesetIndex();
        if (currentTilesetId == TilesetId.Invalid) return;

        TileMapTile tileToPlace = new TileMapTile(tIdToPlace, currentTilesetId);
        (Texture texture, Rectangle uv) = tileData.GetTileRenderData(tileToPlace);

        TileMapTile currentTileData = currentLayer.GetTileAt(cursorPos);
        if (currentTileData == tileToPlace) return;

        _bucketTilesToSet.Clear();
        SpanFill(cursorPos, currentTileData, currentLayer);

        foreach (Vector2 tilePos in _bucketTilesToSet)
        {
            currentLayer.EditorSetTileAt(tilePos, tileToPlace, out bool layerBoundsChanged);
            tileData.EditorUpdateRenderCacheForTile(currentLayer, tilePos);
            Assert(!layerBoundsChanged);
        }
    }

    public override void RenderCursor(RenderComposer c, TileEditorWindow editor, TileMapLayerGrid currentLayer, Vector2 cursorPos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) return;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        AssertNotNull(tileData);
        if (tileData == null) return;

        Vector2 cursorTileWorldSpace = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        (TileTextureId, Vector2)[] placementPattern = editor.TileTextureSelector.GetSelectedTileTextures(out Vector2 center);
        (TileTextureId tIdToPlace, Vector2 _) = placementPattern[0];

        TilesetId currentTilesetId = editor.GetCurrentTilesetIndex();
        if (currentTilesetId == TilesetId.Invalid) return;

        TileMapTile tileToPlace = new TileMapTile(tIdToPlace, currentTilesetId);
        (Texture texture, Rectangle uv) = tileData.GetTileRenderData(tileToPlace);

        TileMapTile currentTileData = currentLayer.GetTileAt(cursorPos);
        if (currentTileData == tileToPlace || tileToPlace == TileMapTile.Empty)
        {
            c.RenderSprite(cursorTileWorldSpace, tileSize, Color.PrettyGreen * 0.2f);
            c.RenderOutline(cursorTileWorldSpace, tileSize, Color.Green, 3f * editor.GetScale());
            return;
        }

        _bucketTilesToSet.Clear();
        SpanFill(cursorPos, currentTileData, currentLayer);

        // Camera culling
        CameraBase cam = c.Camera;
        Rectangle camBound = cam.GetCameraFrustum();
        foreach (Vector2 tilePos in _bucketTilesToSet)
        {
            Vector2 worldPos = currentLayer.GetWorldPosOfTile(tilePos);
            Rectangle drawRect = new Rectangle(worldPos, tileSize);
            if (!camBound.Intersects(drawRect)) continue; // Clip visible

            c.RenderSprite(drawRect, Color.White, texture, uv);
            c.RenderSprite(drawRect, Color.Blue * 0.2f);
        }

        c.RenderOutline(cursorTileWorldSpace.ToVec3(), tileSize, Color.Green, 3f * editor.GetScale());
    }
}

