// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Platform.Base.Interfaces;
using Emotion.Primitives;

#endregion

namespace Emotion.Platform.Base.Assets
{
    public abstract class Font : IDestroyable
    {
        /// <summary>
        /// Returns the size in pixels the string would use at the specified font size.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="size">The font size to use to measure.</param>
        /// <returns>The size in pixels the string would use at the specified font size.</returns>
        public abstract Vector2 MeasureString(string text, int size);

        /// <summary>
        /// Returns the distance between text lines.
        /// </summary>
        /// <param name="size">The font size to use to measure.</param>
        /// <returns>The distance between text lines.</returns>
        public abstract int LineSpacing(int size);

        public abstract void Destroy();
    }
}