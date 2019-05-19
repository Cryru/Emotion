#region Using

using System;
using System.Numerics;

#endregion

namespace Adfectus.Game.AStar
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
        /// The X position of the node.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y position of the node.
        /// </summary>
        public int Y;

        /// <summary>
        /// Whether the node is walkable.
        /// </summary>
        public bool Walkable;

        /// <summary>
        /// The A* algorithm value.
        /// </summary>
        public int F
        {
            get => G + H;
        }
        
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
        /// <param name="walkable"></param>
        public AStarNode(int x, int y, bool walkable)
        {
            X = x;
            Y = y;
            Walkable = walkable;
        }

        /// <summary>
        /// Copy the node.
        /// </summary>
        /// <returns></returns>
        public AStarNode Clone()
        {
            return new AStarNode(X, Y, Walkable);
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
            return X ^ Y;
        }
    }

    /// <summary>
    /// A node which can hold a reference to an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AStarNode<T> : AStarNode
    {
        /// <summary>
        /// An object to store in the node.
        /// Beware of reference copying.
        /// </summary>
        public T Tag;

        public AStarNode(T tag, int x, int y, bool walkable) : base(x, y, walkable)
        {
            Tag = tag;
        }

        /// <summary>
        /// Copy the node, with its tag.
        /// </summary>
        /// <returns></returns>
        public new AStarNode<T> Clone()
        {
            return new AStarNode<T>(Tag, X, Y, Walkable);
        }
    }
}