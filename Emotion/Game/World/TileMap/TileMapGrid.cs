#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Camera;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class TileMapGrid : IMapGrid
{
    public List<TileMapLayer> Layers = new();
    public List<TileMapTileset> Tilesets = new();

    private Texture[]? _tilesetTexturesLoaded;
    private AssetOwner<TextureAsset, Texture>[]? _tilesetTextureOwners;

    public IEnumerator InitRuntimeDataRoutine()
    {
        var textures = new Texture[Tilesets.Count];
        _tilesetTexturesLoaded = textures;
        _tilesetTextureOwners = new AssetOwner<TextureAsset, Texture>[Tilesets.Count];

        // Cause loading of all tileset assets and add self as referencing
        Coroutine[] routines = new Coroutine[_tilesetTexturesLoaded.Length];
        for (int i = 0; i < Tilesets.Count; i++)
        {
            _tilesetTexturesLoaded[i] = Texture.EmptyWhiteTexture;

            TileMapTileset ts = Tilesets[i];
            TextureReference asset = ts.Texture;

            // Initialize the tileset texture owners
            AssetOwner<TextureAsset, Texture> owner = new();
            owner.SetOnChangeCallback(static (owner, userData) => {
                if (userData is not TileMapTileset ts) return;

                Texture? texture = owner.GetCurrentObject();
                if (texture == null) return;

                if (ts.BilinearFilterTexture)
                    texture.Smooth = true;
            }, ts);
            _tilesetTextureOwners[i] = owner;

            routines[i] = owner.Set(asset) ?? Coroutine.CompletedRoutine;
        }

        yield return Coroutine.WhenAll(routines);
    }

    public void Done()
    {
        if (_tilesetTextureOwners == null) return;
        foreach (AssetOwner<TextureAsset, Texture> owner in _tilesetTextureOwners)
            owner.Done();
        _tilesetTextureOwners = null;
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

    public void Update(float dt)
    {

    }

    public void Render(Renderer r, CameraCullingContext culling)
    {
        Rectangle clipArea = culling.Rect2D;

        if (_renderCache == null)
            _renderCache = new TileMapLayerRenderCache[Layers.Count];
        else if (Layers.Count != _renderCache.Length)
            Array.Resize(ref _renderCache, Layers.Count);

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
            cache.Render(r, _tilesetTexturesLoaded);
        }
    }

    #endregion
}
