using Emotion.IO;

#nullable enable

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapTileset
{
    public SerializableAssetHandle<TextureAsset> Texture = new();
    public Vector2 Spacing;
    public Vector2 Margin;
    public Vector2 TileSize = Vector2.One;

    public Vector2 GetTilesetSizeInTiles()
    {
        AssetHandle<TextureAsset> handle = Texture.GetAssetHandle();
        if (!handle.AssetLoaded) return Vector2.Zero;

        TextureAsset? asset = handle.Asset;
        if (asset == null || asset.Texture == null) return Vector2.Zero;

        return asset.Texture.Size / TileSize;
    }
}
