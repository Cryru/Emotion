#nullable enable

using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Primitives.Grids;
using System.Reflection.Emit;

namespace Emotion.Game.World.TileMap;

public class TileMapLayer : ChunkedGrid<TileMapTile, TileMapChunk>, IGridWorldSpaceTiles, IMapGrid
{
    public string UniqueId { get; set; } = Guid.NewGuid().ToString("N");

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

    public IEnumerator InitRoutine(GameMap.GridFriendAdapter adapter)
    {
        yield break;
    }

    public void Done()
    {
        _renderThisPass.Clear();
        _cachedChunks.Clear();
    }

    public void Update(float dt)
    {

    }

    #region Rendering

    private class TileMapLayerRenderCacheChunk
    {
        public int CachedVersion = -1;
        public Vector2 CachedOffset = new Vector2(float.NaN);
        public float CachedOpacity = float.NaN;

        public int QuadsUsed = 0;
        public VertexData[] VerticesData = Array.Empty<VertexData>();
        public Texture[] Textures = Array.Empty<Texture>();
    }

    private List<TileMapLayerRenderCacheChunk> _renderThisPass = new();
    private Dictionary<Vector2, TileMapLayerRenderCacheChunk> _cachedChunks = new();

    private void ResetChunksMarkedToRender()
    {
        _renderThisPass?.Clear();
    }

    private void MarkChunkForRender(GameMap map, TileMapChunk chunk, Vector2 chunkCoord)
    {
        _renderThisPass ??= new List<TileMapLayerRenderCacheChunk>(32);

        _cachedChunks.TryGetValue(chunkCoord, out TileMapLayerRenderCacheChunk? cachedChunk);
        if (cachedChunk == null)
        {
            cachedChunk = new TileMapLayerRenderCacheChunk();
            _cachedChunks.Add(chunkCoord, cachedChunk);
        }
        _renderThisPass.Add(cachedChunk);

        UpdateChunkRenderCache(map, cachedChunk, chunk, chunkCoord);
    }

    private void UpdateChunkRenderCache(GameMap map, TileMapLayerRenderCacheChunk chunkCache, TileMapChunk chunk, Vector2 chunkCoord)
    {
        // We already have the latest version of this
        if (chunkCache.CachedVersion == chunk.ChunkVersion &&
            chunkCache.CachedOpacity == Opacity &&
            chunkCache.CachedOffset == LayerOffset) return;

        TileMapTile[] chunkData = chunk.GetRawData();
        int quadsNeeded = chunk.GetNonEmptyCount();
        int vertsNeeded = quadsNeeded * 4;

        if (vertsNeeded > chunkCache.VerticesData.Length)
            chunkCache.VerticesData = new VertexData[vertsNeeded];
        chunkCache.QuadsUsed = quadsNeeded;

        if (quadsNeeded > chunkCache.Textures.Length)
            chunkCache.Textures = new Texture[quadsNeeded];

        int layerIdx = map.Grids.IndexOf(this);
        Texture[] textureCache = chunkCache.Textures;
        Span<VertexData> vertexData = chunkCache.VerticesData;

        Vector2 chunkSize = ChunkSize;
        Vector2 tileSize = TileSize;
        Vector2 tileSizeHalf = TileSize / 2f;

        Vector2 chunkOffset = chunkCoord * chunkSize * tileSize + LayerOffset;

        TileMapData tileMapData = map.TileMapData;
        int currentQuadWrite = 0;
        for (int i = 0; i < chunkData.Length; i++)
        {
            ref TileMapTile tileData = ref chunkData[i];
            if (tileData == TileMapTile.Empty) continue;

            Vector2 tileIdx2d = GridHelpers.GetCoordinate2DFrom1D(i, chunkSize);

            (Texture tilesetTexture, Rectangle tiUV) = tileMapData.GetTileTexture(tileData);
            textureCache[currentQuadWrite] = tilesetTexture;

            // Calculate dimensions of the tile.
            Vector2 position = tileIdx2d * tileSize - tileSizeHalf;
            position += chunkOffset;
            var v3 = new Vector3(position, layerIdx);
            var tileTint = new Color(255, 255, 255, (int)(Opacity * 255));

            // Write vertices
            bool flipX = false;
            bool flipY = false;
            VertexData.SpriteToVertexData(vertexData.Slice(currentQuadWrite * 4), v3, tileSize, tileTint, tilesetTexture, tiUV, flipX, flipY);

            currentQuadWrite++;
        }

        chunkCache.CachedVersion = chunk.ChunkVersion;
        chunkCache.CachedOffset = LayerOffset;
        chunkCache.CachedOpacity = Opacity;
    }

    private void UpdateRenderCache(GameMap map, Rectangle cacheArea)
    {
        ResetChunksMarkedToRender();

        Vector2 chunkWorldSize = ChunkSize * TileSize;

        Rectangle cacheAreaChunkSpace = cacheArea;
        cacheAreaChunkSpace.SnapToGrid(chunkWorldSize);

        cacheAreaChunkSpace.GetMinMaxPoints(out Vector2 min, out Vector2 max);
        min /= chunkWorldSize;
        max /= chunkWorldSize;

        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 chunkCoord = new Vector2(x, y);
                TileMapChunk? chunk = GetChunk(chunkCoord);
                if (chunk == null) continue;
                MarkChunkForRender(map, chunk, chunkCoord);
            }
        }
    }

    public void Render(GameMap map, Renderer r, CameraCullingContext culling)
    {
        if (!Visible) return;

        UpdateRenderCache(map, culling.Rect2D);
        if (_renderThisPass == null) return;

        for (int i = 0; i < _renderThisPass.Count; i++)
        {
            TileMapLayerRenderCacheChunk chunk = _renderThisPass[i];
            Span<VertexData> chunkVertices = chunk.VerticesData;
            Texture[] chunkTextures = chunk.Textures;

            // todo: batch request memory if textures are the same
            for (int q = 0; q < chunk.QuadsUsed; q++)
            {
                Texture texture = chunkTextures[q];
                Span<VertexData> vertices = r.RenderStream.GetStreamMemory(4, BatchMode.Quad, texture);
                chunkVertices.Slice(q * 4, 4).CopyTo(vertices);
            }
        }
    }

    #endregion
}
