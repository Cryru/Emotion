#nullable enable

using Emotion.Graphics.Data;
using Emotion.IO;

namespace Emotion.WIPUpdates.One.TileMap;

public class GameMapTileData
{
    public List<TileMapLayer> Layers = new();
    public List<TileMapTileset> Tilesets = new();

    private Texture[]? _tilesetTexturesLoaded;

    public IEnumerator InitRuntimeDataRoutine()
    {
        _tilesetTexturesLoaded = new Graphics.Objects.Texture[Tilesets.Count];

        // Cause loading of all tileset assets and add self as referencing
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            ts.Texture.GetAssetHandle(this);
        }

        // Wait to load
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            IO.AssetHandle<IO.TextureAsset> handle = ts.Texture.GetAssetHandle();
            yield return handle;

            // todo: hot reload
            if (handle.AssetLoaded && handle.Asset != null)
            {
                Texture texture = handle.Asset.Texture;
                if (ts.BilinearFilterTexture)
                    texture.Smooth = true;

                _tilesetTexturesLoaded[i] = texture;
            }
            else
            {
                _tilesetTexturesLoaded[i] = Texture.EmptyWhiteTexture;
            }
        }
    }

    public void UnloadRuntimeData()
    {
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            AssetHandle<TextureAsset> handle = ts.Texture.GetAssetHandle();
            Engine.AssetLoader.RemoveReferenceFromAssetHandle(handle, this, true);
        }
    }

    #region Render Cache

    public int GetLayerOrderIndex(TileMapLayer layer)
    {
        int idx = Layers.IndexOf(layer);
        return idx == -1 ? 0 : idx;
    }

    public (Texture texture, Rectangle uv) GetTileRenderData(TileMapTile tile)
    {
        if (_tilesetTexturesLoaded == null) return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        TileMapTileset tileset = Tilesets[tile.TilesetId];
        Texture tilesetTexture = _tilesetTexturesLoaded[tile.TilesetId];
        if (tilesetTexture == null) return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        Vector2 tilesetSizeInTiles = tileset.GetTilesetSizeInTiles();
        int widthInTiles = (int)tilesetSizeInTiles.X;
        Vector2 tilesetTileSize = tileset.TileSize;

        int tId = tile.TextureId - 1;

        // Get tile image properties.
        int tiColumn = tId % widthInTiles;
        var tiRow = (int)(tId / (float)widthInTiles);
        var tiRect = new Rectangle(tilesetTileSize.X * tiColumn, tilesetTileSize.Y * tiRow, tilesetTileSize);

        // Add margins and spacing.
        tiRect.X += tileset.Margin.X;
        tiRect.Y += tileset.Margin.Y;
        tiRect.X += tileset.Spacing.X * tiColumn;
        tiRect.Y += tileset.Spacing.Y * tiRow;

        return (tilesetTexture, tiRect);
    }

    public void UpdateRenderCache(int layerIdx = -1)
    {
        //bool recalcAll = layerIdx < 0;
        //if (_renderCache == null || recalcAll)
        //{
        //    _renderCache = new Dictionary<Vector2, TileMapChunkRenderCache>[Layers.Count];
        //    for (int i = 0; i < Layers.Count; i++)
        //    {
        //        _renderCache[i] = new Dictionary<Vector2, TileMapChunkRenderCache>();
        //    }
        //}

        //// All layers
        //if (layerIdx < 0)
        //{
        //    for (var l = 0; l < Layers.Count; l++)
        //    {
        //        TileMapLayer layer = Layers[l];
        //        UpdateRenderCacheForLayer(layer, l);
        //    }
        //    return;
        //}

        //// Specific layer
        //if (layerIdx > Layers.Count - 1) return;
        //UpdateRenderCacheForLayer(Layers[layerIdx], layerIdx);
    }

    private void UpdateRenderCacheForLayer(TileMapLayer layer, int layerIdx)
    {
        AssertNotNull(_renderCache);

        //Dictionary<Vector2, TileMapChunkRenderCache> currentCache = _renderCache[layerIdx];

        
    }

    //private (VertexData[], Texture[], Vector2) CalculateRenderCacheForLayer(int layerIdx)
    //{
    //    TileMapLayer layer = Layers[layerIdx];
    //    Vector2 sizeInTiles = layer.SizeInTiles;

    //    int tileColumns = (int)sizeInTiles.X;
    //    int tileRows = (int)sizeInTiles.Y;
    //    int totalTileSize = tileRows * tileColumns;

    //    var layerVertexCache = new VertexData[totalTileSize * 4];
    //    var layerTextureCache = new Texture[totalTileSize];

    //    int totalTiles = tileRows * tileColumns;
    //    for (int tileIdx = 0; tileIdx < totalTiles; tileIdx++)
    //    {
    //        CalculateRenderCacheForTile(layerIdx, tileIdx, layerVertexCache, layerTextureCache);
    //    }

    //    return (layerVertexCache, layerTextureCache, sizeInTiles);
    //}

    private void CalculateRenderCacheForTile(int layerIdx, int tileIdx, Span<VertexData> layerCache, Texture[] textureCache)
    {
        //TileMapLayerGrid layer = Layers[layerIdx];
        //Vector2 tileSize = layer.TileSize;
        //Vector2 tileSizeHalf = tileSize / 2f;
        //Vector2 tileIdx2D = layer.GetCoordinate2DFrom1D(tileIdx);
        //tileIdx2D -= layer.RenderOffsetInTiles;

        //TileMapTile tileData = layer.GetTileAt(tileIdx);
        //Span<VertexData> tileRenderData = layerCache.Slice(tileIdx * 4, 4);

        //// If empty skip it
        //// todo: maybe we dont wanna cache these at all to save on vertices
        //if (tileData.TextureId == 0)
        //{
        //    for (var i = 0; i < tileRenderData.Length; i++)
        //    {
        //        tileRenderData[i].Color = 0;
        //    }

        //    return;
        //}

        //(Texture tilesetTexture, Rectangle tiUV) = GetTileRenderData(tileData);
        //textureCache[tileIdx] = tilesetTexture;

        //// Calculate dimensions of the tile.
        //Vector2 position = (tileIdx2D * tileSize) - tileSizeHalf;

        //var v3 = new Vector3(position, layerIdx);
        //var c = new Color(255, 255, 255, (int)(layer.Opacity * 255));

        //// Write to tilemap mesh.
        //bool flipX = false;
        //bool flipY = false;
        //VertexData.SpriteToVertexData(tileRenderData, v3, tileSize, c, tilesetTexture, tiUV, flipX, flipY);
    }

    #endregion

    #region Editor

    //public void EditorUpdateRenderCacheForTile(TileMapLayer layer, Vector2 tileLocation)
    //{
    //    int layerIdx = Layers.IndexOf(layer);
    //    if (layerIdx == -1) return;

    //    AssertNotNull(_cachedTileRenderData);
    //    AssertNotNull(_cachedTileTextures);

    //    int tileIdx = layer.GetCoordinate1DFrom2D(tileLocation);

    //    VertexData[] cacheForThisLayer = _cachedTileRenderData[layerIdx];
    //    Texture[] textureCacheForThisLayer = _cachedTileTextures[layerIdx];
    //    CalculateRenderCacheForTile(layerIdx, tileIdx, cacheForThisLayer, textureCacheForThisLayer);
    //}

    public void EditorUpdateRenderCacheForLayer(TileMapLayerGrid layer)
    {
        //int layerIdx = Layers.IndexOf(layer);
        //if (layerIdx == -1) return;

        //AssertNotNull(_cachedTileRenderData);
        //AssertNotNull(_cachedTileTextures);
        //AssertNotNull(_cachedSizeInTiles);

        //(VertexData[] vertexData, Texture[] textureData, Vector2 sizeInTiles) = CalculateRenderCacheForLayer(layerIdx);
        //_cachedTileRenderData[layerIdx] = vertexData;
        //_cachedTileTextures[layerIdx] = textureData;
        //_cachedSizeInTiles[layerIdx] = sizeInTiles;
    }

    #endregion

    #region Render

    public void RenderLayer(RenderComposer composer, int layerIdx, Rectangle clipVal)
    {
        //if (_cachedTileRenderData == null || _cachedTileTextures == null || _cachedSizeInTiles == null) return;
        //VertexData[] renderCache = _cachedTileRenderData[layerIdx];
        //Texture[] textureCache = _cachedTileTextures[layerIdx];
        //Vector2 sizeInTiles = _cachedSizeInTiles[layerIdx];

        //var layer = Layers[layerIdx];
        //Vector2 tileSize = layer.TileSize;

        //// Apply render offset
        //Vector2 offset = layer.LayerOffset;
        //clipVal.Position += offset * tileSize;
        //clipVal.Size += tileSize;

        //var yStart = (int)Maths.Clamp(MathF.Floor(clipVal.Y / tileSize.Y), 0, sizeInTiles.Y);
        //var yEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Bottom / tileSize.Y), 0, sizeInTiles.Y);
        //var xStart = (int)Maths.Clamp(MathF.Floor(clipVal.X / tileSize.X), 0, sizeInTiles.X);
        //var xEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Right / tileSize.X), 0, sizeInTiles.X);

        //for (int y = yStart; y < yEnd; y++)
        //{
        //    var yIdx = (int)(y * sizeInTiles.X);
        //    for (int x = xStart; x < xEnd; x++)
        //    {
        //        int tileIdx = yIdx + x;
        //        int tileVertexIdx = tileIdx * 4;
        //        if (renderCache[tileVertexIdx].Color == 0) continue;

        //        Span<VertexData> vertices = composer.RenderStream.GetStreamMemory(4, BatchMode.Quad, textureCache[tileIdx]);
        //        for (var i = 0; i < vertices.Length; i++)
        //        {
        //            vertices[i] = renderCache[tileVertexIdx + i];
        //        }
        //    }
        //}
    }

    private TileMapLayerRenderCache[]? _renderCache;

    public void Render(RenderComposer c, Rectangle clipArea)
    {
        if (_renderCache == null)
        {
            _renderCache = new TileMapLayerRenderCache[Layers.Count];
        }
        else if (Layers.Count != _renderCache.Length)
        {
            Array.Resize(ref _renderCache, Layers.Count);
        }

        // Update the render cache, load chunks, etc.
        for (int i = 0; i < Layers.Count; i++)
        {
            TileMapLayer layer = Layers[i];
            _renderCache[i] = TileMapLayerRenderCache.UpdateRenderCache(this, layer, clipArea, _renderCache[i]);
        }

        // Render caches
        for (int i = 0; i < _renderCache.Length; i++)
        {
            TileMapLayerRenderCache cache = _renderCache[i];
            AssertNotNull(cache);
            cache.Render(c, _tilesetTexturesLoaded);
        }
    }

    #endregion
}
