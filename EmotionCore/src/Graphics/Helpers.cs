// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics
{
    public static class Helpers
    {
        /// <summary>
        /// Converts an rectangle structure to an array of vectors.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>An array of vectors.</returns>
        public static Vector2[] RectangleToVertices(Rectangle rect)
        {
            return new[]
            {
                new Vector2(rect.X, rect.Y),
                new Vector2(rect.X + rect.Width, rect.Y),
                new Vector2(rect.X + rect.Width, rect.Y + rect.Height),
                new Vector2(rect.X, rect.Y + rect.Height)
            };
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
    }
}