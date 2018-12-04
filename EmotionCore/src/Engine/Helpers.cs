// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Engine
{
    /// <summary>
    /// General helper functions.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// The size of a Vector2 struct in bytes.
        /// </summary>
        public static int Vector2SizeInBytes = Marshal.SizeOf(new Vector2());

        /// <summary>
        /// The size of a Vector3 struct in bytes.
        /// </summary>
        public static int Vector3SizeInBytes = Marshal.SizeOf(new Vector3());

        /// <summary>
        /// Safely parses the text to an int. If the parse fails returns a default value.
        /// </summary>
        /// <param name="text">The text to parse to an int.</param>
        /// <param name="invalidValue">The value to return if the parsing fails. 0 by default.</param>
        /// <returns>The text parsed as an int, or a default value.</returns>
        public static int StringToInt(string text, int invalidValue = 0)
        {
            bool parsed = int.TryParse(text, out int result);
            return parsed ? result : invalidValue;
        }

        /// <summary>
        /// The generator to be used for generating randomness.
        /// </summary>
        private static Random _generator = new Random();

        /// <summary>
        /// Returns a randomly generated number within specified constraints.
        /// </summary>
        /// <param name="min">The lowest number that can be generated.</param>
        /// <param name="max">The highest number that can be generated.</param>
        /// <returns></returns>
        public static int GenerateRandomNumber(int min = 0, int max = 100)
        {
            //We add one because Random.Next does not include max.
            return _generator.Next(min, max + 1);
        }

        /// <summary>
        /// Returns the contents of an embedded resource file.
        /// </summary>
        /// <param name="path">Path to the embedded resource file.</param>
        /// <returns>A string representation of the read file.</returns>
        public static string ReadEmbeddedResource(string path)
        {
            using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream(path))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Converts a path to the platform equivalent on the currently running platform.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A cross platform path.</returns>
        public static string CrossPlatformPath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', Path.DirectorySeparatorChar);
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
        public static float DegreesToRadians(int angle)
        {
            // Divide Pi by 180 and multiply by the angle, round up to two decimals.
            return (float) Math.Round(Math.PI / 180 * angle, 2);
        }

        /// <summary>
        /// Converts the radians to angles.
        /// </summary>
        /// <param name="radian">Angle in radians.</param>
        /// <returns>The radians in degrees</returns>
        public static int RadiansToDegrees(float radian)
        {
            // Divide 180 by Pi and multiply by the radians. Convert to an integer.
            return (int) (180 / (float) Math.PI * radian);
        }
    }
}