#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    public struct Circle
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

        #region Static Methods

        public static bool IntersectsLine(Circle c, Ray2D l)
        {
            return IsPointInsideCircle(c, l.Start) || IsPointInsideCircle(c, l.End);
        }

        public static bool IsPointInsideCircle(Circle c, Vector2 p)
        {
            return Vector2.Distance(c.Center, p) <= c.Radius;
        }

        public static bool IntersectsRectangle(Circle c, Rectangle r)
        {
            return r.Intersects(c.Center) ||
                   IntersectsLine(c, new Ray2D(new Vector2(r.X, r.Y), new Vector2(r.X + r.Width, r.Y))) ||
                   IntersectsLine(c, new Ray2D(new Vector2(r.X + r.Width, r.Y), new Vector2(r.X + r.Width, r.Y + r.Height))) ||
                   IntersectsLine(c, new Ray2D(new Vector2(r.X + r.Width, r.Y + r.Height), new Vector2(r.X, r.Y + r.Height))) ||
                   IntersectsLine(c, new Ray2D(new Vector2(r.X, r.Y + r.Height), new Vector2(r.X, r.Y)));
        }

        #endregion
    }
}