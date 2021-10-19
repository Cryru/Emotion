#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.TMX.Layer;
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

        /// <summary>
        /// The world size of one pathing grid tile.
        /// </summary>
        public Vector2 PathGridTileSize { get; protected set; }

        #endregion

        private bool[,] _nodes;

        public PathingGrid(int width, int height, Vector2? tileSize = null)
        {
            Width = width;
            Height = height;
            PathGridTileSize = tileSize ?? Vector2.One;
            _nodes = new bool[width, height];
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    _nodes[x, y] = true;
                }
            }
        }

        public PathingGrid(Vector2 size, Vector2? tileSize = null) : this((int)size.X, (int)size.Y, tileSize)
        {
        }

        /// <summary>
        /// Create a grid from a TileMap object.
        /// </summary>
        /// <param name="tileMap">The map to create the grid from.</param>
        /// <param name="layerId">The tile layer to create the grid from.</param>
        /// <param name="impassableTiles">The list of image ids considered impassable.</param>
        public static PathingGrid FromTileMap<T>(TileMap<T> tileMap, int layerId, int[] impassableTiles) where T : TransformRenderable
        {
            if (layerId == -1 || tileMap.TiledMap == null || layerId > tileMap.TiledMap.Layers.Count - 1) return null;
            TmxLayer layer = tileMap.TiledMap.Layers[layerId];

            var newGrid = new PathingGrid(layer.Width, layer.Height, tileMap.TileSize);
            for (var x = 0; x < newGrid.Width; x++)
            {
                for (var y = 0; y < newGrid.Height; y++)
                {
                    int tileId = x + y * layer.Width;
                    int imageId = tileMap.GetTileImageIdInLayer(tileId, layerId, out int _);
                    bool solid = impassableTiles.IndexOf(imageId) != -1;
                    if (solid) newGrid.SetWalkable(x, y, false);
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
            if (x >= Width || x < 0) return;
            if (y >= Height || y < 0) return;

            _nodes[x, y] = walkable;
        }

        /// <summary>
        /// Whether the specified tile is walkable.
        /// </summary>
        public virtual bool IsWalkable(int x, int y)
        {
            if (x < 0) return false;
            if (y < 0) return false;
            if (x >= Width) return false;
            if (y >= Height) return false;

            return _nodes[x, y];
        }

        /// <summary>
        /// Whether the specified tile is walkable.
        /// </summary>
        public bool IsWalkable(Vector2 coordinate)
        {
            var x = (int)coordinate.X;
            var y = (int)coordinate.Y;
            return IsWalkable(x, y);
        }

        #region World To Pathing

        public Vector2 WorldToPathTile(Vector2 pos)
        {
            return (pos / PathGridTileSize).Floor();
        }

        public Vector2 WorldToPassableTile(Vector2 pos)
        {
            Vector2 pathingGridTile = WorldToPathTile(pos);
            if (IsWalkable(pathingGridTile)) return pos;

            for (var i = 0; i < Maths.CardinalDirections2D.Length; i++)
            {
                Vector2 direction = Maths.CardinalDirections2D[i];
                Vector2 tileInDirection = pathingGridTile + direction;
                if (IsWalkable(tileInDirection)) return pathingGridTile * PathGridTileSize;
            }

            Engine.Log.Warning($"Couldn't snap pos {pos}/{pathingGridTile} to a passable tile.", MessageSource.Game);
            return pos;
        }

        #endregion
    }
}