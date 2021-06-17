#region Using

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Primitives;

#endregion

namespace Emotion.Utility
{
    /// <summary>
    /// Math formulas and helpers.
    /// </summary>
    public static class Maths
    {
        /// <summary>
        /// The natural logarithmic base.
        /// </summary>
        public const float E = (float) Math.E;

        /// <summary>
        /// Logarithm of 10E.
        /// </summary>
        public const float LOG10_E = 0.4342945f;

        /// <summary>
        /// Logarithm of 2E.
        /// </summary>
        public const float LOG2_E = 1.442695f;

        /// <summary>
        /// The mathematical constant Pi.
        /// </summary>
        public const float PI = (float) Math.PI;

        /// <summary>
        /// The mathematical constant Pi - divided by two.
        /// </summary>
        public const float PI_OVER2 = (float) (Math.PI / 2.0);

        /// <summary>
        /// The mathematical constant Pi - divided by four.
        /// </summary>
        public const float PI_OVER4 = (float) (Math.PI / 4.0);

        /// <summary>
        /// The mathematical constant Pi - multiplied by two.
        /// </summary>
        public const float TWO_PI = (float) (Math.PI * 2.0);

        /// <summary>
        /// The mathematical constant Pi - as a double.
        /// </summary>
        public const double PI_DOUBLE = Math.PI;

        /// <summary>
        /// The mathematical constant Pi - as a double, multiplied by 2.
        /// </summary>
        public const double TWO_PI_DOUBLE = Math.PI * 2;

        /// <summary>
        /// Constant for converting degrees to radians.
        /// </summary>
        public const float DEG2_RAD = 0.0174532924f;

        /// <summary>
        /// Constant for converting radians to degrees.
        /// </summary>
        public const float RAD_2DEG = 57.29578f;

        /// <summary>
        /// A positive infinitesimal quantity. 1e-7
        /// </summary>
        public const float EPSILON = 0.0000001f;

        /// <summary>
        /// Epsilon times two.
        /// </summary>
        public const float EPSILON_2 = EPSILON * 2;

        /// <summary>
        /// A bigger positive infinitesimal quantity. 1e-4
        /// </summary>
        public const float EPSILON_BIGGER = 0.0001f;

        /// <summary>
        /// Bigger epsilon times two.
        /// </summary>
        public const float EPSILON_BIGGER_2 = EPSILON_BIGGER * 2;

        /// <summary>
        /// Matrix for flipping the X axis.
        /// </summary>
        public static Matrix4x4 FlipMatX = Matrix4x4.CreateScale(-1, 1, 1);

        /// <summary>
        /// Matrix for flipping the Y axis.
        /// </summary>
        public static Matrix4x4 FlipMatY = Matrix4x4.CreateScale(1, -1, 1);

        /// <summary>
        /// The four 2d directions.
        /// </summary>
        public static Vector2[] CardinalDirections2D =
        {
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1)
        };

        /// <summary>
        /// The four 2d diagonals.
        /// </summary>
        public static Vector2[] Diagonals2D =
        {
            new Vector2(-1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1)
        };

        /// <summary>
        /// The four 2d directions and the four diagonals.
        /// </summary>
        public static Vector2[] CardinalDirectionsAndDiagonals2D = Extensions.JoinArrays(CardinalDirections2D, Diagonals2D);

        /// <summary>
        /// Ceiling round the float to the nearest int value above y. note that this only works for values in the range of short.
        /// </summary>
        /// <returns>The ceil to int.</returns>
        /// <param name="y">F.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastCeilToInt(float y)
        {
            return 32768 - (int) (32768f - y);
        }

        /// <summary>
        /// Floors the float to the nearest int value below x. note that this only works for values in the range of short.
        /// </summary>
        /// <returns>The floor to int.</returns>
        /// <param name="x">The x coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FastFloorToInt(float x)
        {
            // We shift to guaranteed positive before casting then shift back after
            return (int) (x + 32768f) - 32768;
        }

        /// <summary>
        /// Clamps the value between 0 and 1.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;

            return value > 1f ? 1f : value;
        }

        /// <summary>
        /// Clamp a value between two values.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum the value can be.</param>
        /// <param name="max">The maximum the value can be.</param>
        /// <returns>The value clamped.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            return value > max ? max : value;
        }

        /// <summary>
        /// Clamps a value between the two provided values.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The lower bound to clamp to.</param>
        /// <param name="max">The upper bound to clamp to.</param>
        /// <returns>The clamped value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            return value > max ? max : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Snap(float value, float increment)
        {
            return MathF.Round(value / increment) * increment;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Snap(float value, float increment, float offset)
        {
            return MathF.Round((value - offset) / increment) * increment + offset;
        }

        /// <summary>
        /// The linear blend of two floats.
        /// </summary>
        /// <param name="from">First input float. The start.</param>
        /// <param name="to">Second input float. The end.</param>
        /// <param name="t">The blend or time factor between the two values. Clamped.</param>
        /// <returns>The linear interpolation between the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * Clamp01(t);
        }

        /// <summary>
        /// Produces the inverse result of Lerp.
        /// </summary>
        /// <param name="from">First input float. The start.</param>
        /// <param name="to">Second input float. The end.</param>
        /// <param name="t">The blend or time factor between the two values. Clamped.</param>
        /// <returns>The inverse linear interpolation between the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float from, float to, float t)
        {
            t = Clamp01(t);
            if (from < to)
            {
                if (t < from)
                    return 0.0f;
                if (t > to)
                    return 1.0f;
            }
            else
            {
                if (t < to)
                    return 1.0f;
                if (t > from)
                    return 0.0f;
            }

            return (t - from) / (to - from);
        }

        /// <summary>
        /// Lerp without clamping the time.
        /// </summary>
        /// <param name="from">First input float. The start.</param>
        /// <param name="to">Second input float. The end.</param>
        /// <param name="t">The blend or time factor between the two values. Clamped.</param>
        /// <returns>The linear interpolation between the two values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FastLerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        /// <summary>
        /// Lerps an angle in degrees between a and b. handles wrapping around 360
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="t">T.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpAngle(float a, float b, float t)
        {
            float num = Repeat(b - a, 360f);
            if (num > 180f)
                num -= 360f;

            return a + num * Clamp01(t);
        }

        /// <summary>
        /// Lerps an angle in radians between a and b. handles wrapping around 2*Pi
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="t">T.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpAngleRadians(float a, float b, float t)
        {
            float num = Repeat(b - a, TWO_PI);
            if (num > PI)
                num -= TWO_PI;

            return a + num * Clamp01(t);
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

            result = amount switch
            {
                0f => value1,
                1f => value2,
                _ => (2 * v1 - 2 * v2 + t2 + t1) * sCubed + (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared + t1 * s + v1
            };

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
        /// SmoothStep all components of the vector.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="interpolation"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothStep(Vector3 start, Vector3 end, float interpolation)
        {
            float x = SmoothStep(start.X, end.X, interpolation);
            float y = SmoothStep(start.Y, end.Y, interpolation);
            float z = SmoothStep(start.Z, end.Z, interpolation);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// loops t so that it is never larger than length and never smaller than 0
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="length">Length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(float t, float length)
        {
            return t - MathF.Floor(t / length) * length;
        }


        /// <summary>
        /// increments t and ensures it is always greater than or equal to 0 and less than length
        /// </summary>
        /// <param name="t">T.</param>
        /// <param name="length">Length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IncrementWithWrap(int t, int length)
        {
            t++;
            return t == length ? 0 : t;
        }


        /// <summary>
        /// decrements t and ensures it is always greater than or equal to 0 and less than length
        /// </summary>
        /// <returns>The with wrap.</returns>
        /// <param name="t">T.</param>
        /// <param name="length">Length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecrementWithWrap(int t, int length)
        {
            t--;
            if (t < 0)
                return length - 1;
            return t;
        }


        /// <summary>
        /// ping-pongs t so that it is never larger than length and never smaller than 0
        /// </summary>
        /// <returns>The pong.</returns>
        /// <param name="t">T.</param>
        /// <param name="length">Length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PingPong(float t, float length)
        {
            t = Repeat(t, length * 2f);
            return length - Math.Abs(t - length);
        }


        /// <summary>
        /// if value >= threshold returns its sign else returns 0
        /// </summary>
        /// <returns>The threshold.</returns>
        /// <param name="value">Value.</param>
        /// <param name="threshold">Threshold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignThreshold(float value, float threshold)
        {
            return MathF.Abs(value) >= threshold ? MathF.Sign(value) : 0;
        }

        /// <summary>
        /// Calculates the shortest difference between two given angles in degrees
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="current">Current.</param>
        /// <param name="target">Target.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaAngle(float current, float target)
        {
            float num = Repeat(target - current, 360f);
            if (num > 180f)
                num -= 360f;

            return num;
        }

        /// <summary>
        /// Calculates the shortest difference between two given angles given in radians
        /// </summary>
        /// <returns>The angle.</returns>
        /// <param name="current">Current.</param>
        /// <param name="target">Target.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaAngleRadians(float current, float target)
        {
            float num = Repeat(target - current, 2 * PI);
            if (num > PI)
                num -= 2 * PI;

            return num;
        }

        /// <summary>
        /// moves start towards end by shift amount clamping the result. start can be less than or greater than end.
        /// example: start is 2, end is 10, shift is 4 results in 6
        /// </summary>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <param name="shift">Shift.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Approach(float start, float end, float shift)
        {
            return start < end ? MathF.Min(start + shift, end) : MathF.Max(start - shift, end);
        }

        /// <summary>
        /// moves start angle towards end angle by shift amount clamping the result and choosing the shortest path. start can be
        /// less than or greater than end.
        /// example 1: start is 30, end is 100, shift is 25 results in 55
        /// example 2: start is 340, end is 30, shift is 25 results in 5 (365 is wrapped to 5)
        /// </summary>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <param name="shift">Shift.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ApproachAngle(float start, float end, float shift)
        {
            float deltaAngle = DeltaAngle(start, end);
            if (-shift < deltaAngle && deltaAngle < shift)
                return end;
            return Repeat(Approach(start, start + deltaAngle, shift), 360f);
        }

        /// <summary>
        /// moves start angle towards end angle by shift amount (all in radians) clamping the result and choosing the shortest
        /// path. start can be less than or greater than end.
        /// this method works very similar to approachAngle, the only difference is use of radians instead of degrees and wrapping
        /// at 2*Pi instead of 360.
        /// </summary>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <param name="shift">Shift.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ApproachAngleRadians(float start, float end, float shift)
        {
            float deltaAngleRadians = DeltaAngleRadians(start, end);
            if (-shift < deltaAngleRadians && deltaAngleRadians < shift)
                return end;
            return Repeat(Approach(start, start + deltaAngleRadians, shift), TWO_PI);
        }

        /// <summary>
        /// checks to see if two values are approximately the same using an acceptable tolerance for the check
        /// </summary>
        /// <param name="value1">Value1.</param>
        /// <param name="value2">Value2.</param>
        /// <param name="tolerance">Tolerance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Approximately(float value1, float value2, float tolerance = EPSILON)
        {
            return MathF.Abs(value1 - value2) <= tolerance;
        }

        /// <summary>
        /// Returns the minimum of the passed in values
        /// </summary>
        /// <param name="values">The values to check between.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MinOf(params float[] values)
        {
            float smallest = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                smallest = MathF.Min(smallest, values[i]);
            }

            return smallest;
        }

        /// <summary>
        /// Returns the minimum of the passed in values
        /// </summary>
        /// <param name="values">The values to check between.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MaxOf(params float[] values)
        {
            float smallest = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                smallest = MathF.Max(smallest, values[i]);
            }

            return smallest;
        }

        /// <summary>
        /// checks to see if value is between min/max inclusive of min/max
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// checks to see if value is between min/max inclusive of min/max
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Between(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// rounds value and returns it and the amount that was rounded
        /// </summary>
        /// <returns>The with remainder.</returns>
        /// <param name="value">Value.</param>
        /// <param name="roundedAmount">roundedAmount.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundWithRoundedAmount(float value, out float roundedAmount)
        {
            float rounded = MathF.Round(value);
            roundedAmount = value - rounded * MathF.Round(value / rounded);
            return rounded;
        }

        /// <summary>
        /// Maps a value from some arbitrary range to the 0 to 1 range.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Normalize(float value, float min, float max)
        {
            return (value - min) * 1f / (max - min);
        }

        /// <summary>
        /// Maps a value from some arbitrary range to the 1 to 0 range, this is just the reverse of map01
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="min">Minimum value.</param>
        /// <param name="max">Maximum value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReverseNormalize(float value, float min, float max)
        {
            return 1f - Normalize(value, min, max);
        }

        /// <summary>
        /// Maps value which is in the range leftMin - leftMax to a value in the range rightMin - rightMax
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="leftMin">Left minimum.</param>
        /// <param name="leftMax">Left max.</param>
        /// <param name="rightMin">Right minimum.</param>
        /// <param name="rightMax">Right max.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
        {
            return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
        }

        /// <summary>
        /// rounds value to the nearest number in steps of roundToNearest. Ex: found 127 to nearest 5 results in 125
        /// </summary>
        /// <returns>The to nearest.</returns>
        /// <param name="value">Value.</param>
        /// <param name="roundToNearest">Round to nearest.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundToNearest(float value, float roundToNearest)
        {
            return MathF.Round(value / roundToNearest) * roundToNearest;
        }

        /// <summary>
        /// Round to the closest integer. x.5 and higher will round to 1, anything lower will round to 0.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundClosest(float v)
        {
            return MathF.Floor(v + 0.5f);
        }

        /// <summary>
        /// returns sqrt( x * x + y * y )
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Hypotenuse(float x, float y)
        {
            return MathF.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Returns the next number which is a power of 2.
        /// </summary>
        /// <param name="x">The number to find closest to.</param>
        /// <returns>The closest power of 2 to num</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClosestPowerOfTwoGreaterThan(int x)
        {
            //int temp = 1;
            //while (temp < x) temp <<= 1;
            //return temp;

            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;

            return x + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleBetweenVectors(Vector2 from, Vector2 to)
        {
            return MathF.Atan2(to.Y - from.Y, to.X - from.X);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 AngleToVector(float angleRadians, float length)
        {
            return new Vector2(MathF.Cos(angleRadians) * length, MathF.Sin(angleRadians) * length);
        }

        /// <summary>
        /// The rotation is relative to the current position not the total rotation. For example, if you are currently at 1 Pi
        /// radians and want to rotate to 1.5 Pi radians, you would use an angle of 0.5 Pi, not 1.5 Pi.
        /// </summary>
        /// <returns>The around.</returns>
        /// <param name="point">Point.</param>
        /// <param name="center">Center.</param>
        /// <param name="angleInRadians">Angle in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RotateAround(Vector2 point, Vector2 center, float angleInRadians)
        {
            float cos = MathF.Cos(angleInRadians);
            float sin = MathF.Sin(angleInRadians);
            float rotatedX = cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X;
            float rotatedY = sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y;

            return new Vector2(rotatedX, rotatedY);
        }

        /// <summary>
        /// Gets a point on the circumference of the circle given its center, radius and angle. 0 radians is 3 o'clock.
        /// </summary>
        /// <returns>The on circle.</returns>
        /// <param name="circleCenter">Circle center.</param>
        /// <param name="radius">Radius.</param>
        /// <param name="angleInRadians">Angle in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 PointOnCircleRadians(Vector2 circleCenter, float radius, float angleInRadians)
        {
            return new Vector2
            {
                X = MathF.Cos(angleInRadians) * radius + circleCenter.X,
                Y = MathF.Sin(angleInRadians) * radius + circleCenter.Y
            };
        }

        /// <summary>
        /// Converts the angle in degrees to radians up to two decimals.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The degrees in radians.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DegreesToRadians(float angle)
        {
            // Divide Pi by 180 and multiply by the angle, round up to two decimals.
            return angle * DEG2_RAD;
        }

        /// <summary>
        /// Converts the radians to angles.
        /// </summary>
        /// <param name="radian">Angle in radians.</param>
        /// <returns>The radians in degrees</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadiansToDegrees(float radian)
        {
            // Divide 180 by Pi and multiply by the radians. Convert to an integer.
            return radian * RAD_2DEG;
        }

        /// <summary>
        /// Used for getting unique hash codes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCantorPair(int x, int y)
        {
            return (x + y) * (x + y + 1) / 2 + y;
        }

        /// <summary>
        /// Returns the distance between two points measured along 90 degree angles.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ManhattanDistance(Vector2 start, Vector2 end)
        {
            return MathF.Abs(start.X - end.X) + MathF.Abs(start.Y - end.Y);
        }

        /// <summary>
        /// Get the odd number nearest to the provided number.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetNearestOddNumber(float n)
        {
            if (n % 2 == 0) return n + 1;
            return n;
        }

        /// <summary>
        /// Find the overlap between two one dimensional segments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Get1DIntersectionDepth(float minA, float maxA, float minB, float maxB)
        {
            return Math.Max(0, Math.Min(maxA, maxB) - Math.Max(minA, minB));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AbsSubtract(float vel, float incr = 1.0f)
        {
            return (MathF.Abs(vel) - incr) * MathF.Sign(vel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetPointAlongSegment(Vector2 pointA, Vector2 pointB, float amount01)
        {
            var line = new LineSegment(pointA, pointB);
            return line.PointOnLineAtDistance(line.Length() * amount01);
        }
    }
}