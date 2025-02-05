#nullable enable

using Emotion.Game.World.Grid;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapLayerGrid : PackedNumericMapGrid<uint>
{
    public bool Visible = true;

    public Vector2 RenderOffsetInTiles;
    public float Opacity = 1f;

    public TileMapLayerGrid()
    {
    }

    public override string ToString()
    {
        return Name;
    }

    #region Set/Get

    public TileMapTile GetTileAt(int x, int y)
    {
        return GetTileAt(new Vector2(x, y));
    }

    public TileMapTile GetTileAt(Vector2 location)
    {
        int oneD = GetCoordinate1DFrom2D(location);
        return GetTileAt(oneD);
    }

    public TileMapTile GetTileAt(int location)
    {
        if (location == -1) return new TileMapTile();

        uint tileDataPacked = _data[location];
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

    public bool EditorResizeToFitTile(Vector2 location, out bool layerBoundsChanged)
    {
        Assert(location == location.Floor());

        int oneD = GetCoordinate1DFrom2D(location);
        if (oneD != -1)
        {
            layerBoundsChanged = false;
            return true;
        }

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

        Resize((int)newWidth, (int)newHeight);
        Offset((int)offsetX, (int)offsetY, false);
        RenderOffsetInTiles += new Vector2(offsetX, offsetY);
        layerBoundsChanged = true;

        return true;
    }

    public bool EditorSetTileAt(Vector2 location, TileTextureId tId, TilesetId tsId, out bool layerBoundsChanged)
    {
        return EditorSetTileAt(location, new TileMapTile(tId, tsId), out layerBoundsChanged);
    }

    public bool EditorSetTileAt(Vector2 location, TileMapTile tileData, out bool layerBoundsChanged)
    {
        Assert(location == location.Floor());

        layerBoundsChanged = false;

        bool isDelete = tileData.Equals(TileMapTile.Empty);
        int oneD = GetCoordinate1DFrom2D(location);
        if (oneD != -1)
        {
            // Changing tile in map - easy peasy
            bool success = SetTileAt(location, tileData);
            if (!isDelete) return success;

            // We deleted a tile, try compacting the grid.
            bool compacted = Compact(TileMapTile.EmptyUint, out Vector2 compactOffset);
            if (compacted)
            {
                RenderOffsetInTiles += compactOffset;
                layerBoundsChanged = true;
            }

            return true;
        }

        // Deleting outside bounds - no need to do anything
        if (isDelete) return false;

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

        Resize((int)newWidth, (int)newHeight);
        Offset((int)offsetX, (int)offsetY, false);
        RenderOffsetInTiles += new Vector2(offsetX, offsetY);
        layerBoundsChanged = true;

        Vector2 newLocation = location + new Vector2(offsetX, offsetY);
        return SetTileAt(newLocation, tileData);
    }

    #endregion

    public Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        //location -= TileSize / 2f;

        var left = MathF.Round(location.X / TileSize.X);
        var top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top) + RenderOffsetInTiles;
    }

    public bool IsPositionInMap(Vector2 tileCoord2d)
    {
        return IsCoordinate2DValid(tileCoord2d);
    }

    public Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        tileCoord2d = tileCoord2d - RenderOffsetInTiles;
        return (tileCoord2d * TileSize) - TileSize / 2f;
    }
}
