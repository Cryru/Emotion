#nullable enable

namespace Emotion.WIPUpdates.One.TileMap;

public struct TileTextureId
{
    public ushort Value;

    public static implicit operator TileTextureId(ushort val)
    {
        return new TileTextureId { Value = val };
    }

    public static implicit operator ushort(TileTextureId tsId)
    {
        return tsId.Value;
    }
}