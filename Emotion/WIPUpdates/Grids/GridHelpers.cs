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
}