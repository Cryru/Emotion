#region Using

using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    public interface IShape
    {
        /// <summary>
        /// The center of the shape.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        /// Return a copy of this shape.
        /// </summary>
        IShape CloneShape();

        /// <summary>
        /// Whether the shape intersects with the line.
        /// </summary>
        bool Intersects(ref LineSegment line);

        /// <summary>
        /// The point of intersection between the shape and a line segment.
        /// </summary>
        Vector2 GetIntersectionPoint(ref LineSegment line);

        /// <summary>
        /// Whether the shape contains the specified point.
        /// </summary>
        bool Contains(ref Vector2 point);

        /// <summary>
        /// Whether the shape contains the specified point.
        /// </summary>
        bool ContainsInclusive(ref Vector2 point);

        /// <summary>
        /// Whether the shape contains the specified rectangle.
        /// </summary>
        bool Contains(ref Rectangle rect);

        /// <summary>
        /// Whether the shape contains the specified rectangle.
        /// </summary>
        bool ContainsInclusive(ref Rectangle rect);

        /// <summary>
        /// Whether the shape intersects with the specified rectangle.
        /// </summary>
        bool Intersects(ref Rectangle rect);
    }
}