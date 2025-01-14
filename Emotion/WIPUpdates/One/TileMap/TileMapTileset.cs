using Emotion.IO;
using Microsoft.CodeAnalysis;

#nullable enable

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapTileset
{
    public SerializableAssetHandle<TextureAsset> Texture = new();
    public Vector2 Spacing;
    public Vector2 Margin;
    public Vector2 TileSize = Vector2.One;
    public bool BilinearFilterTexture = true;

    public override string ToString()
    {
        return Texture?.Name ?? "Textureless Tileset";
    }

    public Vector2 GetTilesetSizeInTiles()
    {
        AssetHandle<TextureAsset> handle = Texture.GetAssetHandle();
        if (!handle.AssetLoaded) return Vector2.Zero;

        TextureAsset? asset = handle.Asset;
        if (asset == null || asset.Texture == null) return Vector2.Zero;

        Vector2 textureAssetSize = asset.Texture.Size;
        Vector2 margin = Margin;
        Vector2 spacing = Spacing;

        Vector2 marginLess = textureAssetSize - margin;
        Vector2 tileSizeWithSpacing = TileSize + Spacing;
        Vector2 sizeInTiles = (marginLess / tileSizeWithSpacing).Round();

        return Vector2.Max(sizeInTiles, Vector2.One);
    }
}
