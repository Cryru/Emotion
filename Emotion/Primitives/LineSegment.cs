#region Using

using System;
using System.Numerics;
using Emotion.Utility;

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

        public Vector2 GetClosestPointOnLineSegment(Vector2 p)
        {
            Vector2 AP = p - Start; //Vector from A to P   
            Vector2 AB = End - Start; //Vector from A to B  

            float magnitudeAB = AB.LengthSquared(); // Magnitude of AB vector (it's length squared)     
            float dAPAB = Vector2.Dot(AP, AB); // The DOT product of a_to_p and a_to_b     
            float distance = dAPAB / magnitudeAB; // The normalized "distance" from a to your closest point  

            if (distance < 0) // Check if P projection is over the line
                return Start;
            if (distance > 1)
                return End;
            return Start + AB * distance;
        }

        /// <summary>
        /// Returns the normal vector on the left side of the line.
        /// </summary>
        /// <param name="getRightNormal">Whether to return the normal on the right side instead.</param>
        public Vector2 GetNormal(bool getRightNormal = false)
        {
            Vector2 d = End - Start;
            return Vector2.Normalize(getRightNormal ? new Vector2(d.Y, -d.X) : new Vector2(-d.Y, d.X));
        }

        /// <summary>
        /// Whether the provided point is left (or above) the line, and false if it is right or below.
        /// If the point is on the line, returns null
        /// </summary>
        public bool? IsPointLeftOf(Vector2 c)
        {
            // vertical line
            if (Maths.Approximately(End.X - Start.X, 0))
            {
                if (c.X < End.X) return End.Y > Start.Y;
                if (c.X > End.X) return End.Y <= Start.Y;
                return null;
            }

            // horizontal line
            if (Maths.Approximately(End.Y - Start.Y, 0))
            {
                if (c.Y < End.Y) return End.X <= Start.X;
                if (c.Y > End.Y) return End.X > Start.X;
                return null;
            }

            double slope = (End.Y - Start.Y) / (End.X - Start.X);
            double yIntercept = Start.Y - Start.X * slope;
            double cSolution = slope * c.X + yIntercept;
            if (slope == 0) return null;

            if (c.Y > cSolution) return End.X > Start.X;
            if (c.Y < cSolution) return End.X <= Start.X;
            return null;
        }

        public Vector2 PointOnLineAtDistance(float distance)
        {
            float d = Length();
            float t = distance / d;

            float xt = (1 - t) * Start.X + t * End.X;
            float yt = (1 - t) * Start.Y + t * End.Y;

            return new Vector2(xt, yt);
        }
    }
}