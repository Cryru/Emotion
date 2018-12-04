// Emotion - https://github.com/Cryru/Emotion

using System.Numerics;

namespace Emotion.Engine
{
    /// <summary>
    /// Math related helper functions.
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// Clamp a value between two values.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum the value can be.</param>
        /// <param name="max">The maximum the value can be.</param>
        /// <returns>The value clamped.</returns>
        public static float Clamp(float value, float min, float max)
        {
            // If the value is higher than the max, return max.
            if (value > max) return max;
            // If the value is lower than the min, return min, otherwise return value.
            return value < min ? min : value;
        }

        /// <summary>
        /// The linear blend of two floats.
        /// </summary>
        /// <param name="a">First input float. The start.</param>
        /// <param name="b">Second input float. The end.</param>
        /// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
        /// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
        public static float Lerp(float a, float b, float blend)
        {
            return a + (b - a) * blend;
        }
    }
}