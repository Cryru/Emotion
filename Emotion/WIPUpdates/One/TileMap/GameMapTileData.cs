#nullable enable


using Emotion.Game.World2D.Tile;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.IO;
using Emotion.Utility;
using System.Runtime.CompilerServices;

namespace Emotion.WIPUpdates.One.TileMap;

public class GameMapTileData
{
    public List<TileMapLayerGrid> Layers = new();
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
                _tilesetTexturesLoaded[i] = handle.Asset.Texture;
            else
                _tilesetTexturesLoaded[i] = Texture.EmptyWhiteTexture;
        }

        BuildRenderCache();
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

    private (Texture texture, Rectangle uv) GetTileRenderData(TileMapTile tile)
    {
        if (_tilesetTexturesLoaded == null) return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        TileMapTileset tileset = Tilesets[tile.TilesetId];

        Texture tilesetTexture = _tilesetTexturesLoaded[tile.TilesetId];
        if (tilesetTexture == null) return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        Vector2 coord = Grid.GetCoordinate2DFrom1D(tile.TextureId - 1, tilesetTexture.Size);
        Rectangle uv = new Rectangle(coord * tileset.TileSize, tileset.TileSize);
        return (tilesetTexture, uv);
    }

    #region Render Cache

    /// <summary>
    /// Cached render data for tiles per layer.
    /// </summary>
    private VertexData[][]? _cachedTileRenderData;

    /// <summary>
    /// Cached render data for tiles per layer.
    /// </summary>
    private Texture[][]? _cachedTileTextures;

    private void BuildRenderCache()
    {
        if (Tilesets.Count == 0) return;

        _cachedTileRenderData = null;
        _cachedTileTextures = null;
        if (Layers.Count == 0) return;

        _cachedTileRenderData = new VertexData[Layers.Count][];
        _cachedTileTextures = new Texture[Layers.Count][];
        for (var layerIdx = 0; layerIdx < Layers.Count; layerIdx++)
        {
            (VertexData[] vertexData, Texture[] textureData) = CalculateRenderCacheForLayer(layerIdx);

            _cachedTileRenderData[layerIdx] = vertexData;
            _cachedTileTextures[layerIdx] = textureData;
        }
    }

    private (VertexData[], Texture[]) CalculateRenderCacheForLayer(int layerIdx)
    {
        TileMapLayerGrid layer = Layers[layerIdx];
        int tileColumns = (int)layer.SizeInTiles.X;
        int tileRows = (int)layer.SizeInTiles.Y;
        int totalTileSize = tileRows * tileColumns;

        var layerVertexCache = new VertexData[totalTileSize * 4];
        var layerTextureCache = new Texture[totalTileSize];

        int totalTiles = tileRows * tileColumns;
        for (int tileIdx = 0; tileIdx < totalTiles; tileIdx++)
        {
            CalculateRenderCacheForTile(layerIdx, tileIdx, layerVertexCache, layerTextureCache);
        }

        return (layerVertexCache, layerTextureCache);
    }

    private void CalculateRenderCacheForTile(int layerIdx, int tileIdx, Span<VertexData> layerCache, Texture[] textureCache)
    {
        TileMapLayerGrid layer = Layers[layerIdx];
        Vector2 tileSize = layer.TileSize;
        Vector2 tileSizeHalf = tileSize / 2f;
        Vector2 tileIdx2D = layer.GetCoordinate2DFrom1D(tileIdx);
        Vector2 layerOffset = layer.RenderOffsetInTiles * layer.TileSize;

        TileMapTile tileData = layer.GetTileAt(tileIdx);
        //GetTileData(layer, tileIdx, out uint tId, out bool flipX, out bool flipY, out bool _);

        Span<VertexData> tileRenderData = layerCache.Slice(tileIdx * 4, 4);

        // If empty skip it
        // todo: maybe we dont wanna cache these at all to save on vertices
        if (tileData.TextureId == 0)
        {
            for (var i = 0; i < tileRenderData.Length; i++)
            {
                tileRenderData[i].Color = 0;
            }

            return;
        }

        (Texture tilesetTexture, Rectangle tiUV) = GetTileRenderData(tileData);
        textureCache[tileIdx] = tilesetTexture;

        // Calculate dimensions of the tile.
        Vector2 position = (tileIdx2D * tileSize) - tileSizeHalf;
        position += layerOffset;

        var v3 = new Vector3(position, layerIdx);
        var c = new Color(255, 255, 255, (int)(layer.Opacity * 255));

        // Write to tilemap mesh.
        bool flipX = false;
        bool flipY = false;
        VertexData.SpriteToVertexData(tileRenderData, v3, tileSize, c, tilesetTexture, tiUV, flipX, flipY);
    }

    #endregion

    #region Editor

    public void EditorUpdateRenderCacheForTile(TileMapLayerGrid layer, Vector2 tileLocation)
    {
        int layerIdx = Layers.IndexOf(layer);
        if (layerIdx == -1) return;

        AssertNotNull(_cachedTileRenderData);
        AssertNotNull(_cachedTileTextures);

        int tileIdx = layer.GetCoordinate1DFrom2D(tileLocation);

        VertexData[] cacheForThisLayer = _cachedTileRenderData[layerIdx];
        Texture[] textureCacheForThisLayer = _cachedTileTextures[layerIdx];
        CalculateRenderCacheForTile(layerIdx, tileIdx, cacheForThisLayer, textureCacheForThisLayer);
    }

    public void EditorUpdateRenderCacheForLayer(TileMapLayerGrid layer)
    {
        int layerIdx = Layers.IndexOf(layer);
        if (layerIdx == -1) return;

        AssertNotNull(_cachedTileRenderData);
        AssertNotNull(_cachedTileTextures);

        (VertexData[] vertexData, Texture[] textureData) = CalculateRenderCacheForLayer(layerIdx);
        _cachedTileRenderData[layerIdx] = vertexData;
        _cachedTileTextures[layerIdx] = textureData;
    }

    #endregion

    #region Render

    public void RenderTileLayerRange(RenderComposer composer, Rectangle clipRect, int start = 0, int end = -1)
    {
        end = end == -1 ? Layers.Count : end;
        for (int layerId = start; layerId < end; layerId++)
        {
            TileMapLayerGrid layer = Layers[layerId];
            if (!layer.Visible) continue;
            RenderLayer(composer, layerId, clipRect);
        }
    }

    public void RenderLayer(RenderComposer composer, int layerIdx, Rectangle clipVal)
    {
        if (_cachedTileRenderData == null || _cachedTileTextures == null) return;
        VertexData[] renderCache = _cachedTileRenderData[layerIdx];
        Texture[] textureCache = _cachedTileTextures[layerIdx];

        var layer = Layers[layerIdx];
        Vector2 tileSize = layer.TileSize;
        Vector2 sizeInTiles = layer.SizeInTiles;

        var yStart = (int)Maths.Clamp(MathF.Floor(clipVal.Y / tileSize.Y), 0, sizeInTiles.Y);
        var yEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Bottom / tileSize.Y), 0, sizeInTiles.Y);
        var xStart = (int)Maths.Clamp(MathF.Floor(clipVal.X / tileSize.X), 0, sizeInTiles.X);
        var xEnd = (int)Maths.Clamp(MathF.Ceiling(clipVal.Right / tileSize.X), 0, sizeInTiles.X);

        for (int y = yStart; y < yEnd; y++)
        {
            var yIdx = (int)(y * sizeInTiles.X);
            for (int x = xStart; x < xEnd; x++)
            {
                int tileIdx = yIdx + x;
                int tileVertexIdx = tileIdx * 4;
                if (renderCache[tileVertexIdx].Color == 0) continue;

                Span<VertexData> vertices = composer.RenderStream.GetStreamMemory(4, BatchMode.Quad, textureCache[tileIdx]);
                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = renderCache[tileVertexIdx + i];
                }
            }
        }
    }

    #endregion
}
