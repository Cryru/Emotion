#nullable enable

namespace Emotion.Game.World.TileMap;

public struct TilesetId
{
    public static TilesetId Invalid = ushort.MaxValue;

    public ushort Value;

    public static implicit operator TilesetId(ushort val)
    {
        return new TilesetId { Value = val };
    }

    public static implicit operator ushort(TilesetId tsId)
    {
        return tsId.Value;
    }
}