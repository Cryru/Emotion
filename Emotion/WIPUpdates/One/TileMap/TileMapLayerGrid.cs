using Emotion.Game.World.Grid;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapLayerGrid : PackedNumericMapGrid<uint>
{
    public Vector2 RenderOffset;

    public TileMapLayerGrid()
    {
    }

    #region Set/Get

    public TileMapTile GetTileAt(int x, int y)
    {
        return GetTileAt(new Vector2(x, y));
    }

    public TileMapTile GetTileAt(Vector2 location)
    {
        int oneD = GetCoordinate1DFrom2D(location);
        if (oneD == -1) return new TileMapTile();

        uint tileDataPacked = _data[oneD];
        var tileData = new TileMapTile();

        unsafe
        {
            uint* tileDataPackedPtr = &tileDataPacked;
            uint* tileDataPtr = (uint*)&tileData;
            *tileDataPtr = *tileDataPackedPtr;
        }

        return tileData;
    }

    public bool SetTileAt(Vector2 location, TileTextureId tId, TilesetId tsId)
    {
        return SetTileAt(location, new TileMapTile() { TilesetId = tsId, TextureId = tId });
    }

    public bool SetTileAt(Vector2 location, TileMapTile tileData)
    {
        int oneD = GetCoordinate1DFrom2D(location);
        if (oneD == -1) return false;

        uint tileDataPacked = 0;

        unsafe
        {
            uint* tileDataPackedPtr = &tileDataPacked;
            uint* tileDataPtr = (uint*)&tileData;
            *tileDataPackedPtr = *tileDataPtr;
        }

        _data[oneD] = tileDataPacked;

        return true;
    }

    #endregion

    #region Editor

    public bool EditorSetTileAt(Vector2 location, TileTextureId tId, TilesetId tsId)
    {
        return EditorSetTileAt(location, new TileMapTile(tId, tsId));
    }

    public bool EditorSetTileAt(Vector2 location, TileMapTile tileData)
    {
        bool isDelete = tileData.Equals(TileMapTile.Empty);

        int oneD = GetCoordinate1DFrom2D(location);
        if (oneD != -1)
        {
            bool success = SetTileAt(location, tileData);
            if (!isDelete) return success;

            // We are deleting - compact the grid.
            Vector2 compacted = Compact(TileMapTile.EmptyUint);
            RenderOffset += compacted * TileSize;

            return true;
        }

        // Deleting outside bounds - no need to do anything
        if (isDelete) return true;

        // Resize the grid to fit the tile.
        float setX = location.X;
        float newWidth = SizeInTiles.X;
        float offsetX = 0;
        if (setX < 0)
        {
            newWidth += -setX;
            offsetX = -setX;
        }
        else if (setX >= SizeInTiles.X)
        {
            float diff = setX - (SizeInTiles.X - 1);
            newWidth += diff;
        }

        float setY = location.Y;
        float newHeight = SizeInTiles.Y;
        float offsetY = 0;
        if (setY < 0)
        {
            newHeight += -setY;
            offsetY = -setY;
        }
        else if (setY >= SizeInTiles.Y)
        {
            float diff = setY - (SizeInTiles.Y - 1);
            newHeight += diff;
        }

        Resize((int) newWidth, (int) newHeight);

        Offset((int) offsetX, (int) offsetY, false);
        RenderOffset += new Vector2(offsetX, offsetY) * TileSize;

        return SetTileAt(location + new Vector2(offsetX, offsetY), tileData);
    }

    #endregion

    public Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        //location -= TileSize / 2f;

        var left = MathF.Round(location.X / TileSize.X);
        var top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public bool IsPositionInMap(Vector2 tileCoord2d)
    {
        return IsCoordinate2DValid(tileCoord2d);
    }

    public Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        return tileCoord2d * TileSize - TileSize / 2f;
    }
}
