#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Utility;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// Represents a line.
    /// </summary>
    public struct LineSegment : IEquatable<LineSegment>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Length()
        {
            return MathF.Sqrt(MathF.Pow(MathF.Abs(End.X - Start.X), 2) + MathF.Pow(MathF.Abs(End.Y - Start.Y), 2));
        }

        /// <summary>
        /// Returns whether the specified point is on the line.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PointIsOnLine(Vector2 point)
        {
            return point.X >= Start.X && point.X <= End.X &&
                   point.Y >= Start.Y && point.Y <= End.Y ||
                   point.X >= End.X && point.X <= Start.X &&
                   point.Y >= End.Y && point.Y <= Start.Y;
        }

        /// <summary>
        /// Find the closest point on the line segment to the specified point.
        /// </summary>
        /// <param name="p">The point to find the closest point to.</param>
        /// <param name="infinite">
        /// Line segments have a start and beginning, so if the point is further than the start or end it
        /// will be clamped, unless this is set to true.
        /// </param>
        /// <returns></returns>
        public Vector2 GetClosestPointOnLineSegment(Vector2 p, bool infinite = false)
        {
            Vector2 ap = p - Start;
            Vector2 ab = End - Start;

            float magnitudeAb = ab.LengthSquared();
            float dApab = Vector2.Dot(ap, ab);
            float distance = dApab / magnitudeAb; // The normalized "distance" from a to your closest point  

            if (infinite) return Start + ab * distance;
            if (distance < 0) // Check if P projection is over the line
                return Start;
            if (distance > 1)
                return End;

            return Start + ab * distance;
        }

        /// <summary>
        /// Returns the normal vector on the left side of the line. For more information on what left and right means
        /// refer to the documentation of IsPointLeftOf
        /// </summary>
        /// <param name="getRightNormal">Whether to return the normal on the right side instead.</param>
        public Vector2 GetNormal(bool getRightNormal = false)
        {
            Vector2 d = End - Start;
            return Vector2.Normalize(getRightNormal ? new Vector2(d.Y, -d.X) : new Vector2(-d.Y, d.X));
        }

        /// <summary>
        /// Whether the provided point is left (when top to bottom) or below (when left to right) of the line.
        /// If the line directions are reversed, the result is flipped as well.
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

        /// <summary>
        /// Get a point on the line at a specified distance from the start.
        /// Min 0 = Start, and Max Length() = End
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Vector2 PointOnLineAtDistance(float distance)
        {
            float d = Length();
            float t = distance / d;

            float xt = (1 - t) * Start.X + t * End.X;
            float yt = (1 - t) * Start.Y + t * End.Y;

            return new Vector2(xt, yt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LineSegment other)
        {
            return other.Start == Start && other.End == End;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LineSegment Clone()
        {
            return new LineSegment(Start, End);
        }

        #region Line

        /// <summary>
        /// https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        /// </summary>
        public bool Intersects(ref LineSegment line)
        {
            Vector2 d = End - Start;
            Vector2 otherD = line.End - line.Start;

            float s = (-d.Y * (Start.X - line.Start.X) + d.X * (Start.Y - line.Start.Y)) / (-otherD.X * d.Y + d.X * otherD.Y);
            float t = (otherD.X * (Start.Y - line.Start.Y) - otherD.Y * (Start.X - line.Start.X)) / (-otherD.X * d.Y + d.X * otherD.Y);
            return s >= 0 && s <= 1 && t >= 0 && t <= 1;
        }

        /// <summary>
        /// Find the point of intersection between two line segments.
        /// </summary>
        public Vector2 GetIntersectionPoint(ref LineSegment other)
        {
            float s10X = End.X - Start.X;
            float s10Y = End.Y - Start.Y;
            float s32X = other.End.X - other.Start.X;
            float s32Y = other.End.Y - other.Start.Y;

            float denom = s10X * s32Y - s32X * s10Y;
            if (denom == 0f) return Vector2.Zero; // Collinear.

            bool denomIsPositive = denom > 0;
            float s02X = Start.X - other.Start.X;
            float s02Y = Start.Y - other.Start.Y;

            float sNumer = s10X * s02Y - s10Y * s02X;
            if (sNumer < 0 == denomIsPositive) return Vector2.Zero;

            float tNumer = s32X * s02Y - s32Y * s02X;
            if (tNumer < 0 == denomIsPositive) return Vector2.Zero;
            if (sNumer > denom == denomIsPositive || tNumer > denom == denomIsPositive) return Vector2.Zero;

            float t = tNumer / denom;
            return new Vector2(Start.X + t * s10X, Start.Y + t * s10Y);
        }

        #endregion

        #region Circle

        /// <summary>
        /// Whether this line insects the circle.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref Circle c)
        {
            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            Vector2 closestPoint = GetClosestPointOnLineSegment(c.Center);
            return Vector2.Distance(closestPoint, c.Center) < c.Radius;
        }

        /// <summary>
        /// The intersection point between the circle and this line. If they don't intersect
        /// Vector2.Zero is returned instead.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionPoint(ref Circle c)
        {
            Vector2 closestPoint = GetClosestPointOnLineSegment(c.Center);
            return Vector2.Distance(closestPoint, c.Center) < c.Radius ? closestPoint : Vector2.Zero;
        }

        #endregion

        #region Rectangle

        /// <summary>
        /// Whether this line intersects the rectangle.
        /// </summary>
        public bool Intersects(ref Rectangle r)
        {
            Span<LineSegment> surfaces = stackalloc LineSegment[4];
            r.GetLineSegments(surfaces);
            for (var i = 0; i < surfaces.Length; i++)
            {
                if (Intersects(ref surfaces[i])) return true;
            }

            return false;
        }

        /// <summary>
        /// Get the intersection point between the rectangle and the line segment.
        /// </summary>
        public Vector2 GetIntersectionPoint(ref Rectangle r)
        {
            Span<LineSegment> surfaces = stackalloc LineSegment[4];
            r.GetLineSegments(surfaces);
            Vector2 intersectionPoint = Vector2.Zero;
            for (var i = 0; i < surfaces.Length; i++)
            {
                intersectionPoint = GetIntersectionPoint(ref surfaces[i]);
                if (intersectionPoint != Vector2.Zero) return intersectionPoint;
            }

            return intersectionPoint;
        }

        #endregion

        #region Ray

        /// <summary>
        /// Find the point of intersection between a ray and this line segment.
        /// If the ray doesn't hit the line 0,0 is returned.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionPoint(ref Ray2D ray)
        {
            return GetIntersectionPointAndDistance(ref ray, out float _);
        }

        /// <summary>
        /// Find the point of intersection between a ray and this line segment, and the distance of intersection.
        /// If the ray doesn't hit the line 0,0 is returned.
        /// </summary>
        public Vector2 GetIntersectionPointAndDistance(ref Ray2D ray, out float dist)
        {
            dist = float.MaxValue;

            Vector2 v1 = ray.Start - Start;
            Vector2 v2 = End - Start;
            var v3 = new Vector2(-ray.Direction.Y, ray.Direction.X);

            float dot = Vector2.Dot(v2, v3);
            if (MathF.Abs(dot) < 0.000001) return Vector2.Zero;

            dist = v2.Cross(v1) / dot;
            float t2 = Vector2.Dot(v1, v3) / dot;

            if (dist >= 0.0 && t2 >= 0.0 && t2 <= 1.0) return ray.Start + ray.Direction * dist;
            return Vector2.Zero;
        }

        #endregion

        /// <summary>
        /// Whether the two segments are equal.
        /// </summary>
        public static bool operator ==(LineSegment a, LineSegment b)
        {
            return a.Start == b.Start && a.End == b.End;
        }

        /// <summary>
        /// Whether the two segments are not equal.
        /// </summary>
        public static bool operator !=(LineSegment a, LineSegment b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is LineSegment l) return l == this;
            return false;
        }

        public override string ToString()
        {
            return $"[Start:{Start} End:{End}]";
        }

        public override int GetHashCode()
        {
            return Maths.GetCantorPair(Maths.GetCantorPair((int)Start.X, (int)Start.Y), Maths.GetCantorPair((int)End.X, (int)End.Y));
        }
    }
}