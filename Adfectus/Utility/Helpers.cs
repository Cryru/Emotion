#region Using

using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

namespace Adfectus.Utility
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
        /// Calculate the aspect ratio.
        /// </summary>
        /// <param name="width">The x resolution.</param>
        /// <param name="height">The y resolution.</param>
        /// <returns>Returns the aspect ratio in format in x:y.</returns>
        public static string GetAspectRatio(float width, float height)
        {
            int gcd = 1;

            for (int i = 1; i <= width && i <= height; ++i)
            {
                // Checks if i is factor of both integers
                if (width % i == 0 && height % i == 0)
                    gcd = i;
            }

            return $"{width / gcd}:{height / gcd}";
        }
    }
}