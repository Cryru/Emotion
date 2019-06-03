#region Using

using System.Linq;
using System.Numerics;
using Adfectus.Game.Tiled;

#endregion

namespace Adfectus.Game.AStar
{
    /// <summary>
    /// A AStar grid map - can be used to path find.
    /// </summary>
    public class AStarGrid
    {
        #region Properties

        /// <summary>
        /// The width of the grid - in nodes.
        /// </summary>
        public int Width
        {
            get => _nodes.GetLength(0);
        }

        /// <summary>
        /// The height of the grid - in nodes.
        /// </summary>
        public int Height
        {
            get => _nodes.GetLength(1);
        }

        #endregion

        private AStarNode[,] _nodes;

        /// <summary>
        /// Create a grid from a list of nodes.
        /// </summary>
        /// <param name="nodes">The list of nodes to create a matrix from.</param>
        public AStarGrid(AStarNode[,] nodes)
        {
            _nodes = nodes;
        }

        /// <summary>
        /// Create a grid from a TileMap object.
        /// </summary>
        public AStarGrid(TileMap tileMap, int layerId, int[] solidTiles)
        {
            _nodes = new AStarNode[tileMap.TiledMap.Width, tileMap.TiledMap.Height];

            for (int x = 0; x < tileMap.TiledMap.Width; x++)
            {
                for (int y = 0; y < tileMap.TiledMap.Height; y++)
                {
                    int tileId = x + y * tileMap.TiledMap.Height;
                    int imageId = tileMap.GetTileImageIdInLayer(tileId, layerId);
                    bool solid = !solidTiles.Contains(imageId);

                    _nodes[x, y] = new AStarNode(x, y, solid);
                }
            }
        }

        /// <summary>
        /// Find a path from a location - to a location.
        /// </summary>
        /// <param name="from">The path start.</param>
        /// <param name="to">The path end.</param>
        /// <param name="diagonalMovement">Whether diagonal movement is allowed.</param>
        /// <returns></returns>
        public AStarPath FindPath(Vector2 from, Vector2 to, bool diagonalMovement = true)
        {
            return new AStarPath(this, from, to, diagonalMovement);
        }

        #region Accessors

        /// <summary>
        /// Get a copy of a node using its index as a Vector2.
        /// </summary>
        /// <param name="location">The index of the AStar node.</param>
        /// <returns>The node found, or null.</returns>
        public AStarNode GetNodeAt(Vector2 location)
        {
            return GetNodeAt(location.X, location.Y);
        }

        /// <summary>
        /// Get a copy of a node using its index (as floats).
        /// </summary>
        /// <param name="x">The x index of the node.</param>
        /// <param name="y">They y index of the node.</param>
        /// <returns>The node found, or null.</returns>
        public AStarNode GetNodeAt(float x, float y)
        {
            return GetNodeAt((int) x, (int) y);
        }

        /// <summary>
        /// Get a copy of a node using its index.
        /// </summary>
        /// <param name="x">The x index of the node.</param>
        /// <param name="y">They y index of the node.</param>
        /// <returns>The node found, or null.</returns>
        public AStarNode GetNodeAt(int x, int y)
        {
            if (x > Width || x < 0) return null;
            if (y > Height || y < 0) return null;

            return _nodes[x, y].Clone();
        }

        #endregion
    }
}