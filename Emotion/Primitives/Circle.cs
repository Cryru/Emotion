#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    public struct Circle : IShape
    {
        public float X;
        public float Y;
        public float Radius;

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

        #region Properties

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

        public override string ToString()
        {
            return $"Circle - X:{X}, Y:{Y}, Radius: {Radius}";
        }

        /// <inheritdoc />
        public bool Intersects(ref LineSegment l)
        {
            Vector2 d = l.End - l.Start;
            Vector2 f = l.Start - Center;

            float a = Vector2.Dot(d, d);
            float b = 2 * Vector2.Dot(f, d);
            float c = Vector2.Dot(f, f) - Radius * Radius;

            float discriminant = b * b - 4 * a * c;
            // Missed
            if (discriminant < 0) return false;

            // Either solution can be on or off.
            discriminant = MathF.Sqrt(discriminant);
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            // t1 is closer, check it first.
            if (t1 >= 0 && t1 <= 1) return true;
            return t2 >= 0 && t2 <= 1;
        }

        public bool Contains(ref Vector2 p)
        {
            return Vector2.Distance(Center, p) <= Radius;
        }

        public bool Intersects(ref Rectangle r)
        {
            var top = new LineSegment(r.TopLeft, r.TopRight);
            var right = new LineSegment(r.TopRight, r.BottomRight);
            var bottom = new LineSegment(r.BottomLeft, r.BottomRight);
            var left = new LineSegment(r.BottomLeft, r.TopLeft);

            return r.Intersects(Center) || Intersects(ref top) || Intersects(ref right) || Intersects(ref bottom) || Intersects(ref left);
        }

        public Circle Clone()
        {
            return new Circle(Center, Radius);
        }

        /// <inheritdoc />
        public IShape CloneShape()
        {
            return Clone();
        }

        public static Circle Empty { get; } = new Circle();
    }
}