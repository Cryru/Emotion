#nullable enable

namespace Emotion.WIPUpdates.One.TileMap;

public struct TileMapTile
{
    public TileTextureId TextureId;
    public TilesetId TilesetId;

    public TileMapTile(TileTextureId tId, TilesetId tsId)
    {
        TextureId = tId;
        TilesetId = tsId;
    }

    public static TileMapTile Empty = new TileMapTile(0, 0);
    public static uint EmptyUint = 0;

    public override string ToString()
    {
        return $"<Tid:{TextureId}, TsId:{TilesetId}>";
    }

    public static implicit operator TileMapTile(uint val)
    {
        TileTextureId textureId = (TileTextureId)(val & 0xFFFF);
        TilesetId tilesetId = (TilesetId)((val >> 16) & 0xFFFF);
        return new TileMapTile(textureId, tilesetId);
    }

    public static implicit operator uint(TileMapTile tile)
    {
        return ((uint)tile.TilesetId << 16) | tile.TextureId;
    }
}
