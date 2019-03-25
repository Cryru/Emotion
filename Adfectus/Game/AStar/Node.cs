#region Using

using System;
using System.Numerics;

#endregion

namespace Adfectus.Game.AStar
{
    public class Node : IEquatable<Node>
    {
        // User Properties
        public Vector2 Location
        {
            get => new Vector2(X, Y);
        }

        public int X;
        public int Y;
        public bool Walkable;

        // A* Properties
        public int F
        {
            get => G + H;
        }

        public int G;
        public int H;

        public Node CameFrom;

        public Node(int x, int y, bool walkable)
        {
            X = x;
            Y = y;
            Walkable = walkable;
        }

        public Node Copy()
        {
            return new Node(X, Y, Walkable);
        }

        public bool Equals(Node other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }
    }
}