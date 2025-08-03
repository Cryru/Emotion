#nullable enable

using Emotion.Graphics.Assets;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class GameMapTileData
{
    public List<TileMapLayer> Layers = new();
    public List<TileMapTileset> Tilesets = new();

    private Texture[]? _tilesetTexturesLoaded;

    public IEnumerator InitRuntimeDataRoutine()
    {
        _tilesetTexturesLoaded = new Texture[Tilesets.Count];

        // Cause loading of all tileset assets and add self as referencing
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            ts.Texture.Get(this);
        }

        // Wait to load
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            TextureAsset asset = ts.Texture.Get();
            yield return asset;

            // todo: hot reload
            if (asset.Loaded)
            {
                Texture texture = asset.Texture;
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
            TextureAsset? asset = ts.Texture.Get();
            Engine.AssetLoader.RemoveReferenceFromAsset(asset, this, true);
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

        Vector2 tilesetSize = tileset.GetTilesetTextureSize();
        int tId = tile.TextureId - 1;
        Rectangle uvRect = GridHelpers.GetBoxInGridAt1D(tId, tilesetSize, tileset.TileSize, tileset.Margin, tileset.Spacing);

        return (tilesetTexture, uvRect);
    }

    public void Render(Renderer c, Rectangle clipArea)
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
