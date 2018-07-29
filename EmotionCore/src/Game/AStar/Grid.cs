// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Linq;
using Emotion.Game.Tiled;
using Emotion.Primitives;

#endregion

namespace Emotion.Game.AStar
{
    public class Grid
    {
        #region Properties

        public int Width
        {
            get => _nodes.GetLength(0);
        }

        public int Height
        {
            get => _nodes.GetLength(1);
        }

        #endregion

        private Node[,] _nodes;

        /// <summary>
        /// Create a grid from a list of nodes.
        /// </summary>
        /// <param name="nodes">The list of nodes to create a matrix from.</param>
        public Grid(Node[,] nodes)
        {
            _nodes = nodes;
        }

        /// <summary>
        /// Create a grid from a a Map object.
        /// </summary>
        public Grid(Map map, int layerId, int[] solidTiles)
        {
            _nodes = new Node[map.TiledMap.Width, map.TiledMap.Height];

            for (int x = 0; x < map.TiledMap.Width; x++)
            {
                for (int y = 0; y < map.TiledMap.Height; y++)
                {
                    int tileId = x + y * map.TiledMap.Height;
                    int imageId = map.GetTileImageIdInLayer(tileId, layerId);
                    bool solid = !solidTiles.Contains(imageId);

                    _nodes[x, y] = new Node(x, y, solid);
                }
            }
        }

        #region Accessors

        public Node GetNodeAt(Vector2 location)
        {
            return GetNodeAt(location.X, location.Y);
        }

        public Node GetNodeAt(float x, float y)
        {
            return GetNodeAt((int) x, (int) y);
        }

        public Node GetNodeAt(int x, int y)
        {
            return _nodes[x, y].Copy();
        }

        #endregion
    }
}