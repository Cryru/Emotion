#nullable enable

using Emotion.Common.Serialization;
using Emotion.WIPUpdates.Grids;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapChunk : GenericGridChunk<TileMapTile>, IPackedNumberData<TileMapTile, uint>
{
    // This is used to track runtime changes in the chunk for render cache updates
    [DontSerialize]
    public int ChunkVersion = 0;
}