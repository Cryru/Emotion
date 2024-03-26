namespace Emotion.Game;

#nullable enable

public abstract class Grid
{
    public Vector2 TileSize { get; set; } = new Vector2(1f);

    public Vector2 SizeInTiles { get; set; } = new Vector2(1f);

    public Vector2 GetCoordinate2DFrom1D(int oneD)
    {
        if (!IsCoordinate1DValid(oneD)) return Vector2.Zero;

        int width = (int)SizeInTiles.X;
        int x = oneD % width;
        int y = oneD / width;
        return new Vector2(x, y);
    }

    public int GetCoordinate1DFrom2D(Vector2 twoD)
    {
        if (!IsCoordinate2DValid(twoD)) return 0;

        var top = (int)twoD.Y;
        var left = (int)twoD.X;

        return (int)(left + SizeInTiles.X * top);
    }

    public bool IsCoordinate2DValid(Vector2 coord)
    {
        if (coord.X < 0 || coord.Y < 0) return false;
        return coord.X < SizeInTiles.X && coord.Y < SizeInTiles.Y;
    }

    public bool IsCoordinate1DValid(int oneDCoord)
    {
        if (oneDCoord < 0) return false;
        return oneDCoord < SizeInTiles.X * SizeInTiles.Y;
    }

    public Vector2 ClampCoordinate2DToGrid(Vector2 coord)
    {
        return Vector2.Clamp(coord, Vector2.Zero, SizeInTiles - Vector2.One);
    }
}

public class Grid<T> : Grid
{
    protected T[] _data = Array.Empty<T>();

    public Grid(int width, int height)
    {
        _data = new T[width * height];
        SizeInTiles = new Vector2(width, height);
    }

    // serialization constructor
    protected Grid()
    {

    }

    public T? GetValueInTile(int x, int y)
    {
        return GetValueInTile(new Vector2(x, y));
    }

    public virtual T? GetValueInTile(Vector2 pos)
    {
        if (_data.Length == 0 || !IsCoordinate2DValid(pos)) return default;

        int index = GetCoordinate1DFrom2D(pos);
        return _data[index];
    }

    public void SetValueInTile(int x, int y, T value)
    {
        SetValueInTile(new Vector2(x, y), value);
    }

    public virtual void SetValueInTile(Vector2 pos, T value)
    {
        if (_data.Length == 0 || !IsCoordinate2DValid(pos)) return;

        int coord = GetCoordinate1DFrom2D(pos);
        _data[coord] = value;
    }

    public IEnumerable<T> EnumAllTiles()
    {
        for (int x = 0; x < SizeInTiles.X; x++)
        {
            for (int y = 0; y < SizeInTiles.Y; y++)
            {
                T val = GetValueInTile(x, y)!;
                yield return val;
            }
        }
    }
}