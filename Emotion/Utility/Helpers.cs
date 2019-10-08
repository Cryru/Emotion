#region Using

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#endregion

namespace Emotion.Utility
{
    public static class Helpers
    {
        /// <summary>
        /// Regex for capturing Windows line endings.
        /// </summary>
        private static Regex _newlineRegex = new Regex("\r\n");

        /// <summary>
        /// Replaces windows new lines with unix new lines.
        /// </summary>
        public static string NormalizeNewLines(string source)
        {
            return _newlineRegex.Replace(source, "\n");
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
        /// Converts the string to one which is safe for use in the file system.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A string safe to use in the file system.</returns>
        public static string MakeStringPathSafe(string str)
        {
            return Path.GetInvalidPathChars().Aggregate(str, (current, c) => current.Replace(c, ' '));
        }

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
    }
}