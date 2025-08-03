#nullable enable

using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class TileMapChunk : VersionedGridChunk<TileMapTile>, IPackedNumberData<TileMapTile, uint>
{
}