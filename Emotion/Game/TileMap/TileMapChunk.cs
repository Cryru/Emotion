#nullable enable

using Emotion.Common.Serialization;
using Emotion.WIPUpdates.Grids;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapChunk : VersionedGridChunk<TileMapTile>, IPackedNumberData<TileMapTile, uint>
{
}