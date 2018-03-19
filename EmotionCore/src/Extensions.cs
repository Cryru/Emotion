// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Drawing;

#endregion

namespace Emotion
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the center of a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to return the center of.</param>
        /// <returns>The center of the rectangle.</returns>
        public static Point GetCenter(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
    }
}