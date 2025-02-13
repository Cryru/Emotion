#nullable enable

using Emotion.Common.Serialization;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapChunk
{
    public float ChunkSize { get; set; }

    // This is used to track runtime changes in the chunk for render cache updates
    [DontSerialize]
    public int ChunkVersion = 0;

    private uint[] _data;

    public TileMapChunk(float sizeInTiles)
    {
        ChunkSize = sizeInTiles;

        int sizeAsInt = (int)sizeInTiles;
        _data = new uint[sizeAsInt * sizeAsInt];
    }

    // serialization constructor
    protected TileMapChunk()
    {
        _data = null!;
    }

    public uint[] GetRawData()
    {
        return _data;
    }

    public TileMapTile GetTileAt(Vector2 location2D)
    {
        int loc1D = Grid.GetCoordinate1DFrom2D(location2D, new Vector2(ChunkSize));
        return GetTileAt(loc1D);
    }

    public TileMapTile GetTileAt(int location1D)
    {
        if (location1D == -1) return new TileMapTile();

        uint tileDataPacked = _data[location1D];
        var tileData = new TileMapTile();

        unsafe
        {
            uint* tileDataPackedPtr = &tileDataPacked;
            uint* tileDataPtr = (uint*)&tileData;
            *tileDataPtr = *tileDataPackedPtr;
        }

        return tileData;
    }

    public bool SetTileAt(Vector2 location, TileMapTile tileData)
    {
        int oneD = Grid.GetCoordinate1DFrom2D(location, new Vector2(ChunkSize));
        if (oneD == -1) return false;

        uint tileDataPacked = 0;

        unsafe
        {
            uint* tileDataPackedPtr = &tileDataPacked;
            uint* tileDataPtr = (uint*)&tileData;
            *tileDataPackedPtr = *tileDataPtr;
        }

        _data[oneD] = tileDataPacked;
        ChunkVersion++;

        return true;
    }

    public bool CheckIfEmpty()
    {
        for (int i = 0; i < _data.Length; i++)
        {
            uint tile = _data[i];
            if (tile != TileMapTile.Empty) return false;
        }
        return true;
    }


    public int CountNonEmptyTiles()
    {
        int nonEmpty = 0;
        for (int i = 0; i < _data.Length; i++)
        {
            uint tile = _data[i];
            if (tile != TileMapTile.Empty) nonEmpty++;
        }
        return nonEmpty;
    }
}