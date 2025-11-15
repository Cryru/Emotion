#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Graphics.Assets;
using Emotion.Graphics.Camera;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class TileMapData
{
    public TileMapTileset[] Tilesets = Array.Empty<TileMapTileset>();

    private AssetOwner<TextureAsset, Texture>[]? _tilesetTextureOwners;

    private bool _loaded = false;

    public IEnumerator InitRoutine()
    {
        _loaded = true;

        // Create asset owners for all tile set textures. 
        _tilesetTextureOwners = new AssetOwner<TextureAsset, Texture>[Tilesets.Length];
        Coroutine[] routines = new Coroutine[Tilesets.Length];
        for (int i = 0; i < Tilesets.Length; i++)
        {
            TileMapTileset ts = Tilesets[i];

            // Initialize the tileset texture owners
            AssetOwner<TextureAsset, Texture> owner = new();
            owner.SetOnChangeCallback(TextureOwnerOnSetChangeCallback, ts);
            _tilesetTextureOwners[i] = owner;

            routines[i] = owner.Set(ts.Texture) ?? Coroutine.CompletedRoutine;
        }

        yield return Coroutine.WhenAll(routines);
    }

    private static void TextureOwnerOnSetChangeCallback(AssetOwner<TextureAsset, Texture> owner, object? userData)
    {
        if (userData is not TileMapTileset ts) return;

        Texture? texture = owner.GetCurrentObject();
        if (texture == null) return;

        if (ts.BilinearFilterTexture)
            texture.Smooth = true;
    }

    public void Done()
    {
        if (_tilesetTextureOwners != null)
        {
            foreach (AssetOwner<TextureAsset, Texture> owner in _tilesetTextureOwners)
                owner.Done();
            _tilesetTextureOwners = null;
        }

        _loaded = false;
    }

    public (Texture texture, Rectangle uv) GetTileTexture(TileMapTile tile)
    {
        if (_tilesetTextureOwners == null || tile.TilesetId >= Tilesets.Length)
            return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        TileMapTileset tileset = Tilesets[tile.TilesetId];

        if (tile.TilesetId >= _tilesetTextureOwners.Length)
            return (Texture.EmptyWhiteTexture, Rectangle.Empty);

        AssetOwner<TextureAsset, Texture> textureOwner = _tilesetTextureOwners[tile.TilesetId];

        Vector2 tilesetSize = tileset.GetTilesetTextureSize();
        int tId = tile.TextureId - 1;
        Rectangle uvRect = GridHelpers.GetBoxInGridAt1D(tId, tilesetSize, tileset.TileSize, tileset.Margin, tileset.Spacing);

        return (textureOwner.GetCurrentObject() ?? Texture.EmptyWhiteTexture, uvRect);
    }

    #region Tilesets

    public void AddTileset(TileMapTileset tileset)
    {
        Tilesets = Tilesets.AddToArray(tileset);

        if (_loaded)
        {
            AssertNotNull(_tilesetTextureOwners);

            AssetOwner<TextureAsset, Texture> owner = new();
            owner.SetOnChangeCallback(TextureOwnerOnSetChangeCallback, tileset);
            owner.Set(tileset.Texture);

            _tilesetTextureOwners = _tilesetTextureOwners.AddToArray(owner);
        }
    }

    #endregion
}
