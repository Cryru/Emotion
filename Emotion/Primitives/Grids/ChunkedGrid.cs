#nullable enable

namespace Emotion.Primitives.Grids;

public interface IChunkedGrid
{
    public Vector2 GetChunkCoordinateOfValueCoordinate(Vector2 tile);
}

public class ChunkedGrid<T, ChunkT> : IChunkedGrid, IGrid<T>
    where ChunkT : IGridChunk<T>, new()
    where T : struct, IEquatable<T>
{
    public Vector2 ChunkSize { get; private set; }

    protected Dictionary<Vector2, ChunkT> _chunks { get; set; } = new();

    public ChunkedGrid(float chunkSize)
    {
        ChunkSize = new Vector2(chunkSize);
    }

    // serialization
    protected ChunkedGrid()
    {

    }

    public Vector2 GetChunkCoordinateOfValueCoordinate(Vector2 tile)
    {
        return (tile / ChunkSize).Floor();
    }

    public ChunkT? GetChunkAt(Vector2 tileCoordinate, out Vector2 chunkCoord, out Vector2 tileRelativeCoord)
    {
        chunkCoord = GetChunkCoordinateOfValueCoordinate(tileCoordinate);
        tileRelativeCoord = tileCoordinate - chunkCoord * ChunkSize;
        _chunks.TryGetValue(chunkCoord, out ChunkT? chunk);
        return chunk;
    }

    public ChunkT? GetChunkAt(Vector2 tileCoordinate, out Vector2 tileRelativeCoord)
    {
        return GetChunkAt(tileCoordinate, out Vector2 _, out tileRelativeCoord);
    }

    public ChunkT? GetChunk(Vector2 chunkCoord)
    {
        _chunks.TryGetValue(chunkCoord, out ChunkT? chunk);
        return chunk;
    }

    public void CreateEmptyChunksInArea(Vector2 origin, Vector2 size)
    {
        Rectangle areaToSpawn = new Rectangle(origin, size);
        areaToSpawn.SnapToGrid(ChunkSize);

        areaToSpawn.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        min /= ChunkSize;
        max /= ChunkSize;

        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 chunkCoord = new Vector2(x, y);
                ChunkT? existingChunk = GetChunk(chunkCoord);
                if (existingChunk == null)
                {
                    ChunkT newChunk = InitializeNewChunk();
                    _chunks.Add(chunkCoord, newChunk);
                    OnChunkCreated(chunkCoord, newChunk);
                    _chunkBoundsCacheValid = false;
                }
            }
        }
    }

    #region ChunkHelpers

    protected virtual ChunkT InitializeNewChunk()
    {
        ChunkT newChunk = new ChunkT();
        T[] newChunkData = new T[(int)(ChunkSize.X * ChunkSize.Y)];
        newChunk.SetRawData(newChunkData);

        return newChunk;
    }

    protected virtual void SetAtForChunk(ChunkT chunk, Vector2 position, T value)
    {
        T[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom2D(position, ChunkSize);
        data[idx] = value;
    }

    protected virtual T GetAtForChunk(ChunkT chunk, Vector2 position)
    {
        T[] data = chunk.GetRawData();
        int idx = GridHelpers.GetCoordinate1DFrom2D(position, ChunkSize);
        return data[idx];
    }

    #endregion

    public T[] GetRawData()
    {
        throw new Exception("Cannot get the raw data of a chunked grid.");
    }

    public void SetRawData(T[] data, Vector2 gridSize)
    {
        throw new Exception("Cannot set the raw data of a chunked grid.");
    }

    private Vector2 _smallestChunkCoordCache;
    private Vector2 _largestChunkCoordCache;
    protected bool _chunkBoundsCacheValid = false;

    private void CacheChunkBounds()
    {
        if (_chunkBoundsCacheValid) return;

        Vector2 smallestChunkCoord = Vector2.Zero;
        Vector2 largestChunkCoord = Vector2.Zero;
        bool first = true;
        foreach (var chunkPair in _chunks)
        {
            Vector2 coord = chunkPair.Key;
            if (coord.X > largestChunkCoord.X || first) largestChunkCoord.X = coord.X;
            if (coord.Y > largestChunkCoord.Y || first) largestChunkCoord.Y = coord.Y;
            if (coord.X < smallestChunkCoord.X || first) smallestChunkCoord.X = coord.X;
            if (coord.Y < smallestChunkCoord.Y || first) smallestChunkCoord.Y = coord.Y;

            first = false;
        }

        if (_chunks.Count != 0) largestChunkCoord += Vector2.One;

        _smallestChunkCoordCache = smallestChunkCoord;
        _largestChunkCoordCache = largestChunkCoord;
        _chunkBoundsCacheValid = true;
    }

    public Vector2 GetOrigin()
    {
        CacheChunkBounds();

        Vector2 smallestChunkCoord = _smallestChunkCoordCache;
        return smallestChunkCoord * ChunkSize;
    }

    /// <summary>
    /// Get the size of the grid in value coordinates.
    /// </summary>
    public Vector2 GetSize()
    {
        CacheChunkBounds();

        Vector2 smallestChunkCoord = _smallestChunkCoordCache;
        Vector2 largestChunkCoord = _largestChunkCoordCache;
        return (largestChunkCoord - smallestChunkCoord) * ChunkSize;
    }

    public bool IsValidPosition(Vector2 position)
    {
        var origin = GetOrigin();
        var size = GetSize();
        var mapRect = new Rectangle(origin, size);

        // Inclusive check on the lower end
        mapRect.Position -= Vector2.One;
        mapRect.Size += Vector2.One;

        return mapRect.Contains(position);
    }

    public void SetAt(Vector2 position, T value)
    {
        ChunkT? chunk = GetChunkAt(position, out Vector2 chunkCoord, out Vector2 relativeCoord);
        if (chunk == null) return;
        SetAtForChunk(chunk, relativeCoord, value);
        OnChunkChanged(chunkCoord, chunk);
    }

    public T GetAt(Vector2 position)
    {
        ChunkT? chunk = GetChunkAt(position, out Vector2 relativeCoord);
        if (chunk == null) return default;
        return GetAtForChunk(chunk, relativeCoord);
    }

    public bool ExpandingSetAt(Vector2 position, T value)
    {
        Assert(position == position.Floor());

        Vector2 chunkCoord = GetChunkCoordinateOfValueCoordinate(position);

        T defVal = default;
        bool isDelete = defVal.Equals(value);

        ChunkT? chunk = GetChunkAt(position, out Vector2 relativeLocation);
        if (chunk != null)
        {
            // Setting position in a chunk - easy peasy.
            SetAtForChunk(chunk, relativeLocation, value);
            OnChunkChanged(chunkCoord, chunk);

            if (!isDelete) return false;

            // Compact the grid if the chunk is now empty.
            if (chunk.IsEmpty())
            {
                _chunks.Remove(chunkCoord);
                OnChunkRemoved(chunkCoord, chunk);
                return true;
            }

            return false;
        }

        // Trying to delete nothing
        if (isDelete) return false;

        // Initialize new chunk
        ChunkT newChunk = InitializeNewChunk();
        _chunks.Add(chunkCoord, newChunk);
        OnChunkCreated(chunkCoord, newChunk);
        _chunkBoundsCacheValid = false;

        SetAtForChunk(newChunk, relativeLocation, value);
        OnChunkChanged(chunkCoord, newChunk);
        return true;
    }

    #region Events

    protected virtual void OnChunkCreated(Vector2 chunkCoord, ChunkT newChunk)
    {
        // nop
    }

    protected virtual void OnChunkRemoved(Vector2 chunkCoord, ChunkT newChunk)
    {
        // nop
    }

    protected virtual void OnChunkChanged(Vector2 chunkCoord, ChunkT newChunk)
    {
        if (newChunk is VersionedGridChunk<T> versionedChunk)
            versionedChunk.ChunkVersion++;
    }

    #endregion
}

public interface IGridChunk<T> where T : struct
{
    public bool IsEmpty();

    public int GetNonEmptyCount();

    public T[] GetRawData();

    public void SetRawData(T[] data);
}

public class GenericGridChunk<T> : IGridChunk<T> where T : struct, IEquatable<T>
{
    protected T[] _data = Array.Empty<T>();

    public bool IsEmpty()
    {
        T defValue = default;
        for (int i = 0; i < _data.Length; i++)
        {
            T value = _data[i];
            if (!defValue.Equals(value)) return false;
        }
        return true;
    }

    public int GetNonEmptyCount()
    {
        T defValue = default;
        int nonEmpty = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            T value = _data[i];
            if (!defValue.Equals(value)) nonEmpty++;
        }
        return nonEmpty;
    }

    public T[] GetRawData()
    {
        return _data;
    }

    public void SetRawData(T[] data)
    {
        _data = data;
    }
}

public class VersionedGridChunk<T> : GenericGridChunk<T> where T : struct, IEquatable<T>
{
    // This is used to track runtime changes in the chunk for render cache updates
    [DontSerialize]
    public int ChunkVersion = 0;
}