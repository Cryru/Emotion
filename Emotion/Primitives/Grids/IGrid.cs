#nullable enable

namespace Emotion.Primitives.Grids;

public interface IGrid
{
    public Vector2 GetOrigin();

    public Vector2 GetSize();

    public bool IsValidPosition(Vector2 position);

    public Vector2 ClampPositionToGrid(Vector2 position)
    {
        Vector2 origin = GetOrigin();
        Vector2 size = GetSize();
        return Vector2.Clamp(position, origin, size - Vector2.One);
    }
}

public interface IGridWorldSpaceTiles : IGrid
{
    public Vector2 TileSize { get; }

    public virtual Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        float left = MathF.Round(location.X / TileSize.X);
        float top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public virtual Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        Vector2 worldPos = (tileCoord2d * TileSize);
        return worldPos;
    }
}

public interface IGrid<T> : IGrid
{
    public T[] GetRawData();

    public void SetRawData(T[] data, Vector2 gridSize);

    public void SetAt(Vector2 position, T value);

    public T GetAt(Vector2 position);

    public void SetAt(int x, int y, T value)
    {
        SetAt(new Vector2(x, y), value);
    }

    public T GetAt(int x, int y)
    {
        return GetAt(new Vector2(x, y));
    }

    public bool _Save(string folder)
    {
        return true;
    }
}
