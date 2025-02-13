#nullable enable

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

    #region Rendering Cache

    private TileMapLayerRenderCache[]? _renderCache;

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
