#nullable enable

using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class TileMapLayer : ChunkedGrid<TileMapTile, TileMapChunk>, IGridWorldSpaceTiles
{
    /// <summary>
    /// The name of the layer, to be used by the editor or code.
    /// </summary>
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
    /// The size of tiles in world space.
    /// </summary>
    public Vector2 TileSize { get; set; }

    /// <summary>
    /// The opacity of layer tiles.
    /// </summary>
    public float Opacity { get; set; } = 1f;

    public override string ToString()
    {
        return Name;
    }

    public TileMapLayer() : base(16)
    {

    }

    public TileMapLayer(float chunkSize) : base(chunkSize)
    {

    }

    public IEnumerable<Rectangle> ForEachLoadedChunkBound()
    {
        foreach (KeyValuePair<Vector2, TileMapChunk> chunkData in _chunks)
        {
            Rectangle chunkBounds = new Rectangle(chunkData.Key * ChunkSize * TileSize - TileSize / 2f, ChunkSize * TileSize);
            chunkBounds.Position += LayerOffset;
            yield return chunkBounds;
        }
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
        Vector2 worldPos = tileCoord2d * TileSize - TileSize / 2f;
        return worldPos + LayerOffset;
    }
}
