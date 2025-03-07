#nullable enable

using Emotion.IO;
using Emotion.WIPUpdates.Grids;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapTileset
{
    public SerializableAsset<TextureAsset> Texture = new();
    public Vector2 Spacing;
    public Vector2 Margin;
    public Vector2 TileSize = Vector2.One;
    public bool BilinearFilterTexture = false;

    public override string ToString()
    {
        return Texture?.Name ?? "Textureless Tileset";
    }

    public Vector2 GetTilesetTextureSize()
    {
        TextureAsset? asset = Texture.Get();
        if (asset == null || !asset.Loaded) return TileSize;

        return Vector2.Max(asset.Texture.Size, TileSize);
    }

    public Vector2 GetCoordOfTId(TileTextureId tId)
    {
        Vector2 totalSize = GetTilesetTextureSize();
        Vector2 sizeInTiles = GridHelpers.GetGridSizeInElements(totalSize, TileSize, Margin, Spacing);

        tId -= 1;
        return Grid.GetCoordinate2DFrom1D(tId, sizeInTiles);
    }

    public TileTextureId GetTIdOfCoord(Vector2 coord)
    {
        Vector2 totalSize = GetTilesetTextureSize();
        Vector2 sizeInTiles = GridHelpers.GetGridSizeInElements(totalSize, TileSize, Margin, Spacing);

        float tileOneD = (coord.Y * sizeInTiles.X) + coord.X;
        tileOneD += 1; // 0 is empty

        return (TileTextureId)tileOneD;
    }
}
