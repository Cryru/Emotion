using System.Numerics;

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
    }
}