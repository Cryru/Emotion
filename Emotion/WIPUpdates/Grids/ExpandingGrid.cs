#nullable enable

using Emotion.Utility;

namespace Emotion.WIPUpdates.Grids;

public class ExpandingGrid<T>(Vector2 size) : GenericGrid<T>(size) where T : struct
{
    public Vector2 PositionOffset;

    public override Vector2 GetOrigin()
    {
        return PositionOffset;
    }

    /// <summary>
    /// Sets the data at the specified location.
    /// Returns a boolean for whether the size or origin of the grid changed.
    /// </summary>
    public bool ExpandingSetAt(Vector2 location, T value)
    {
        Assert(location == location.Floor());

        T defVal = default;
        bool isDelete = Helpers.AreObjectsEqual(defVal, value);
        bool validPosition = IsValidPosition(location);
        if (validPosition)
        {
            // Changing tile in map - easy peasy
            SetAt(location, value);
            if (!isDelete) return false;

            // We deleted a tile, try compacting the grid.
            bool compacted = Compact(defVal, out Vector2 compactOffset);
            if (compacted)
            {
                PositionOffset += compactOffset;
                return true;
            }

            return false;
        }

        // Deleting outside bounds - no need to do anything
        if (isDelete) return false;

        // Resize the grid to fit the tile.
        float setX = location.X;
        float newWidth = _size.X;
        float offsetX = 0;
        if (setX < 0)
        {
            newWidth += -setX;
            offsetX = -setX;
        }
        else if (setX >= _size.X)
        {
            float diff = setX - (_size.X - 1);
            newWidth += diff;
        }

        float setY = location.Y;
        float newHeight = _size.Y;
        float offsetY = 0;
        if (setY < 0)
        {
            newHeight += -setY;
            offsetY = -setY;
        }
        else if (setY >= _size.Y)
        {
            float diff = setY - (_size.Y - 1);
            newHeight += diff;
        }

        Resize(new Vector2(newWidth, newHeight));
        Offset(new Vector2(offsetX, offsetY), false);
        PositionOffset += new Vector2(offsetX, offsetY);

        Vector2 newLocation = location + new Vector2(offsetX, offsetY);
        SetAt(newLocation, value);

        return true;
    }
}
