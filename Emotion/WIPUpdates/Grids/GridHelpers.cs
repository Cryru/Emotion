#nullable enable

namespace Emotion.WIPUpdates.Grids;

public static class GridHelpers
{
    public static Vector2 GetCoordinate2DFrom1D(int oneD, Vector2 size)
    {
        int width = (int)size.X;
        int x = oneD % width;
        int y = oneD / width;
        return new Vector2(x, y);
    }

    public static int GetCoordinate1DFrom2D(Vector2 twoD, Vector2 size)
    {
        var top = (int)twoD.Y;
        var left = (int)twoD.X;
        return (int)(left + size.X * top);
    }

    public static Vector2 GetGridSizeInElements(Vector2 totalSize, Vector2 elementSize, Vector2 margin, Vector2 spacing)
    {
        Vector2 totalSizeMarginless = totalSize - margin;
        Vector2 elementSizeWithSpacing = elementSize + spacing;

        return (totalSizeMarginless / elementSizeWithSpacing).Round();
    }

    public static Rectangle GetBoxInGridAt1D(int oneD, Vector2 totalSize, Vector2 elementSize, Vector2 margin, Vector2 spacing)
    {
        Vector2 sizeInElements = GetGridSizeInElements(totalSize, elementSize, margin, spacing);

        // Get the element needed
        Vector2 pos = GetCoordinate2DFrom1D(oneD, sizeInElements);
        int column = (int) pos.X;
        int row = (int) pos.Y;
        return new Rectangle(
            (elementSize.X * column) + margin.X + (spacing.X * (column + 1)),
            (elementSize.Y * row) + margin.Y + (spacing.Y * (row + 1)),
            elementSize
        );
    }
}