#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// Represents a line.
    /// </summary>
    public struct LineSegment
    {
        public Vector2 Start;
        public Vector2 End;

        /// <summary>
        /// Is the line is has a sloped (m != 0) or is axis-aligned.
        /// </summary>
        public bool IsSloped
        {
            get => Start.Y != End.Y;
        }

        /// <summary>
        /// Is the line is ascending or descending from the start to the end.
        /// </summary>
        public bool IsAscending
        {
            get => Start.Y > End.Y;
        }

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        public float Length()
        {
            return MathF.Sqrt(MathF.Pow(MathF.Abs(End.X - Start.X), 2) + MathF.Pow(MathF.Abs(End.Y - Start.Y), 2));
        }

        public bool Intersects(ref Rectangle r)
        {
            var top = new LineSegment(r.TopLeft, r.TopRight);
            var right = new LineSegment(r.TopRight, r.BottomRight);
            var bottom = new LineSegment(r.BottomLeft, r.BottomRight);
            var left = new LineSegment(r.BottomLeft, r.TopLeft);

            return Intersects(ref top) || Intersects(ref right) || Intersects(ref bottom) || Intersects(ref left);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        /// </summary>
        public bool Intersects(ref LineSegment line)
        {
            Vector2 d = End - Start;
            Vector2 otherD = line.End - line.Start;

            double s = (-d.Y * (Start.X - line.Start.X) + d.X * (Start.Y - line.Start.Y)) / (-otherD.X * d.Y + d.X * otherD.Y);
            double t = (otherD.X * (Start.Y - line.Start.Y) - otherD.Y * (Start.X - line.Start.X)) / (-otherD.X * d.Y + d.X * otherD.Y);
            return s >= 0 && s <= 1 && t >= 0 && t <= 1;
        }

        public bool PointIsOnLine(Vector2 point)
        {
            return point.X >= Start.X && point.X <= End.X &&
                   point.Y >= Start.Y && point.Y <= End.Y ||
                   point.X >= End.X && point.X <= Start.X &&
                   point.Y >= End.Y && point.Y <= Start.Y;
        }
    }
}