#nullable enable

using Emotion.Graphics.Data;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapLayer
{
    public const float CHUNK_SIZE = 16;
    public static Vector2 CHUNK_SIZE2 = new Vector2(CHUNK_SIZE);

    public string Name { get; set; } = "Untitled";

    /// <summary>
    /// Whether the layer is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// The offset of the layer's chunks from the map origin.
    /// </summary>
    public Vector2 LayerOffset { get; set; }

    /// <summary>
    /// The size of tiles in the grid.
    /// </summary>
    public Vector2 TileSize { get; set; }

    /// <summary>
    /// The opacity of layer tiles.
    /// </summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>
    /// Layer chunks containing tile data.
    /// </summary>
    private Dictionary<Vector2, TileMapChunk> Chunks { get; set; } = new();

    public override string ToString()
    {
        return Name;
    }

    #region Chunks

    public Vector2 GetChunkCoordinateOfTileCoordinate(Vector2 tile)
    {
        return (tile / CHUNK_SIZE2).Floor();
    }

    public TileMapChunk? GetChunkAtTile(Vector2 tile, out Vector2 relativeCoord)
    {
        Vector2 chunkCoord = GetChunkCoordinateOfTileCoordinate(tile);
        relativeCoord = tile - chunkCoord * CHUNK_SIZE2;
        Chunks.TryGetValue(chunkCoord, out TileMapChunk? chunk);
        return chunk;
    }

    public TileMapChunk? GetChunk(Vector2 chunkCoord)
    {
        Chunks.TryGetValue(chunkCoord, out TileMapChunk? chunk);
        return chunk;
    }

    public IEnumerable<Rectangle> ForEachLoadedChunkBound()
    {
        foreach (KeyValuePair<Vector2, TileMapChunk> chunkData in Chunks)
        {
            yield return new Primitives.Rectangle( chunkData.Key * CHUNK_SIZE2 * TileSize - TileSize / 2f, CHUNK_SIZE2 * TileSize);
        }
    }

    #endregion

    #region Set/Get Tiles

    public TileMapTile GetTileAt(int x, int y)
    {
        return GetTileAt(new Vector2(x, y));
    }

    public TileMapTile GetTileAt(Vector2 location)
    {
        TileMapChunk? chunk = GetChunkAtTile(location, out Vector2 relativeCoord);
        if (chunk == null) return TileMapTile.Empty;

        return chunk.GetTileAt(relativeCoord);
    }

    public bool IsTileInBounds(Vector2 location)
    {
        return GetChunkAtTile(location, out Vector2 _) != null;
    }

    public bool SetTileAt(Vector2 location, TileTextureId tId, TilesetId tsId)
    {
        return SetTileAt(location, new TileMapTile() { TilesetId = tsId, TextureId = tId });
    }

    public bool SetTileAt(Vector2 location, TileMapTile tileData)
    {
        TileMapChunk? chunk = GetChunkAtTile(location, out Vector2 relativeCoord);
        if (chunk == null) return false;

        return chunk.SetTileAt(relativeCoord, tileData);
    }

    #endregion

    #region Editor

    public bool EditorSetTileAt(Vector2 location, TileTextureId tId, TilesetId tsId)
    {
        return EditorSetTileAt(location, new TileMapTile(tId, tsId));
    }

    public bool EditorSetTileAt(Vector2 location, TileMapTile tileData)
    {
        Assert(location == location.Floor());

        Vector2 chunkCoord = GetChunkCoordinateOfTileCoordinate(location);
        bool isDelete = tileData.Equals(TileMapTile.Empty);

        TileMapChunk? chunk = GetChunkAtTile(location, out Vector2 relativeLocation);
        if (chunk != null)
        {
            bool success = chunk.SetTileAt(relativeLocation, tileData);
            if (!isDelete) return success;
            if (!success) return false;

            // Compact the grid if the chunk is now empty.
            if (chunk.CheckIfEmpty())
                Chunks.Remove(chunkCoord);

            return true;
        }

        // Trying to delete nothing
        if (isDelete) return true;

        TileMapChunk newChunk = new TileMapChunk(CHUNK_SIZE);
        Chunks.Add(chunkCoord, newChunk);

        return newChunk.SetTileAt(relativeLocation, tileData);
    }

    #endregion

    public bool IsTilePositionValid(Vector2 tileCoord2d)
    {
        TileMapChunk? chunk = GetChunkAtTile(tileCoord2d, out Vector2 _);
        return chunk != null;
    }

    public Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        //location -= TileSize / 2f;
        location -= LayerOffset;

        var left = MathF.Round(location.X / TileSize.X);
        var top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        Vector2 worldPos = (tileCoord2d * TileSize) - TileSize / 2f;
        return worldPos + LayerOffset;
    }
}
