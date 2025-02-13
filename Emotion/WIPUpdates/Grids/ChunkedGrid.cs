#nullable enable

using Emotion.Utility;

namespace Emotion.WIPUpdates.Grids;

public interface IGridChunk<T> : IGrid<T> where T : struct
{
    public abstract static IGridChunk<T> CreateChunk(float chunkSize);

    public bool IsEmpty();

    public int GetNonEmptyCount();
}

public class ChunkedGrid<T, ChunkT> : IGrid<T> where ChunkT : IGridChunk<T> where T : struct
{
    public Vector2 ChunkSize { get; private set; }

    protected Dictionary<Vector2, ChunkT> _chunks { get; set; } = new();

    public ChunkedGrid(float chunkSize)
    {
        ChunkSize = new Vector2(chunkSize);
    }

    public Vector2 GetChunkCoordinateOfValueCoordinate(Vector2 tile)
    {
        return (tile / ChunkSize).Floor();
    }

    public ChunkT? GetChunkAt(Vector2 position, out Vector2 relativeCoord)
    {
        Vector2 chunkCoord = GetChunkCoordinateOfValueCoordinate(position);
        relativeCoord = position - chunkCoord * ChunkSize;
        _chunks.TryGetValue(chunkCoord, out ChunkT? chunk);
        return chunk;
    }

    public ChunkT? GetChunk(Vector2 chunkCoord)
    {
        _chunks.TryGetValue(chunkCoord, out ChunkT? chunk);
        return chunk;
    }


    public T[] GetRawData()
    {
        throw new Exception("Cannot get the raw data of a chunked grid.");
    }

    public void SetRawData(T[] data, Vector2 gridSize)
    {
        throw new Exception("Cannot set the raw data of a chunked grid.");
    }

    public Vector2 GetOrigin()
    {
        Vector2 smallestChunkCoord = Vector2.Zero;
        bool first = true;
        foreach (var chunkPair in _chunks)
        {
            Vector2 coord = chunkPair.Key;
            if (coord.X < smallestChunkCoord.X || first) smallestChunkCoord.X = coord.X;
            if (coord.Y < smallestChunkCoord.Y || first) smallestChunkCoord.X = coord.X;

            first = false;
        }

        return smallestChunkCoord;
    }

    public Vector2 GetSize()
    {
        Vector2 largestChunkCoord = Vector2.Zero;
        bool first = true;
        foreach (var chunkPair in _chunks)
        {
            Vector2 coord = chunkPair.Key;
            if (coord.X > largestChunkCoord.X || first) largestChunkCoord.X = coord.X;
            if (coord.Y > largestChunkCoord.Y || first) largestChunkCoord.X = coord.X;

            first = false;
        }

        return largestChunkCoord;
    }

    public bool IsValidPosition(Vector2 position)
    {
        ChunkT? chunk = GetChunkAt(position, out Vector2 _);
        return chunk != null;
    }

    public void SetAt(Vector2 position, T value)
    {
        ChunkT? chunk = GetChunkAt(position, out Vector2 relativeCoord);
        if (chunk == null) return;

        chunk.SetAt(relativeCoord, value);
    }

    public T GetAt(Vector2 position)
    {
        ChunkT? chunk = GetChunkAt(position, out Vector2 relativeCoord);
        if (chunk == null) return default;

        return chunk.GetAt(relativeCoord);
    }

    public bool ExpandingSetAt(Vector2 position, T value)
    {
        Assert(position == position.Floor());

        Vector2 chunkCoord = GetChunkCoordinateOfValueCoordinate(position);

        T defVal = default;
        bool isDelete = Helpers.AreObjectsEqual(defVal, value);

        ChunkT? chunk = GetChunkAt(position, out Vector2 relativeLocation);
        if (chunk != null)
        {
            // Setting position in a chunk - easy peasy.
            chunk.SetAt(relativeLocation, value);
            if (!isDelete) return false;

            // Compact the grid if the chunk is now empty.
            if (chunk.IsEmpty())
            {
                _chunks.Remove(chunkCoord);
                return true;
            }

            return false;
        }

        // Trying to delete nothing
        if (isDelete) return false;

        ChunkT newChunk = (ChunkT) ChunkT.CreateChunk(ChunkSize.X);
        _chunks.Add(chunkCoord, newChunk);

        newChunk.SetAt(relativeLocation, value);
        return true;
    }
}
