#region Using

using System.Numerics;

#endregion

namespace Emotion.Primitives
{
    /// <summary>
    /// A struct representing a ray.
    /// Taken from Nez and modified (https://github.com/prime31/Nez) covered by the MIT License
    /// While technically not a ray (rays are just start and direction) it does double duty as both a line and a ray.
    /// </summary>
    public struct Ray2D
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }
        public Vector2 Direction { get; set; }

        public bool Finite { get; set; }

        /// <summary>
        /// Create a ray from a starting and ending position.
        /// The direction is inferred from the two points.
        /// </summary>
        /// <param name="start">The position the ray starts.</param>
        /// <param name="end">The position the ray ends.</param>
        public Ray2D(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
            Direction = end - Start;
            Finite = true;
        }

        /// <summary>
        /// Create a ray from a starting position, direction and length.
        /// </summary>
        /// <param name="start">The ray's start.</param>
        /// <param name="direction">The direction of the ray.</param>
        /// <param name="length">The ray's length.</param>
        public Ray2D(Vector2 start, Vector2 direction, float length)
        {
            Start = start;
            End = new Vector2(length * direction.X, length * direction.Y);
            Direction = direction;
            Finite = false;
        }
    }
}