#region Using

using System;
using System.Drawing;
using System.Numerics;
using System.Xml.Serialization;

#endregion

#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team
All rights reserved.
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A rectangle object
    /// </summary>
    public struct Rectangle : IEquatable<Rectangle>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        #region Public Properties

        public static Rectangle Empty { get; } = new Rectangle();

        [XmlIgnore]
        public float Left
        {
            get => X;
            set => X = value;
        }

        [XmlIgnore]
        public float Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        [XmlIgnore]
        public float Top
        {
            get => Y;
            set => Y = value;
        }

        [XmlIgnore]
        public float Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        public Vector2 TopLeft
        {
            get => new Vector2(X, Y);
        }

        public Vector2 TopRight
        {
            get => new Vector2(Right, Y);
        }

        public Vector2 BottomLeft
        {
            get => new Vector2(X, Top);
        }

        public Vector2 BottomRight
        {
            get => new Vector2(Right, Bottom);
        }

        [XmlIgnore]
        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [XmlIgnore]
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [XmlIgnore]
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        [XmlIgnore]
        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        #endregion

        #region Constructors

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 location, Vector2 size) : this(location.X, location.Y, size.X, size.Y)
        {
        }

        public Rectangle(float x, float y, Vector2 size) : this(x, y, size.X, size.Y)
        {
        }

        #endregion Constructors

        #region Public Methods

        public Vector3 LocationZ(float z)
        {
            return new Vector3(X, Y, z);
        }

        public Vector3 PositionZ(float z)
        {
            return new Vector3(X, Y, z);
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        public bool Contains(float x, float y)
        {
            return X <= x && x < X + Width && Y <= y && y < Y + Height;
        }

        public bool Contains(Vector2 value)
        {
            return X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
        }

        public bool Contains(Rectangle value)
        {
            return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        public Rectangle Inflate(float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;

            return this;
        }

        public Rectangle Deflate(float horizontalValue, float verticalValue)
        {
            X += horizontalValue;
            Y += verticalValue;
            Width -= horizontalValue * 2;
            Height -= verticalValue * 2;

            return this;
        }

        public bool IsEmpty
        {
            get => Width == 0 && Height == 0 && X == 0 && Y == 0;
        }

        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && this == rectangle;
        }

        public override string ToString()
        {
            return $"[X:{X} Y:{Y} Width:{Width} Height:{Height}]";
        }

        public override int GetHashCode()
        {
            return (int) Math.Pow(Math.Pow(X, Y), Math.Pow(Width, Height));
        }

        public bool Intersects(Vector2 v2)
        {
            return Intersects(new Rectangle(v2, Vector2.One));
        }

        public bool Intersects(Rectangle r2)
        {
            return r2.Left < Right &&
                   Left < r2.Right &&
                   r2.Top < Bottom &&
                   Top < r2.Bottom;
        }

        public bool IntersectsInclusive(Vector2 v2)
        {
            return IntersectsInclusive(new Rectangle(v2, Vector2.One));
        }

        public bool IntersectsInclusive(Rectangle r2)
        {
            return r2.Left <= Right &&
                   Left <= r2.Right &&
                   r2.Top <= Bottom &&
                   Top <= r2.Bottom;
        }

        #endregion

        // Taken from Nez and Modified
        // MIT License
        // https://github.com/prime31/Nez

        #region Extensions

        /// <summary>
        /// Get a rectangle representing half of the rectangle - cut from a certain side.
        /// </summary>
        /// <param name="rectSide"></param>
        /// <returns></returns>
        public Rectangle GetHalfRect(RectSide rectSide)
        {
            switch (rectSide)
            {
                case RectSide.Top:
                    return new Rectangle(X, Y, Width, Height / 2);
                case RectSide.Bottom:
                    return new Rectangle(X, Y + Height / 2, Width, Height / 2);
                case RectSide.Left:
                    return new Rectangle(X, Y, Width / 2, Height);
                case RectSide.Right:
                    return new Rectangle(X + Width / 2, Y, Width / 2, Height);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get a rectangle representing a portion of the Rectangle with a width/height of size that is on the RectSide of the
        /// Rectangle but still contained
        /// within it.
        /// </summary>
        /// <returns>The rect rectSide portion.</returns>
        /// <param name="rectSide">RectSide.</param>
        /// <param name="size">Size.</param>
        public Rectangle GetRectPortion(RectSide rectSide, int size = 1)
        {
            switch (rectSide)
            {
                case RectSide.Top:
                    return new Rectangle(X, Y, Width, size);
                case RectSide.Bottom:
                    return new Rectangle(X, Y + Height - size, Width, size);
                case RectSide.Left:
                    return new Rectangle(X, Y, size, Height);
                case RectSide.Right:
                    return new Rectangle(X + Width - size, Y, size, Height);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Expand this rectangle in a certain direction.
        /// </summary>
        /// <param name="rectSide"></param>
        /// <param name="amount"></param>
        public void ExpandSide(RectSide rectSide, int amount)
        {
            // ensure we have a positive value
            amount = Math.Abs(amount);

            switch (rectSide)
            {
                case RectSide.Top:
                    Y -= amount;
                    Height += amount;
                    break;
                case RectSide.Bottom:
                    Height += amount;
                    break;
                case RectSide.Left:
                    X -= amount;
                    Width += amount;
                    break;
                case RectSide.Right:
                    Width += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get a rectangle representing the union of the two Rectangles. The result will be a rectangle that encompasses the other
        /// two.
        /// </summary>
        /// <param name="value1">First.</param>
        /// <param name="value2">Second.</param>
        public static Rectangle Union(Rectangle value1, Rectangle value2)
        {
            var result = new Rectangle {X = Math.Min(value1.X, value2.X), Y = Math.Min(value1.Y, value2.Y)};
            result.Width = Math.Max(value1.Right, value2.Right) - result.X;
            result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;

            return result;
        }

        /// <summary>
        /// Get a rectangle representing the union of this rectangle and another one.
        /// </summary>
        /// <param name="value2"></param>
        /// <returns></returns>
        public Rectangle Union(Rectangle value2)
        {
            return Union(this, value2);
        }

        /// <summary>
        /// Get a rectangle representing the union of first and a 2D point.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="point">Point.</param>
        public static Rectangle Union(Rectangle first, Vector2 point)
        {
            var rect = new Rectangle(point.X, point.Y, 0, 0);
            return Union(first, rect);
        }

        /// <summary>
        /// Get a rectangle representing the union of this rectangle and a 2D point.
        /// </summary>
        /// <param name="point">Point.</param>
        public Rectangle Union(Vector2 point)
        {
            return Union(this, point);
        }

        /// <summary>
        /// creates a Rectangle given min/max points (top-left, bottom-right points)
        /// </summary>
        /// <returns>The minimum max points.</returns>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        public static Rectangle FromMinMaxPoints(Point min, Point max)
        {
            return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        /// <summary>
        /// given the points of a polygon calculates the bounds
        /// </summary>
        /// <returns>The from polygon points.</returns>
        /// <param name="points">Points.</param>
        public static Rectangle BoundsFromPolygonPoints(Vector2[] points)
        {
            // we need to find the min/max x/y values
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            foreach (Vector2 pt in points)
            {
                if (pt.X < minX)
                    minX = pt.X;
                if (pt.X > maxX)
                    maxX = pt.X;

                if (pt.Y < minY)
                    minY = pt.Y;
                if (pt.Y > maxY)
                    maxY = pt.Y;
            }

            return FromMinMaxPoints(new Point((int) minX, (int) minY), new Point((int) maxX, (int) maxY));
        }

        /// <summary>
        /// clones and returns a new Rectangle with the same data as the current rectangle
        /// </summary>
        public Rectangle Clone()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// scales the rect
        /// </summary>
        /// <param name="scale">Scale.</param>
        public void Scale(Vector2 scale)
        {
            X = (int) (X * scale.X);
            Y = (int) (Y * scale.Y);
            Width = (int) (Width * scale.X);
            Height = (int) (Height * scale.Y);
        }


        /// <summary>
        /// Whether the ray intersects with the rectangle, and at what distance.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="ray"></param>
        /// <param name="distance">The distance at which the ray will intersect with the rectangle.</param>
        /// <returns>Whether the ray intersects with the rectangle.</returns>
        public bool RayIntersects(ref Rectangle rect, ref Ray2D ray, out float distance)
        {
            distance = 0f;
            float maxValue = float.MaxValue;

            if (Math.Abs(ray.Direction.X) < 1E-06f)
            {
                if (ray.Start.X < X || ray.Start.X > X + Width)
                    return false;
            }
            else
            {
                float num11 = 1f / ray.Direction.X;
                float num8 = (X - ray.Start.X) * num11;
                float num7 = (X + Width - ray.Start.X) * num11;
                if (num8 > num7)
                {
                    float num14 = num8;
                    num8 = num7;
                    num7 = num14;
                }

                distance = Math.Max(num8, distance);
                maxValue = Math.Min(num7, maxValue);
                if (distance > maxValue)
                    return false;
            }

            if (Math.Abs(ray.Direction.Y) < 1E-06f)
            {
                if (ray.Start.Y < Y || ray.Start.Y > Y + Height) return false;
            }
            else
            {
                float num10 = 1f / ray.Direction.Y;
                float num6 = (Y - ray.Start.Y) * num10;
                float num5 = (Y + Height - ray.Start.Y) * num10;
                if (num6 > num5)
                {
                    float num13 = num6;
                    num6 = num5;
                    num5 = num13;
                }

                distance = Math.Max(num6, distance);
                maxValue = Math.Min(num5, maxValue);
                if (distance > maxValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// returns a Bounds the spans the current bounds and the provided delta positions
        /// </summary>
        /// <returns>The swept broadphase box.</returns>
        /// <param name="deltaX">Velocity x.</param>
        /// <param name="deltaY">Velocity y.</param>
        public Rectangle GetSweptBroadphaseBounds(int deltaX, int deltaY)
        {
            Rectangle broadphaseBox = Empty;

            broadphaseBox.X = deltaX > 0 ? X : X + deltaX;
            broadphaseBox.Y = deltaY > 0 ? Y : Y + deltaY;
            broadphaseBox.Width = deltaX > 0 ? deltaX + Width : Width - deltaX;
            broadphaseBox.Height = deltaY > 0 ? deltaY + Height : Height - deltaY;

            return broadphaseBox;
        }


        /// <summary>
        /// returns true if the boxes are colliding
        /// moveX and moveY will return the movement that b1 must move to avoid the collision
        /// </summary>
        /// <param name="other">Other.</param>
        /// <param name="moveX">Move x.</param>
        /// <param name="moveY">Move y.</param>
        public bool CollisionCheck(ref Rectangle other, out float moveX, out float moveY)
        {
            moveX = moveY = 0.0f;

            float l = other.X - (X + Width);
            float r = other.X + other.Width - X;
            float t = other.Y - (Y + Height);
            float b = other.Y + other.Height - Y;

            // check that there was a collision
            if (l > 0 || r < 0 || t > 0 || b < 0)
                return false;

            // find the offset of both sides
            moveX = Math.Abs(l) < r ? l : r;
            moveY = Math.Abs(t) < b ? t : b;

            // only use whichever offset is the smallest
            if (Math.Abs(moveX) < Math.Abs(moveY))
                moveY = 0.0f;
            else
                moveX = 0.0f;

            return true;
        }


        /// <summary>
        /// Calculates the signed depth of intersection between two rectangles.
        /// </summary>
        /// <returns>
        /// The amount of overlap between two intersecting rectangles. These depth values can be negative depending on which sides
        /// the rectangles
        /// intersect. This allows callers to determine the correct direction to push objects in order to resolve collisions.
        /// If the rectangles are not intersecting, Vector2.Zero is returned.
        /// </returns>
        public Vector2 GetIntersectionDepth(ref Rectangle rectA, ref Rectangle rectB)
        {
            // calculate half sizes
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // calculate centers
            var centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            var centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // calculate current and minimum-non-intersecting distances between centers
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // if we are not intersecting at all, return (0, 0)
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // calculate and return intersection depths
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

            return new Vector2(depthX, depthY);
        }


        public Vector2 GetClosestPointOnBoundsToOrigin()
        {
            var max = new Vector2(Right, Bottom);
            float minDist = Math.Abs(Location.X);
            var boundsPoint = new Vector2(Location.X, 0);

            if (Math.Abs(max.X) < minDist)
            {
                minDist = Math.Abs(max.X);
                boundsPoint.X = max.X;
                boundsPoint.Y = 0f;
            }

            if (Math.Abs(max.Y) < minDist)
            {
                minDist = Math.Abs(max.Y);
                boundsPoint.X = 0f;
                boundsPoint.Y = max.Y;
            }

            if (!(Math.Abs(Location.Y) < minDist)) return boundsPoint;
            boundsPoint.X = 0;
            boundsPoint.Y = Location.Y;

            return boundsPoint;
        }


        /// <summary>
        /// returns the closest point that is in or on the Rectangle to the given point
        /// </summary>
        /// <returns>The closest point on rectangle to point.</returns>
        /// <param name="point">Point.</param>
        public Vector2 GetClosestPointOnRectangleToPoint(Vector2 point)
        {
            // for each axis, if the point is outside the box clamp it to the box else leave it alone
            return new Vector2 {X = Math.Clamp(point.X, Left, Right), Y = Math.Clamp(point.Y, Top, Bottom)};
        }


        /// <summary>
        /// gets the closest point that is on the rectangle border to the given point
        /// </summary>
        /// <returns>The closest point on rectangle border to point.</returns>
        /// <param name="point">Point.</param>
        public Vector2 GetClosestPointOnRectangleBorderToPoint(Vector2 point)
        {
            // for each axis, if the point is outside the box clamp it to the box else leave it alone
            var res = new Vector2 {X = Math.Clamp((int) point.X, (int) Left, (int) Right), Y = Math.Clamp((int) point.Y, (int) Top, (int) Bottom)};

            // if point is inside the rectangle we need to push res to the border since it will be inside the rect
            if (!Contains(res)) return res;
            float dl = res.X - Left;
            float dr = Right - res.X;
            float dt = res.Y - Top;
            float db = Bottom - res.Y;

            float min = Math.Min(Math.Min(dl, dr), Math.Min(dt, db));
            if (min == dt)
                res.Y = Top;
            else if (min == db)
                res.Y = Bottom;
            else if (min == dl)
                res.X = Left;
            else
                res.X = Right;

            return res;
        }

        #endregion
    }
}