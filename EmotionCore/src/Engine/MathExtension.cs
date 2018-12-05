// Emotion - https://github.com/Cryru/Emotion

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

        /// <summary>
        /// Performs a Hermite spline interpolation using the specified floats.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="tangent1"></param>
        /// <param name="value2"></param>
        /// <param name="tangent2"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
        {
            // All transformed to double not to lose precision
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            switch (amount)
            {
                case 0f:
                    result = value1;
                    break;
                case 1f:
                    result = value2;
                    break;
                default:
                    result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                             (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                             t1 * s +
                             v1;
                    break;
            }

            return (float) result;
        }

        /// <summary>
        /// Smoothly interpolates between two values.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static float SmoothStep(float value1, float value2, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2

            float result = Clamp(amount, 0f, 1f);
            result = Hermite(value1, 0f, value2, 0f, result);

            return result;
        }
    }
}