#nullable enable

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

[DontSerialize]
public class TileMapLayerRenderCache
{
    public TileMapLayer Layer;

    public bool Visible;

    private List<TileMapLayerRenderCacheChunk>? _renderThisPass;
    private Dictionary<Vector2, TileMapLayerRenderCacheChunk> _cachedChunks = new();

    public TileMapLayerRenderCache(TileMapLayer layer)
    {
        Layer = layer;
    }

    private void Reset(TileMapLayer layer)
    {
        Layer = layer;
        _cachedChunks.Clear();
    }

    private void ResetChunksMarkedToRender()
    {
        _renderThisPass?.Clear();
    }

    private void MarkChunkForRender(TileMapGrid tileMapData, TileMapLayer layer, TileMapChunk chunk, Vector2 chunkCoord)
    {
        _renderThisPass ??= new List<TileMapLayerRenderCacheChunk>(32);

        _cachedChunks.TryGetValue(chunkCoord, out TileMapLayerRenderCacheChunk? cachedChunk);
        if (cachedChunk == null)
        {
            cachedChunk = new TileMapLayerRenderCacheChunk();
            _cachedChunks.Add(chunkCoord, cachedChunk);
        }
        _renderThisPass.Add(cachedChunk);

        UpdateChunkRenderCache(cachedChunk, tileMapData, layer, chunk, chunkCoord);
    }

    private void UpdateChunkRenderCache(TileMapLayerRenderCacheChunk chunkCache, TileMapGrid tileMapData, TileMapLayer layer, TileMapChunk chunk, Vector2 chunkCoord)
    {
        // We already have the latest version of this
        if (chunkCache.CachedVersion == chunk.ChunkVersion &&
            chunkCache.CachedOpacity == layer.Opacity &&
            chunkCache.CachedOffset == layer.LayerOffset) return;

        TileMapTile[] chunkData = chunk.GetRawData();
        int quadsNeeded = chunk.GetNonEmptyCount();
        int vertsNeeded = quadsNeeded * 4;

        if (vertsNeeded > chunkCache.VerticesData.Length)
            chunkCache.VerticesData = new VertexData[vertsNeeded];
        chunkCache.QuadsUsed = quadsNeeded;

        if (quadsNeeded > chunkCache.Textures.Length)
            chunkCache.Textures = new Texture[quadsNeeded];

        int layerIdx = tileMapData.GetLayerOrderIndex(layer);
        Texture[] textureCache = chunkCache.Textures;
        Span<VertexData> vertexData = chunkCache.VerticesData;

        Vector2 chunkOffset = chunkCoord * layer.ChunkSize * layer.TileSize + layer.LayerOffset;
        Vector2 tileSize = layer.TileSize;
        Vector2 tileSizeHalf = layer.TileSize / 2f;

        int currentQuadWrite = 0;
        for (int i = 0; i < chunkData.Length; i++)
        {
            ref TileMapTile tileData = ref chunkData[i];
            if (tileData == TileMapTile.Empty) continue;

            Vector2 tileIdx2d = GridHelpers.GetCoordinate2DFrom1D(i, layer.ChunkSize);

            (Texture tilesetTexture, Rectangle tiUV) = tileMapData.GetTileRenderData(tileData);
            textureCache[currentQuadWrite] = tilesetTexture;

            // Calculate dimensions of the tile.
            Vector2 position = tileIdx2d * tileSize - tileSizeHalf;
            position += chunkOffset;
            var v3 = new Vector3(position, layerIdx);
            var tileTint = new Color(255, 255, 255, (int)(layer.Opacity * 255));

            // Write vertices
            bool flipX = false;
            bool flipY = false;
            VertexData.SpriteToVertexData(vertexData.Slice(currentQuadWrite * 4), v3, tileSize, tileTint, tilesetTexture, tiUV, flipX, flipY);

            currentQuadWrite++;
        }

        chunkCache.CachedVersion = chunk.ChunkVersion;
        chunkCache.CachedOffset = layer.LayerOffset;
        chunkCache.CachedOpacity = layer.Opacity;
    }

    public void Render(Renderer c, Texture[]? tilesetTextures)
    {
        if (!Visible) return;
        if (_renderThisPass == null) return;
        AssertNotNull(_renderThisPass);

        for (int i = 0; i < _renderThisPass.Count; i++)
        {
            TileMapLayerRenderCacheChunk chunk = _renderThisPass[i];
            Span<VertexData> chunkVertices = chunk.VerticesData;
            Texture[] chunkTextures = chunk.Textures;

            // todo: batch request memory if textures are the same
            for (int q = 0; q < chunk.QuadsUsed; q++)
            {
                Texture texture = chunkTextures[q];
                Span<VertexData> vertices = c.RenderStream.GetStreamMemory(4, BatchMode.Quad, texture);
                chunkVertices.Slice(q * 4, 4).CopyTo(vertices);
            }
        }
    }

    public static TileMapLayerRenderCache UpdateRenderCache(TileMapGrid tileMapData, TileMapLayer layer, Rectangle cacheArea, TileMapLayerRenderCache? cache = null)
    {
        cache ??= new TileMapLayerRenderCache(layer);
        cache.Visible = layer.Visible;
        cache.ResetChunksMarkedToRender();
        if (!layer.Visible) return cache;
        if (cache.Layer != layer) cache.Reset(layer);

        Vector2 chunkWorldSize = layer.ChunkSize * layer.TileSize;

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
                TileMapChunk? chunk = layer.GetChunk(chunkCoord);
                if (chunk == null) continue;
                cache.MarkChunkForRender(tileMapData, layer, chunk, chunkCoord);
            }
        }

        return cache;
    }

    private class TileMapLayerRenderCacheChunk
    {
        public int CachedVersion = -1;
        public Vector2 CachedOffset = new Vector2(float.NaN);
        public float CachedOpacity = float.NaN;

        public int QuadsUsed = 0;
        public VertexData[] VerticesData = Array.Empty<VertexData>();
        public Texture[] Textures = Array.Empty<Texture>();
    }
}


