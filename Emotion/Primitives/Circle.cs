#region Using

using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;
using Emotion.Utility;

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

        public Vector2 GetRandomPointInsideCircle()
        {
            // Generate a random radius and angle.
            float angle = Helpers.GenerateRandomFloat() * 2 * MathF.PI;
            float randomRadius = MathF.Sqrt(Helpers.GenerateRandomFloat()) * Radius;

            // Convert polar coordinates to Cartesian coordinates
            float randomX = X + (float)(randomRadius * MathF.Cos(angle));
            float randomY = Y + (float)(randomRadius * MathF.Sin(angle));

            return new Vector2(randomX, randomY);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref LineSegment l)
        {
            return l.Intersects(ref this);
        }

        public bool Intersects(Circle c)
        {
            return Vector2.Distance(c.Center, Center) < Radius + c.Radius;
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

        public void GetLineSegments(Span<LineSegment> array, int detail)
        {
            Assert(detail > 3);
            for (int i = 0; i < detail; i++)
            {
                float angle1 = MathF.PI * 2 * i / detail;
                float angle2 = MathF.PI * 2 * (i + 1) / detail;

                Vector2 start = new Vector2(
                    Center.X + Radius * MathF.Cos(angle1),
                    Center.Y + Radius * MathF.Sin(angle1)
                );

                Vector2 end = new Vector2(
                    Center.X + Radius * MathF.Cos(angle2),
                    Center.Y + Radius * MathF.Sin(angle2)
                );

                array[i] = new LineSegment(start, end);
            }
        }
    }
}