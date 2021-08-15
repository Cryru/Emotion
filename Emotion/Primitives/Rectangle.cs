#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common.Serialization;
using Emotion.Utility;

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
    public struct Rectangle : IEquatable<Rectangle>, IShape
    {
        /// <summary>
        /// The X position of the rectangle.
        /// </summary>
        public float X;

        /// <summary>
        /// The Y position of the rectangle. Up is negative.
        /// </summary>
        public float Y;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public float Width;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public float Height;

        #region Properties

        public static Rectangle Empty { get; } = new Rectangle();

        /// <summary>
        /// Whether the rectangle is 0,0,0,0
        /// </summary>
        public bool IsEmpty
        {
            get => Width == 0 && Height == 0 && X == 0 && Y == 0;
        }

        [DontSerialize]
        public float Left
        {
            get => X;
            set => X = value;
        }

        [DontSerialize]
        public float Right
        {
            get => X + Width;
            set => X = value - Width;
        }

        [DontSerialize]
        public float Top
        {
            get => Y;
            set => Y = value;
        }

        [DontSerialize]
        public float Bottom
        {
            get => Y + Height;
            set => Y = value - Height;
        }

        [DontSerialize]
        public Vector2 TopLeft
        {
            get => new Vector2(X, Y);
            set => Position = value;
        }

        [DontSerialize]
        public Vector2 TopRight
        {
            get => new Vector2(Right, Y);
            set => Position = new Vector2(value.X - Right, value.Y);
        }

        [DontSerialize]
        public Vector2 BottomLeft
        {
            get => new Vector2(X, Bottom);
            set => Position = new Vector2(value.X, value.Y - Bottom);
        }

        [DontSerialize]
        public Vector2 BottomRight
        {
            get => new Vector2(Right, Bottom);
            set => Position = value - BottomRight;
        }

        [DontSerialize]
        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [DontSerialize]
        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        [DontSerialize]
        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        [DontSerialize]
        public Vector2 Center
        {
            get => new Vector2(X + Width / 2, Y + Height / 2);
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        public float Diagonal
        {
            get => MathF.Sqrt(MathF.Pow(Width, 2) + MathF.Pow(Height, 2));
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

        public Rectangle(Vector3 location, Vector2 size) : this(location.X, location.Y, size.X, size.Y)
        {
        }

        public Rectangle(Vector2 location, Vector2 size) : this(location.X, location.Y, size.X, size.Y)
        {
        }

        public Rectangle(float x, float y, Vector2 size) : this(x, y, size.X, size.Y)
        {
        }

        #endregion Constructors

        #region Operators

        /// <summary>
        /// Whether the two rectangles are equal.
        /// </summary>
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        /// <summary>
        /// Whether the two rectangles are not equal.
        /// </summary>
        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Multiply both the size and location of the rectangle by the given float.
        /// </summary>
        public static Rectangle operator *(Rectangle a, float f)
        {
            a.X *= f;
            a.Y *= f;
            a.Width *= f;
            a.Height *= f;
            return a;
        }

        /// <summary>
        /// Add the size and location of another rectangle to this one.
        /// </summary>
        public static Rectangle operator +(Rectangle a, Rectangle b)
        {
            Rectangle n = a.Clone();
            n.X += b.X;
            n.Y += b.Y;
            n.Width += b.Width;
            n.Height += b.Height;
            return n;
        }

        /// <summary>
        /// Subtract the size and location of another rectangle to this one.
        /// </summary>
        public static Rectangle operator -(Rectangle a, Rectangle b)
        {
            Rectangle n = a.Clone();
            n.X -= b.X;
            n.Y -= b.Y;
            n.Width -= b.Width;
            n.Height -= b.Height;
            return n;
        }

        /// <summary>
        /// Subtract a vector2 two from the position of this rectangle.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Rectangle operator -(Rectangle a, Vector2 v)
        {
            a.X -= v.X;
            a.Y -= v.Y;
            return a;
        }

        #endregion

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Vector2 value)
        {
            return X < value.X && value.X < X + Width && Y < value.Y && value.Y < Y + Height;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(ref Vector2 value)
        {
            return X <= value.X && value.X <= X + Width && Y <= value.Y && value.Y <= Y + Height;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref LineSegment line)
        {
            return line.Intersects(ref this);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionPoint(ref LineSegment l)
        {
            return l.GetIntersectionPoint(ref this);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IShape CloneShape()
        {
            return Clone();
        }

        /// <summary>
        /// Returns a new Rectangle with the same data as the current rectangle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle Clone()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// Create a Vec3 from the rectangle's position with the specified Z value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 PositionZ(float z)
        {
            return new Vector3(X, Y, z);
        }

        /// <summary>
        /// Get the line segments making up the rectangle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LineSegment[] GetLineSegments()
        {
            var arr = new LineSegment[4];
            GetLineSegments(arr);
            return arr;
        }

        /// <summary>
        /// Get the line segments making up the rectangle.
        /// </summary>
        /// <param name="array">The array to fill with the segments.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetLineSegments(Span<LineSegment> array)
        {
            array[0] = new LineSegment(TopLeft, TopRight);
            array[1] = new LineSegment(TopRight, BottomRight);
            array[2] = new LineSegment(BottomRight, BottomLeft);
            array[3] = new LineSegment(BottomLeft, TopLeft);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rectangle other)
        {
            return this == other;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && this == rectangle;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[X:{X} Y:{Y} Width:{Width} Height:{Height}]";
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Maths.GetCantorPair(Maths.GetCantorPair((int) X, (int) Y), Maths.GetCantorPair((int) Width, (int) Height));
        }

        /// <summary>
        /// Whether the rectangle contains the given rectangle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Rectangle value)
        {
            return X < value.X && value.X + value.Width < X + Width && Y < value.Y && value.Y + value.Height < Y + Height;
        }

        /// <summary>
        /// Whether the rectangle contains the given rectangle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(ref Rectangle value)
        {
            return X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
        }

        /// <summary>
        /// Whether the rectangle intersects with the other rectangle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref Rectangle r2)
        {
            return r2.Left < Right &&
                   Left < r2.Right &&
                   r2.Top < Bottom &&
                   Top < r2.Bottom;
        }

        /// <summary>
        /// Whether the rectangle intersects with the other rectangle inclusively.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsInclusive(ref Rectangle r2)
        {
            return r2.Left <= Right &&
                   Left <= r2.Right &&
                   r2.Top <= Bottom &&
                   Top <= r2.Bottom;
        }

        /// <summary>
        /// Increase the size of the rectangle outward.
        /// </summary>
        public Rectangle Inflate(float horizontalValue, float verticalValue)
        {
            X -= horizontalValue;
            Y -= verticalValue;
            Width += horizontalValue * 2;
            Height += verticalValue * 2;

            return this;
        }

        /// <summary>
        /// Reduce the size of the rectangle inward.
        /// </summary>
        public Rectangle Deflate(float horizontalValue, float verticalValue)
        {
            X += horizontalValue;
            Y += verticalValue;
            Width -= horizontalValue * 2;
            Height -= verticalValue * 2;

            return this;
        }

        /// <summary>
        /// Find the closest point of intersection between the ray and the rectangle's surfaces,
        /// and the distance. If the ray doesn't intersect with the rect Vector2.Zero is returned.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionPoint(ref Ray2D ray)
        {
            return GetIntersectionPointAndDistance(ref ray, out float _);
        }

        /// <summary>
        /// Find the closest point of intersection between the ray and the rectangle's surfaces,
        /// and the distance. If the ray doesn't intersect with the rect Vector2.Zero is returned.
        /// </summary>
        public Vector2 GetIntersectionPointAndDistance(ref Ray2D ray, out float distance)
        {
            Vector2 closestPoint = Vector2.Zero;
            distance = float.MaxValue;
            Span<LineSegment> surfaces = stackalloc LineSegment[4];
            GetLineSegments(surfaces);
            for (var i = 0; i < surfaces.Length; i++)
            {
                Vector2 surfacePoint = surfaces[i].GetIntersectionPointAndDistance(ref ray, out float dist);
                if (surfacePoint == Vector2.Zero || !(dist < distance)) continue;
                distance = dist;
                closestPoint = surfacePoint;
            }

            return closestPoint;
        }

        /// <summary>
        /// Snap the rectangle to a grid.
        /// </summary>
        public void SnapToGrid(Vector2 gridSize)
        {
            Vector2 p = (Position / gridSize).Floor();
            Vector2 s = ((Position + Size) / gridSize).Ceiling();
            Position = p * gridSize;
            Size = (s - p) * gridSize;
        }

        /// <summary>
        /// Get a line which goes through the center of the rectangle to it's two smaller sides.
        /// </summary>
        /// <param name="vertical">Whether the line is vertical.</param>
        /// <returns></returns>
        public LineSegment GetCenterLine(out bool vertical)
        {
            if (Height > Width)
            {
                vertical = true;
                var p1 = new Vector2(X + Width / 2, Y);
                var p2 = new Vector2(p1.X, Y + Height);
                return new LineSegment(p1, p2);
            }

            vertical = false;
            var p1W = new Vector2(X, Y + Height / 2);
            var p2W = new Vector2(X + Width, p1W.Y);
            return new LineSegment(p1W, p2W);
        }

        /// <summary>
        /// Return a rectangle identical to this one, but offset by the given amount.
        /// </summary>
        public Rectangle Offset(Vector2 position)
        {
            return new Rectangle(X + position.X, Y + position.Y, Width, Height);
        }

        /// <summary>
        /// Scales the rect by the provided vector.
        /// X and Width and multiplied by the X component, and Y and Height by the Y.
        /// </summary>
        public Rectangle Scale(Vector2 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Width *= scale.X;
            Height *= scale.Y;
            return this;
        }

        public Rectangle Divide(Vector2 scale)
        {
            X /= scale.X;
            Y /= scale.Y;
            Width /= scale.X;
            Height /= scale.Y;
            return this;
        }

        #region NEZ Extensions

        // Taken from Nez and Modified
        // MIT License
        // https://github.com/prime31/Nez

        /// <summary>
        /// Get a rectangle representing half of the rectangle - cut from a certain side.
        /// </summary>
        /// <param name="rectSide"></param>
        /// <returns></returns>
        public Rectangle GetHalfRect(RectSide rectSide)
        {
            return rectSide switch
            {
                RectSide.Top => new Rectangle(X, Y, Width, Height / 2),
                RectSide.Bottom => new Rectangle(X, Y + Height / 2, Width, Height / 2),
                RectSide.Left => new Rectangle(X, Y, Width / 2, Height),
                RectSide.Right => new Rectangle(X + Width / 2, Y, Width / 2, Height),
                _ => throw new ArgumentOutOfRangeException()
            };
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
            return rectSide switch
            {
                RectSide.Top => new Rectangle(X, Y, Width, size),
                RectSide.Bottom => new Rectangle(X, Y + Height - size, Width, size),
                RectSide.Left => new Rectangle(X, Y, size, Height),
                RectSide.Right => new Rectangle(X + Width - size, Y, size, Height),
                _ => throw new ArgumentOutOfRangeException()
            };
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
            if (value1 == Empty) return value2;
            if (value2 == Empty) return value1;

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
        /// Creates a Rectangle given min/max points (top-left, bottom-right points)
        /// </summary>
        public static Rectangle FromMinMaxPoints(Vector2 min, Vector2 max)
        {
            return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        /// <summary>
        /// Creates a Rectangle given min/max points (top-left, bottom-right points)
        /// </summary>
        public static Rectangle FromMinMaxPointsChecked(Vector2 v1, Vector2 v2)
        {
            Vector2 min = Vector2.Min(v1, v2);
            Vector2 max = Vector2.Max(v1, v2);
            return new Rectangle(min.X, min.Y, max.X - min.X, max.Y - min.Y);
        }

        /// <summary>
        /// Creates a Rectangle given min/max points (top-left, bottom-right points)
        /// </summary>
        public static Rectangle FromMinMaxPoints(float minX, float minY, float maxX, float maxY)
        {
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
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
        /// moveX and moveY will return the movement that this rect must move to avoid the collision
        /// </summary>
        /// <param name="other">Other.</param>
        /// <param name="moveX">Move x.</param>
        /// <param name="moveY">Move y.</param>
        public bool CollisionCheck(ref Rectangle other, out float moveX, out float moveY)
        {
            moveX = moveY = 0.0f;

            // check that there was a collision
            if (!Intersects(ref other))
                return false;

            float l = other.X - (X + Width);
            float r = other.X + other.Width - X;
            float t = other.Y - (Y + Height);
            float b = other.Y + other.Height - Y;

            // find the offset of both sides
            moveX = Math.Abs(l) < r ? l : r;
            moveY = Math.Abs(t) < b ? t : b;

            // only use whichever offset is the smallest
            if (Math.Abs(moveX) < Math.Abs(moveY))
            {
                if (moveX != 0)
                    moveY = 0.0f;
            }
            else
            {
                if (moveY != 0)
                    moveX = 0.0f;
            }

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
        public Vector2 GetIntersectionDepth(ref Rectangle r2)
        {
            // calculate half sizes
            float halfWidthA = Width / 2.0f;
            float halfHeightA = Height / 2.0f;
            float halfWidthB = r2.Width / 2.0f;
            float halfHeightB = r2.Height / 2.0f;

            // calculate centers
            Vector2 centerA = Center;
            Vector2 centerB = r2.Center;

            // calculate current and minimum-non-intersecting distances between centers
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // if we are not intersecting at all, return (0, 0)
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return Vector2.Zero;

            // calculate and return intersection depths
            float depthX = distanceX == 0 ? 0 : distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY == 0 ? 0 : distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

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

        /// <summary>
        /// Clamp another rectangle to fit in this one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Rectangle ClampTo(Rectangle other)
        {
            if (other.Contains(this)) return this;

            if (X < other.X) X = other.X;
            if (Y < other.Y) Y = other.Y;
            if (Right > other.Right) X = Right - Width;
            if (Bottom > other.Bottom) Y = other.Bottom - Height;

            return this;
        }

        /// <summary>
        /// Transform the four points of a rectangle by a matrix.
        /// </summary>
        public static Rectangle Transform(Rectangle rect, Matrix4x4 matrix)
        {
            Vector2 p1 = Vector2.Transform(rect.TopLeft, matrix);
            Vector2 p2 = Vector2.Transform(rect.TopRight, matrix);
            Vector2 p3 = Vector2.Transform(rect.BottomRight, matrix);
            Vector2 p4 = Vector2.Transform(rect.BottomLeft, matrix);

            unsafe
            {
                Vector2** vertices = stackalloc Vector2*[4]
                {
                    &p1,
                    &p2,
                    &p3,
                    &p4
                };

                var minX = float.MaxValue;
                var maxX = float.MinValue;
                var minY = float.MaxValue;
                var maxY = float.MinValue;

                for (var i = 0; i < 4; i++)
                {
                    float x = vertices[i]->X;
                    float y = vertices[i]->Y;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }

                float width = maxX - minX;
                float height = maxY - minY;

                return new Rectangle(minX, minY, width, height);
            }
        }

        #endregion

        #region Overloads

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Rectangle r2)
        {
            return Intersects(ref r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsInclusive(Rectangle r2)
        {
            return IntersectsInclusive(ref r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 GetIntersectionDepth(Rectangle r2)
        {
            return GetIntersectionDepth(ref r2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Vector2 value)
        {
            return Contains(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(Vector2 value)
        {
            return ContainsInclusive(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Rectangle value)
        {
            return Contains(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsInclusive(Rectangle value)
        {
            return ContainsInclusive(ref value);
        }

        [Obsolete("Did you mean Rectangle.Contains(Vector2)?")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(Vector2 value)
        {
            return Contains(ref value);
        }

        [Obsolete("Did you mean Rectangle.Contains(ref Vector2)?")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref Vector2 value)
        {
            return Contains(ref value);
        }

        #endregion
    }
}