﻿namespace Emotion.Game;

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
        for (int i = 0; i < _data.Length; i++)
        {
            yield return _data[i];
        }
    }

    public void Resize(int newWidth, int newHeight, Func<int, int, T>? initNewDataFunc = null)
    {
        if (newWidth == SizeInTiles.X && newHeight == SizeInTiles.Y) return;

        T[] newData = new T[newWidth * newHeight];
        for (int x = 0; x < SizeInTiles.X; x++)
        {
            for (int y = 0; y < SizeInTiles.Y; y++)
            {
                int oldOneD = GetCoordinate1DFrom2D(new Vector2(x, y));
                int newOneD = x + newWidth * y;
                newData[newOneD] = _data[oldOneD];
            }
        }

        if (initNewDataFunc != null)
        {
            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    if (y < SizeInTiles.Y && x < SizeInTiles.X) continue;
                    int newOneD = x + newWidth * y;
                    newData[newOneD] = initNewDataFunc(x, y);
                }
            }
        }

        SizeInTiles = new Vector2(newWidth, newHeight);
        _data = newData;
    }

    public void Offset(int x, int y, bool wrapAround, Action<int, int, T>? onOffset = null)
    {
        T[] offsetData = new T[_data.Length];

        int width = (int)SizeInTiles.X;
        int height = (int)SizeInTiles.Y;

        for (int oldX = 0; oldX < width; oldX++)
        {
            for (int oldY = 0; oldY < height; oldY++)
            {
                int newX = oldX + x;
                int newY = oldY + y;

                if (wrapAround)
                {
                    newX = (newX + width) % width;
                    newY = (newY + height) % height;
                }

                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    int oldIndex = GetCoordinate1DFrom2D(new Vector2(oldX, oldY));
                    int newIndex = GetCoordinate1DFrom2D(new Vector2(newX, newY));
                    offsetData[newIndex] = _data[oldIndex];

                    onOffset?.Invoke(newX, newY, offsetData[newIndex]);
                }
            }
        }

        _data = offsetData;
    }
}