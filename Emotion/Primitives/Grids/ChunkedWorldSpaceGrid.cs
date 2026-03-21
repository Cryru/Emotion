#nullable enable


using Emotion.Primitives.Grids.Chunked;

namespace Emotion.Primitives.Grids;

public class ChunkedWorldSpaceGrid<T, ChunkT> : ChunkedGrid<T, ChunkT>
    where T : unmanaged
    where ChunkT : IGridChunk<T>, new()
{
    public Vector2 TileSize { get; protected set; }

    public ChunkedWorldSpaceGrid(Vector2 tileSize, float chunkSize) : base(chunkSize)
    {
        TileSize = tileSize;
    }

    public virtual Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        float left = MathF.Round(location.X / TileSize.X);
        float top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public virtual Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        Vector2 worldPos = (tileCoord2d * TileSize);
        return worldPos;
    }
}
