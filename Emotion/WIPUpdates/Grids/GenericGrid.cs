#nullable enable

using Emotion.Utility;

namespace Emotion.WIPUpdates.Grids;

public class GenericGrid<T> : IGrid<T>, IMutableGrid<T>
{
    protected Vector2 _size;
    protected T[] _data;

    public GenericGrid(Vector2 size)
    {
        _size = size;
        _data = new T[(int)size.X * (int)size.Y];
    }

    // serialization constructor
    protected GenericGrid()
    {
        _data = null!;
    }

    #region IGrid

    public T[] GetRawData()
    {
        return _data;
    }

    public void SetRawData(T[] data, Vector2 gridSize)
    {
        _data = data;
        _size = gridSize;
    }

    public virtual Vector2 GetOrigin()
    {
        return Vector2.Zero;
    }

    public Vector2 GetSize()
    {
        return _size;
    }

    public T GetAt(Vector2 position)
    {
        if (!IsValidPosition(position)) return default;
        int oneD = GridHelpers.GetCoordinate1DFrom2D(position, _size);
        return _data[oneD];
    }

    public void SetAt(Vector2 position, T value)
    {
        if (!IsValidPosition(position)) return;
        int oneD = GridHelpers.GetCoordinate1DFrom2D(position, _size);
        _data[oneD] = value;
    }

    public bool IsValidPosition(Vector2 position)
    {
        if (position.X < 0 || position.Y < 0) return false;
        return position.X < _size.X && position.Y < _size.Y;
    }

    #endregion

    #region IMutableGrid

    public void Resize(Vector2 newSize, Func<int, int, T>? initNewDataFunc = null)
    {
        int newWidth = (int)newSize.X;
        int newHeight = (int)newSize.Y;

        if (newWidth == _size.X && newHeight == _size.Y) return;

        T[] newData = new T[newWidth * newHeight];
        int oldWidth = (int)_size.X;
        int minX = (int)MathF.Min(newWidth, _size.X);
        int minY = (int)MathF.Min(newHeight, _size.Y);

        Span<T> dataSpan = _data.AsSpan();
        for (int y = 0; y < minY; y++)
        {
            // Create slices for the old and new data rows
            Span<T> oldRow = dataSpan.Slice(y * oldWidth, minX);
            Span<T> newRow = newData.AsSpan(y * newWidth, minX);

            // Copy the data for this row
            oldRow.CopyTo(newRow);
        }

        if (initNewDataFunc != null)
        {
            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    if (y < _size.Y && x < _size.X) continue;
                    int newOneD = x + newWidth * y;
                    newData[newOneD] = initNewDataFunc(x, y);
                }
            }
        }

        _size = new Vector2(newWidth, newHeight);
        _data = newData;
    }

    public void Offset(Vector2 offset, bool wrapAround, Action<int, int, T>? onOffset = null)
    {
        int x = (int)offset.X;
        int y = (int)offset.Y;
        if (x == 0 && y == 0) return;

        T[] offsetData = new T[_data.Length];

        int width = (int)_size.X;
        int height = (int)_size.Y;

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
                    int oldIndex = GridHelpers.GetCoordinate1DFrom2D(new Vector2(oldX, oldY), _size);
                    int newIndex = GridHelpers.GetCoordinate1DFrom2D(new Vector2(newX, newY), _size);
                    offsetData[newIndex] = _data[oldIndex];

                    onOffset?.Invoke(newX, newY, offsetData[newIndex]);
                }
            }
        }

        _data = offsetData;
    }

    public bool Compact(T compactValue, out Vector2 compactOffset)
    {
        Vector2 minNonEmpty = new Vector2(-1);
        Vector2 maxNonEmpty = new Vector2(-1);

        for (int y = 0; y < _size.Y; y++)
        {
            for (int x = 0; x < _size.X; x++)
            {
                Vector2 thisTile2D = new Vector2(x, y);
                int thisOneD = GridHelpers.GetCoordinate1DFrom2D(thisTile2D, _size);
                Assert(thisOneD != -1);

                T val = _data[thisOneD];
                if (Helpers.AreObjectsEqual(val, compactValue)) continue;

                if (minNonEmpty.X == -1 || minNonEmpty.X > x)
                    minNonEmpty.X = x;

                if (minNonEmpty.Y == -1 || minNonEmpty.Y > y)
                    minNonEmpty.Y = y;

                if (maxNonEmpty.X == -1 || maxNonEmpty.X < x)
                    maxNonEmpty.X = x;

                if (maxNonEmpty.Y == -1 || maxNonEmpty.Y < y)
                    maxNonEmpty.Y = y;
            }
        }

        // Fully empty
        if (minNonEmpty.X == -1)
        {
            Resize(Vector2.Zero);
            compactOffset = Vector2.Zero;
            return true;
        }

        if (minNonEmpty.X > 0 || minNonEmpty.Y > 0 || maxNonEmpty.X < _size.X - 1 || maxNonEmpty.Y < _size.Y - 1)
        {
            Offset(-minNonEmpty, false);

            float sizeX = (maxNonEmpty.X - minNonEmpty.X) + 1;
            float sizeY = (maxNonEmpty.Y - minNonEmpty.Y) + 1;
            Resize(new Vector2(sizeX, sizeY));

            compactOffset = -minNonEmpty;
            return true;
        }

        compactOffset = Vector2.Zero;
        return false;
    }

    #endregion
}
