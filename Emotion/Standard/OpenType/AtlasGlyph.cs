#region Using

using System;
using System.Numerics;
using Emotion.Primitives;

#endregion

namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// Represents a single glyph within a FontAtlas.
    /// </summary>
    public class AtlasGlyph
    {
        /// <summary>
        /// The location of the glyph within the atlas texture.
        /// </summary>
        public Vector2 Location { get; set; }

        /// <summary>
        /// The size of the glyph texture within the atlas texture.
        /// </summary>
        public Vector2 UV { get; set; }

        /// <summary>
        /// The size of the glyph.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// (XMax) The width between this glyph and the next one (whichever it is).
        /// </summary>
        public float Advance { get; protected set; }

        /// <summary>
        /// The width of the glyph, left of the origin line.
        /// </summary>
        public float XMin { get; protected set; }

        /// <summary>
        /// The height of the glyph below the origin line.
        /// </summary>
        public float YMin { get; protected set; }

        /// <summary>
        /// The drawing offset of this glyph. In typography glyphs are drawn from the bottom up (from the baseline)
        /// but here they are drawn as sprites (top left origin) therefore we need to offset glyphs from the top using the highest glyph in the font.
        /// </summary>
        public float YOffset { get; protected set; }

        /// <summary>
        /// The height of the glyph, excluding the YMin descent.
        /// </summary>
        public float Height { get; protected set; }

        /// <summary>
        /// The height of the glyph, plus the y offset. This is essentially the drawing position of the glyph.
        /// </summary>
        public float YBearing { get => YOffset - Height; }

        /// <summary>
        /// The font glyph this glyph represents.
        /// </summary>
        public Glyph FontGlyph { get; protected set; }

        /// <summary>
        /// Constructor used to synthesize glyphs for tests.
        /// </summary>
        public AtlasGlyph(float advance, float xMin, float yOff)
        {
            Advance = advance;
            XMin = xMin;
            YOffset = yOff;
        }

        /// <summary>
        /// Create a new AtlasGlyph which holds metadata about a rasterized glyph within an atlas texture.
        /// </summary>
        /// <param name="fontGlyph"></param>
        /// <param name="scale"></param>
        /// <param name="ascend"></param>
        public AtlasGlyph(Glyph fontGlyph, float scale, float ascend)
        {
            FontGlyph = fontGlyph;
            Advance = MathF.Round(fontGlyph.AdvanceWidth * scale);
            XMin = MathF.Floor(fontGlyph.XMin * scale);
            Rectangle bbox = fontGlyph.GetBBox(scale);
            Height = -bbox.Y;
            YOffset = MathF.Ceiling(ascend * scale);
            YMin = MathF.Floor(fontGlyph.YMin * scale);
            Size = fontGlyph.GetDrawBox(scale).Size;
        }
    }
}