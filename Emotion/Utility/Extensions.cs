#region Using

using System.Numerics;
using Emotion.Primitives;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Extension functionality of other classes not within Emotion.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns an inverted copy of this instance.
        /// </summary>
        public static Matrix4x4 Inverted(this Matrix4x4 m)
        {
            Matrix4x4.Invert(m, out Matrix4x4 inv);
            return inv;
        }

        /// <summary>
        /// Converts a vector3 to a vector2.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector3 as a Vector2, with the Z unit stripped.</returns>
        public static Vector2 ToVec2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a vector4 to a vector3.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>The Vector4 as a Vector3, with the W unit stripped.</returns>
        public static Vector3 ToVec3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}