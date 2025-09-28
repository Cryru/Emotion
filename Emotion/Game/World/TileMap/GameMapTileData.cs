#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
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
        var textures = new Texture[Tilesets.Count];
        _tilesetTexturesLoaded = textures;

        static Action<object> OnTileSetTextureLoaded(Texture[] textures, TextureReference asset, TileMapTileset tileset, int index)
        {
            return (_) =>
            {
                Texture? texture = asset.GetObject();
                textures[index] = texture ?? Texture.EmptyWhiteTexture;
                if (texture == null)
                    return;
                if (tileset.BilinearFilterTexture)
                    texture.Smooth = true;
            };
        }

        // Cause loading of all tileset assets and add self as referencing
        Coroutine[] routines = new Coroutine[_tilesetTexturesLoaded.Length];
        for (int i = 0; i < Tilesets.Count; i++)
        {
            _tilesetTexturesLoaded[i] = Texture.EmptyWhiteTexture;

            TileMapTileset ts = Tilesets[i];
            TextureReference asset = ts.Texture;
            routines[i] = Engine.CoroutineManager.StartCoroutine(
                asset.PerformLoading(
                    this,
                    OnTileSetTextureLoaded(textures, asset, ts, i),
                    true
                )
            );
        }

        yield return Coroutine.WhenAll(routines);
    }

    public void UnloadRuntimeData()
    {
        for (int i = 0; i < Tilesets.Count; i++)
        {
            TileMapTileset ts = Tilesets[i];
            ts.Texture.Cleanup();
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
