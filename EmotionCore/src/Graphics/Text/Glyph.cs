// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Graphics.Text
{
    /// <summary>
    /// A glyph is a single reference to a part of a font atlas and holds information pertaining to the layout of the character
    /// it represents.
    /// </summary>
    public struct Glyph
    {
        // Atlas Structure

        /// <summary>
        /// X coordinate within the texture atlas.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate within the texture atlas.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The width of the bounding rectangle within the texture atlas.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the bounding rectangle within the texture atlas.
        /// </summary>
        public int Height { get; set; }

        // Drawing

        /// <summary>
        /// The x offset for rendering.
        /// </summary>
        public float XOffset { get; set; }

        /// <summary>
        /// The y offset for rendering. This is important to aligning the letters within the pen.
        /// </summary>
        public float YOffset { get; set; }

        // Metrics

        /// <summary>
        /// The distance to increment the pen position when the glyph is drawn as part of a string of text.
        /// </summary>
        public float Advance { get; set; }

        /// <summary>
        /// This is the vertical distance from the current cursor position (on the baseline) to the topmost border of the glyph
        /// image's bounding box, minus the ascend of the font.
        /// </summary>
        public float YBearing { get; set; }

        /// <summary>
        /// Horizontal Bearing X. This is the horizontal distance from the current cursor position to the leftmost border of the
        /// glyph image's bounding box.
        /// </summary>
        public float MinX { get; set; }

        /// <summary>
        /// Calculated. Is the width of the character.
        /// </summary>
        public float MaxX { get; set; }

        /// <summary>
        /// Calculated. How much the character goes beneath the baseline.
        /// </summary>
        public float MinY { get; set; }

        /// <summary>
        /// Calculated. How much the characters goes above the baseline.
        /// </summary>
        public float MaxY { get; set; }
    }
}