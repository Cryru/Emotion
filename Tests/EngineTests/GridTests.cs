using Emotion.Common;
using Emotion.Testing;
using Emotion.WIPUpdates.Grids;
using System.Numerics;
using System.Text;

namespace Tests.EngineTests;

[Test]
[TestClassRunParallel]
public class GridTests
{
    [Test]
    public void ExpandingGridTest()
    {
        var grid = new ExpandingGrid<uint>(Vector2.Zero);

        // Default is no size
        Assert.Equal(grid.GetSize(), new Vector2(0));
        Assert.False(grid.IsValidPosition(new Vector2(0)));

        // Try to read value anyway
        uint readValue = grid.GetAt(new Vector2(0, 0));
        Assert.Equal(readValue, 0);

        readValue = grid.GetAt(new Vector2(9999, 9999));
        Assert.Equal(readValue, 0);

        // Resize and try setting
        grid.Resize(new Vector2(10));
        Assert.Equal(grid.GetSize(), new Vector2(10, 10));
        Assert.True(grid.IsValidPosition(new Vector2(0)));

        readValue = grid.GetAt(new Vector2(0, 0));
        Assert.Equal(readValue, 0);

        grid.SetAt(new Vector2(0, 0), 1569);
        readValue = grid.GetAt(new Vector2(0, 0));
        Assert.Equal(readValue, 1569);

        // Try auto-resize X
        bool layerBoundsChanged = grid.ExpandingSetAt(new Vector2(100, 0), 1337);
        Assert.True(layerBoundsChanged);

        readValue = grid.GetAt(new Vector2(100, 0));
        Assert.Equal(readValue, 1337);

        Assert.Equal(grid.GetSize(), new Vector2(101, 10));

        // Auto resize Y
        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(100, 100), 1447);
        Assert.True(layerBoundsChanged);

        readValue = grid.GetAt(new Vector2(100, 100));
        Assert.Equal(readValue, 1447);

        Assert.Equal(grid.GetSize(), new Vector2(101, 101));

        // Auto resize both
        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(200, 200), 1557);
        Assert.True(layerBoundsChanged);

        readValue = grid.GetAt(new Vector2(200, 200));
        Assert.Equal(readValue, 1557);

        Assert.Equal(grid.GetSize(), new Vector2(201, 201));

        // Auto resize with offset
        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(-200, -200), 1667);
        Assert.True(layerBoundsChanged);

        Assert.Equal(grid.PositionOffset, new Vector2(200, 200));

        readValue = grid.GetAt(new Vector2(0, 0));
        Assert.Equal(readValue, 1667);

        readValue = grid.GetAt(new Vector2(400, 400));
        Assert.Equal(readValue, 1557);

        readValue = grid.GetAt(new Vector2(300, 300));
        Assert.Equal(readValue, 1447);

        readValue = grid.GetAt(new Vector2(300, 200));
        Assert.Equal(readValue, 1337);

        readValue = grid.GetAt(new Vector2(200, 200));
        Assert.Equal(readValue, 1569);

        // Auto compact tests
        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(1, 1), 0);
        Assert.False(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(401, 401)); // Size unchanged

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(0, 0), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(201, 201)); // Shrunk

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(200, 200), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(101, 101)); // Shrunk

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(100, 100), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(101, 1)); // Should be same size

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(100, 0), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(1, 1)); // Now it should have shrunk

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(0, 0), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(0, 0)); // Shrunk

        grid.ExpandingSetAt(new Vector2(0, 0), 1);
        grid.ExpandingSetAt(new Vector2(1, 0), 1);
        grid.ExpandingSetAt(new Vector2(0, 1), 1);
        grid.ExpandingSetAt(new Vector2(1, 1), 1);
        Assert.Equal(grid.GetSize(), new Vector2(2, 2));

        // . .
        // . .

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(0, 1), 0);
        Assert.False(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(2, 2));

        // . .
        // x .

        layerBoundsChanged = grid.ExpandingSetAt(new Vector2(1, 1), 0);
        Assert.True(layerBoundsChanged);
        Assert.Equal(grid.GetSize(), new Vector2(2, 1));

        // . .
    }

    private static void DebugShowGrid<T>(ExpandingGrid<T> grid) where T : struct, INumber<T>
    {
        StringBuilder b = new StringBuilder();

        var size = grid.GetSize();
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                T val = grid.GetAt(new Vector2(x, y));
                if (val == T.Zero)
                {
                    b.Append('.');
                }
                else
                {
                    b.Append('x');
                }
            }
            b.Append('\n');
        }

        Engine.Log.Info(b.ToString(), "DebugTest");
    }
}
