#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Game.AStar
{
    /// <summary>
    /// An AStar node.
    /// </summary>
    public class AStarNode : IEquatable<AStarNode>
    {
        /// <summary>
        /// The location of the node within the parental grid.
        /// </summary>
        public Vector2 Location
        {
            get => new Vector2(X, Y);
        }

        /// <summary>
        /// The A* algorithm value.
        /// </summary>
        public int F
        {
            get => G + H;
        }

        /// <summary>
        /// The X position of the node.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y position of the node.
        /// </summary>
        public int Y;

        /// <summary>
        /// The distance to the node.
        /// </summary>
        public int G;

        /// <summary>
        /// The result of the node's heuristic function.
        /// </summary>
        public int H;

        /// <summary>
        /// The node the path to this node came from.
        /// </summary>
        public AStarNode CameFrom;

        /// <summary>
        /// Create a new node.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public AStarNode(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Whether this node equals another node.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AStarNode other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        /// <summary>
        /// The unique hash for this node.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetHashCode(X, Y);
        }

        /// <summary>
        /// Get the unique hashcode used for the node.
        /// </summary>
        public static int GetHashCode(int x, int y)
        {
            // Cantor-pair
            return (((x + y) * (x + y + 1)) / 2) + y;
        }
    }
}