// Emotion - https://github.com/Cryru/Emotion

using System;
using System.IO;
using System.Reflection;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

namespace Emotion.Utils
{
    public static class Helpers
    {
        /// <summary>
        /// Safely parses the text to an int. If the parse fails returns a default value.
        /// </summary>
        /// <param name="text">The text to parse to an int.</param>
        /// <param name="invalidValue">The value to return if the parsing fails. 0 by default.</param>
        /// <returns>The text parsed as an int, or a default value.</returns>
        public static int SafeIntParse(string text, int invalidValue = 0)
        {
            bool parsed = int.TryParse(text, out int result);
            return parsed ? result : invalidValue;
        }

        /// <summary>
        /// Returns the bounds of a frame within a spritesheet texture.
        /// </summary>
        /// <param name="textureSize">The size of the spritesheet texture.</param>
        /// <param name="frameSize">The size of individual frames.</param>
        /// <param name="spacing">The spacing between frames.</param>
        /// <param name="frameId">The index of the frame we are looking for. 0 based.</param>
        /// <returns>The bounds of a frame within a spritesheet texture.</returns>
        public static Rectangle GetFrameBounds(Vector2 textureSize, Vector2 frameSize, Vector2 spacing, int frameId)
        {
            // Get the total number of columns.
            int columns = (int)(textureSize.X / frameSize.X);

            // Get the current row and column.
            int row = (int)(frameId / (float)columns);
            int column = frameId % columns;

            // Find the frame we are looking for.
            return new Rectangle((int)(frameSize.X * column + spacing.X * (column + 1)),
                (int)(frameSize.Y * row + spacing.Y * (row + 1)), (int)frameSize.X, (int)frameSize.Y);
        }

        /// <summary>
        /// Returns the contents of an embedded resource file.
        /// </summary>
        /// <param name="path">Path to the embedded resource file.</param>
        /// <returns>A string representation of the read file.</returns>
        public static string ReadEmbeddedResource(string path)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Check for an OpenGL error.
        /// </summary>
        /// <param name="location">Where the error check is.</param>
        public static void CheckError(string location)
        {
            ErrorCode errorCheck = GL.GetError();
            if (errorCheck != ErrorCode.NoError) throw new Exception("OpenGL error at " + location + ":\n" + errorCheck);
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
        /// The global generator used for generating numbers.
        /// </summary>
        private static Random _generator = new Random();

        /// <summary>
        /// Returns a randomly generated number from a global seed.
        /// </summary>
        /// <param name="min">The lowest number that can be generated included.</param>
        /// <param name="max">The highest number that can be generated included.</param>
        /// <returns></returns>
        public static int GetRandom(int min = 0, int max = 100)
        {
            // Add one because by Random.Next does not include max.
            return _generator.Next(min, max + 1);
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
    }
}