#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;

#endregion

namespace Emotion.Primitives
{
    public struct Circle : IShape
    {
        public float X;
        public float Y;
        public float Radius;

        #region Properties

        public static Circle Empty { get; } = new Circle();

        [DontSerialize]
        public Vector2 Center
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Top
        {
            get => new Vector2(X, Y + Radius);
        }

        public Vector2 Bottom
        {
            get => new Vector2(X, Y - Radius);
        }

        public Vector2 Left
        {
            get => new Vector2(X - Radius, Y);
        }

        public Vector2 Right
        {
            get => new Vector2(X + Radius, Y);
        }

        public float Circumference
        {
            get => 2 * (float) Math.PI * Radius;
        }

        public float Area
        {
            get => (float) Math.PI * (float) Math.Pow(Radius, 2);
        }

        #endregion

        #region Constructors

        public Circle(Vector2 center, float radius)
        {
            X = center.X;
            Y = center.Y;
            Radius = radius;
        }

        public Circle(Circle circle)
        {
            X = circle.X;
            Y = circle.Y;
            Radius = circle.Radius;
        }

        #endregion

        public Vector2 PointOnCircle(float angle)
        {
            float x = Radius * angle + X;
            float y = Radius * angle + Y;
            return new Vector2(x, y);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref LineSegment l)
        {
            return l.Intersects(ref this);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionPoint(ref LineSegment l)
        {
            return l.GetIntersectionPoint(ref this);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Vector2 p)
        {
            return Vector2.Distance(Center, p) <= Radius;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(ref Vector2 p)
        {
            return Vector2.Distance(Center, p) < Radius;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Rectangle rect)
        {
            if (Vector2.Distance(rect.TopLeft, Center) > Radius) return false;
            if (Vector2.Distance(rect.TopRight, Center) > Radius) return false;
            if (Vector2.Distance(rect.BottomLeft, Center) > Radius) return false;
            if (Vector2.Distance(rect.BottomRight, Center) > Radius) return false;
            return true;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(ref Rectangle rect)
        {
            if (Vector2.Distance(rect.TopLeft, Center) >= Radius) return false;
            if (Vector2.Distance(rect.TopRight, Center) >= Radius) return false;
            if (Vector2.Distance(rect.BottomLeft, Center) >= Radius) return false;
            if (Vector2.Distance(rect.BottomRight, Center) >= Radius) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref Rectangle r)
        {
            if (r.Contains(Center)) return true;

            Span<LineSegment> surfaces = stackalloc LineSegment[4];
            r.GetLineSegments(surfaces);
            for (var i = 0; i < surfaces.Length; i++)
            {
                if (Intersects(ref surfaces[i])) return true;
            }

            return false;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IShape CloneShape()
        {
            return Clone();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Circle Clone()
        {
            return new Circle(Center, Radius);
        }

        public override string ToString()
        {
            return $"Circle [X:{X}, Y:{Y}, Radius:{Radius}]";
        }
    }
}