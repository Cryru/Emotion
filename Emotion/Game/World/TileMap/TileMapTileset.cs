#nullable enable

using Emotion.Primitives.Grids;

namespace Emotion.Game.World.TileMap;

public class TileMapTileset
{
    public TextureReference Texture = new();
    public Vector2 Spacing;
    public Vector2 Margin;
    public Vector2 TileSize = Vector2.One;
    public bool BilinearFilterTexture = false;

    public override string ToString()
    {
        return $"Tileset {Texture}" ?? "Textureless Tileset";
    }

    public Vector2 GetTilesetTextureSize()
    {
        Texture? asset = Texture.GetObject();
        if (asset == null) return TileSize;
        return Vector2.Max(asset.Size, TileSize);
    }

    public Vector2 GetCoordOfTId(TileTextureId tId)
    {
        Vector2 totalSize = GetTilesetTextureSize();
        Vector2 sizeInTiles = GridHelpers.GetGridSizeInElements(totalSize, TileSize, Margin, Spacing);

        tId -= 1;
        return GridHelpers.GetCoordinate2DFrom1D(tId, sizeInTiles);
    }

    public TileTextureId GetTIdOfCoord(Vector2 coord)
    {
        Vector2 totalSize = GetTilesetTextureSize();
        Vector2 sizeInTiles = GridHelpers.GetGridSizeInElements(totalSize, TileSize, Margin, Spacing);

        float tileOneD = coord.Y * sizeInTiles.X + coord.X;
        tileOneD += 1; // 0 is empty

        return (TileTextureId)tileOneD;
    }
}
