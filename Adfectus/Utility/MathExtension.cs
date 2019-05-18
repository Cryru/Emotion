#region Using

using System;

#endregion

namespace Adfectus.Utility
{
    /// <summary>
    /// Math related helper functions.
    /// Taken from various projects around the internet.
    /// Most are from MonoGame
    /// https://github.com/ManojLakshan/monogame/blob/master/MonoGame.Framework/MathHelper.cs
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// The natural logarithmic base.
        /// </summary>
        public const float E = (float) Math.E;

        /// <summary>
        /// Logarithm of 10E.
        /// </summary>
        public const float Log10E = 0.4342945f;

        /// <summary>
        /// Logarithm of 2E.
        /// </summary>
        public const float Log2E = 1.442695f;

        /// <summary>
        /// The mathematical constant Pi.
        /// </summary>
        public const float Pi = (float) Math.PI;

        /// <summary>
        /// The mathematical constant Pi - divided by two.
        /// </summary>
        public const float PiOver2 = (float) (Math.PI / 2.0);

        /// <summary>
        /// The mathematical constant Pi - divided by four.
        /// </summary>
        public const float PiOver4 = (float) (Math.PI / 4.0);

        /// <summary>
        /// The mathematical constant Pi - multiplied by two.
        /// </summary>
        public const float TwoPi = (float) (Math.PI * 2.0);

        public const float Deg2Rad = 0.0174532924f;
        public const float Rad2Deg = 57.29578f;

        /// <summary>
        /// Barycentric interpolation.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="amount1"></param>
        /// <param name="amount2"></param>
        /// <returns></returns>
        public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
        {
            return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
        }

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

        /// <summary>
        /// Returns the next number which is a power of 2.
        /// </summary>
        /// <param name="num">The number to find.</param>
        /// <returns>The closest power of 2 to num</returns>
        public static int NextP2(int num)
        {
            int temp = 1;
            while (temp < num) temp <<= 1;
            return temp;
        }

        /// <summary>
        /// Converts the angle in degrees to radians up to two decimals.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The degrees in radians.</returns>
        public static float DegreesToRadians(float angle)
        {
            // Divide Pi by 180 and multiply by the angle, round up to two decimals.
            return angle * Deg2Rad;
        }

        /// <summary>
        /// Converts the radians to angles.
        /// </summary>
        /// <param name="radian">Angle in radians.</param>
        /// <returns>The radians in degrees</returns>
        public static float RadiansToDegrees(float radian)
        {
            // Divide 180 by Pi and multiply by the radians. Convert to an integer.
            return radian * Rad2Deg;
        }
    }
}