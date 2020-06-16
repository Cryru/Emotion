#region Using

using System.Numerics;
using Emotion.Game.Tiled;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.Game.AStar
{
    /// <summary>
    /// A grid for path finding.
    /// </summary>
    public class PathingGrid
    {
        #region Properties

        /// <summary>
        /// The width of the grid - in nodes.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The height of the grid - in nodes.
        /// </summary>
        public int Height { get; protected set; }

        #endregion

        private bool[,] _nodes;

        public PathingGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _nodes = new bool[width, height];
        }

        /// <summary>
        /// Create a grid from a TileMap object.
        /// </summary>
        /// <param name="tileMap">The map to create the grid from.</param>
        /// <param name="layerId">The tile layer to create the grid from.</param>
        /// <param name="unwalkableTiles">The list of image ids considered unwalkable.</param>
        public static PathingGrid FromTileMap<T>(TileMap<T> tileMap, int layerId, int[] unwalkableTiles) where T : TransformRenderable
        {
            var newGrid = new PathingGrid((int) tileMap.SizeInTiles.X, (int) tileMap.SizeInTiles.Y);

            for (var x = 0; x < newGrid.Width; x++)
            {
                for (var y = 0; y < newGrid.Height; y++)
                {
                    int tileId = x + y * tileMap.TiledMap.Height;
                    int imageId = tileMap.GetTileImageIdInLayer(tileId, layerId, out int _);
                    bool solid = unwalkableTiles.IndexOf(imageId) != -1;
                    if (solid) newGrid.SetWalkable(x, y, true);
                }
            }

            return newGrid;
        }

        /// <summary>
        /// Set whether the position at x,y is walkable.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="walkable"></param>
        public void SetWalkable(int x, int y, bool walkable)
        {
            if (x >= Width) return;
            if (y >= Height) return;

            _nodes[x, y] = walkable;
        }

        /// <summary>
        /// Whether the specified tile is walkable.
        /// </summary>
        public bool IsWalkable(int x, int y)
        {
            if (x >= Width) return false;
            if (y >= Height) return false;

            return _nodes[x, y];
        }

        /// <summary>
        /// Whether the specified tile is walkable.
        /// </summary>
        public bool IsWalkable(Vector2 coordinate)
        {
            var x = (int) coordinate.X;
            var y = (int) coordinate.Y;
            return IsWalkable(x, y);
        }
    }
}