// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Utils
{
    public static class Helpers
    {
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
            int columns = (int) (textureSize.X / frameSize.X);

            // Get the current row and column.
            int row = (int) (frameId / (float) columns);
            int column = frameId % columns;

            // Find the frame we are looking for.
            return new Rectangle((int) (frameSize.X * column + spacing.X * (column + 1)),
                (int) (frameSize.Y * row + spacing.Y * (row + 1)), (int) frameSize.X, (int) frameSize.Y);
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

        public static float PixelFontSizeToCharSize(uint pixelSize)
        {
            return (float) Math.Pow(96 / 72 / pixelSize, -1);
        }
    }
}