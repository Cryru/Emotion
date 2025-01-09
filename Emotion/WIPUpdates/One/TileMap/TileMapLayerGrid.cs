using Emotion.Game.World.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.TileMap;

public class TileMapLayerGrid : PackedNumericMapGrid<uint>
{
    public TileMapLayerGrid()
    {
        SizeInTiles = new Vector2(10);
    }

    public Vector2 GetTilePosOfWorldPos(Vector2 location)
    {
        //location -= TileSize / 2f;

        var left = MathF.Round(location.X / TileSize.X);
        var top = MathF.Round(location.Y / TileSize.Y);

        return new Vector2(left, top);
    }

    public bool IsPositionInMap(Vector2 tileCoord2d)
    {
        if (tileCoord2d.X < 0) return false;
        if (tileCoord2d.Y < 0) return false;
        if (tileCoord2d.X >= SizeInTiles.X) return false;
        if (tileCoord2d.Y >= SizeInTiles.Y) return false;
        return true;
    }

    public Vector2 GetWorldPosOfTile(Vector2 tileCoord2d)
    {
        return tileCoord2d * TileSize - TileSize / 2f;
    }
}
