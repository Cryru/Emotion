// SoulEngine - https://github.com/Cryru/SoulEngine

using System;

namespace Soul.Engine
{
    /// <summary>
    /// Generic helper functions.
    /// </summary>
    public static class Functions
    {
        private static Random _generator = new Random();
        /// <summary>
        /// Generates a random number within the specified range.
        /// </summary>
        /// <param name="min">The smallest the number can be.</param>
        /// <param name="max">The highest the number can be.</param>
        /// <returns></returns>
        public static int GenerateRandomNumber(int min = 0, int max = 100)
        {
            //We add one because by Random.Next does not include max.
            return _generator.Next(min, max + 1);
        }

        /// <summary>
        /// Formats a debug message.
        /// </summary>
        /// <param name="left">Message on the left, usually the source or a code.</param>
        /// <param name="right">Message on the right, usually the message itself.</param>
        /// <returns></returns>
        public static string FormatDebugMessage(string left, string right)
        {
            return "[" + left + "] " + right;
        }
    }
}