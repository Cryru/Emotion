#region Using

using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A struct representing a ray.
    /// </summary>
    public struct Ray2D
    {
        public Vector2 Start;
        public Vector2 Direction;

        /// <summary>
        /// Create a ray from a starting position, direction and length.
        /// </summary>
        /// <param name="start">The ray's start.</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray2D(Vector2 start, Vector2 direction)
        {
            Start = start;
            Direction = direction;
        }
    }
}