using Emotion.Common;
using Emotion.Testing;
using Emotion.WIPUpdates.One.TileMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tests.EngineTests;

[Test]
[TestClassRunParallel]
public class GridTests
{
    [Test]
    public void TilemapGridTest()
    {
        var layer = new TileMapLayerGrid() { TileSize = new Vector2(64) };

        // Default is no size
        Assert.Equal(layer.SizeInTiles, new Vector2(0));
        Assert.False(layer.IsPositionInMap(new Vector2(0)));

        // Try to read tile anyway
        TileMapTile readTile = layer.GetTileAt(new Vector2(0, 0));
        Assert.Equal(readTile.TextureId, 0);
        Assert.Equal(readTile.TilesetId, 0);

        readTile = layer.GetTileAt(new Vector2(9999, 9999));
        Assert.Equal(readTile.TextureId, 0);
        Assert.Equal(readTile.TilesetId, 0);

        // Resize and try setting
        layer.Resize(10, 10);
        Assert.Equal(layer.SizeInTiles, new Vector2(10, 10));
        Assert.True(layer.IsPositionInMap(new Vector2(0)));

        readTile = layer.GetTileAt(new Vector2(0, 0));
        Assert.Equal(readTile.TextureId, 0);
        Assert.Equal(readTile.TilesetId, 0);

        bool success = layer.SetTileAt(new Vector2(0, 0), 15, 69);
        Assert.True(success);

        readTile = layer.GetTileAt(new Vector2(0, 0));
        Assert.Equal(readTile.TextureId, 15);
        Assert.Equal(readTile.TilesetId, 69);

        // Try auto-resize X
        success = layer.EditorSetTileAt(new Vector2(100, 0), 13, 37, out bool layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);

        readTile = layer.GetTileAt(new Vector2(100, 0));
        Assert.Equal(readTile.TextureId, 13);
        Assert.Equal(readTile.TilesetId, 37);

        Assert.Equal(layer.SizeInTiles, new Vector2(101, 10));

        // Auto resize Y
        success = layer.EditorSetTileAt(new Vector2(100, 100), 14, 47, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);

        readTile = layer.GetTileAt(new Vector2(100, 100));
        Assert.Equal(readTile.TextureId, 14);
        Assert.Equal(readTile.TilesetId, 47);

        Assert.Equal(layer.SizeInTiles, new Vector2(101, 101));

        // Auto resize both
        success = layer.EditorSetTileAt(new Vector2(200, 200), 15, 57, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);

        readTile = layer.GetTileAt(new Vector2(200, 200));
        Assert.Equal(readTile.TextureId, 15);
        Assert.Equal(readTile.TilesetId, 57);

        Assert.Equal(layer.SizeInTiles, new Vector2(201, 201));

        // Auto resize with offset
        success = layer.EditorSetTileAt(new Vector2(-200, -200), 16, 67, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);

        Assert.Equal(layer.RenderOffsetInTiles, new Vector2(200, 200));

        readTile = layer.GetTileAt(new Vector2(0, 0));
        Assert.Equal(readTile.TextureId, 16);
        Assert.Equal(readTile.TilesetId, 67);

        readTile = layer.GetTileAt(new Vector2(400, 400));
        Assert.Equal(readTile.TextureId, 15);
        Assert.Equal(readTile.TilesetId, 57);

        readTile = layer.GetTileAt(new Vector2(300, 300));
        Assert.Equal(readTile.TextureId, 14);
        Assert.Equal(readTile.TilesetId, 47);

        readTile = layer.GetTileAt(new Vector2(300, 200));
        Assert.Equal(readTile.TextureId, 13);
        Assert.Equal(readTile.TilesetId, 37);

        readTile = layer.GetTileAt(new Vector2(200, 200));
        Assert.Equal(readTile.TextureId, 15);
        Assert.Equal(readTile.TilesetId, 69);

        // Auto compact tests
        success = layer.EditorSetTileAt(new Vector2(1, 1), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.False(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(401, 401)); // Size unchanged
       
        success = layer.EditorSetTileAt(new Vector2(0, 0), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(201, 201)); // Shrunk

        success = layer.EditorSetTileAt(new Vector2(200, 200), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(101, 101)); // Shrunk

        success = layer.EditorSetTileAt(new Vector2(100, 100), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(101, 1)); // Should be same size

        success = layer.EditorSetTileAt(new Vector2(100, 0), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(1, 1)); // Now it should have shrunk

        success = layer.EditorSetTileAt(new Vector2(0, 0), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(0, 0)); // Shrunk

        layer.EditorSetTileAt(new Vector2(0, 0), 1, 0, out layerBoundsChanged);
        layer.EditorSetTileAt(new Vector2(1, 0), 1, 0, out layerBoundsChanged);
        layer.EditorSetTileAt(new Vector2(0, 1), 1, 0, out layerBoundsChanged);
        layer.EditorSetTileAt(new Vector2(1, 1), 1, 0, out layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(2, 2));

        // . .
        // . .

        success = layer.EditorSetTileAt(new Vector2(0, 1), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.False(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(2, 2));

        // . .
        // x .

        success = layer.EditorSetTileAt(new Vector2(1, 1), 0, 0, out layerBoundsChanged);
        Assert.True(success);
        Assert.True(layerBoundsChanged);
        Assert.Equal(layer.SizeInTiles, new Vector2(2, 1));

        // . .
    }

    private static void DebugShowGrid(TileMapLayerGrid grid)
    {
        StringBuilder b = new StringBuilder();

        for (int y = 0; y < grid.SizeInTiles.Y; y++)
        {
            for (int x = 0; x < grid.SizeInTiles.X; x++)
            {
                TileMapTile val = grid.GetTileAt(x, y);
                if (val == TileMapTile.Empty)
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
