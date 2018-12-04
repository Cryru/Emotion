// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;

#endregion

namespace Emotion.Primitives
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
    }
}