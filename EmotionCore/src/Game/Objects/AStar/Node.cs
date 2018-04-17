// Emotion - https://github.com/Cryru/Emotion

#region Using

#endregion

using System;
using Emotion.Primitives;

namespace Emotion.Game.Objects.AStar
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
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X^Y;
        }
    }
}