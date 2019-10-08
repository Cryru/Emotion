#region Using

using System.Numerics;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Extension functionality of primitives.
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
    }
}