// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Platform.Base.Assets;
using Emotion.Platform.Base.Interfaces;
using Emotion.Primitives;

#endregion

namespace Emotion.Platform.Base.Objects
{
    /// <summary>
    /// A text drawing session. Used as an object holder by the Renderer when rendering text.
    /// </summary>
    public abstract class TextDrawingSession : Destroyable
    {
        public Vector2 Size { get; internal set; }

        /// <summary>
        /// Add a glyph to a text rendering session. The glyph is automatically positioned to be after the previous one, but can be
        /// additionally offset.
        /// </summary>
        /// <param name="glyphChar">The glyph to add.</param>
        /// <param name="color">The glyph color.</param>
        /// <param name="xOffset">The x offset to render the glyph at.</param>
        /// <param name="yOffset">The y offset to render the glyph at.</param>
        public abstract void AddGlyph(char glyphChar, Color color, int xOffset = 0, int yOffset = 0);

        /// <summary>
        /// Add a new line to the text rendering session.
        /// </summary>
        public abstract void NewLine();

        /// <summary>
        /// Return the state of the session as a texture. Any old states returned are automatically freed from memory.
        /// </summary>
        /// <returns>A texture state of the text drawing session.</returns>
        public abstract Texture GetTexture();

        /// <summary>
        /// Reset the text drawing session, clearing any exported results, and the current state.
        /// </summary>
        public abstract void Reset();
    }
}