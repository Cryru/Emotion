#region Using

using System;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.OpenType;

#endregion

namespace Emotion.Graphics.Text
{
    /// <summary>
    /// Represents a single glyph within a FontAtlas.
    /// </summary>
    public class AtlasGlyph
    {
        /// <summary>
        /// The location of the glyph within the atlas texture.
        /// </summary>
        public Vector2 UVLocation { get; set; }

        /// <summary>
        /// The size of the glyph texture within the atlas texture.
        /// </summary>
        public Vector2 UVSize { get; set; }

        /// <summary>
        /// The metric size of the glyph.
        /// </summary>
        public Vector2 Size { get; init; }

        /// <summary>
        /// (XMax) The width between this glyph and the next one (whichever it is).
        /// </summary>
        public float Advance { get; protected init; }

        /// <summary>
        /// The width of the glyph, left of the origin line.
        /// </summary>
        public float XMin { get; protected init; }

        /// <summary>
        /// The height of the glyph below the origin line.
        /// </summary>
        public float YMin { get; protected init; }

        /// <summary>
        /// The drawing offset of this glyph. In typography glyphs are drawn from the bottom up (from the baseline)
        /// but here they are drawn as sprites (top left origin) therefore we need to offset glyphs from the top using the highest
        /// glyph in the font.
        /// </summary>
        public float YOffset { get; protected init; }

        /// <summary>
        /// The height of the glyph, excluding the YMin descent.
        /// </summary>
        public float Height { get; protected init; }

        /// <summary>
        /// The height of the glyph, plus the y offset. This is essentially the top-left drawing position of the glyph texture.
        /// </summary>
        public float YBearing
        {
            get => YOffset - Height;
        }

        /// <summary>
        /// The font glyph this glyph represents.
        /// </summary>
        public Glyph FontGlyph { get; protected init; }

        protected AtlasGlyph()
        {
        }

        /// <summary>
        /// Create a new AtlasGlyph which holds metadata about a rasterized glyph within an atlas texture.
        /// </summary>
        public static AtlasGlyph CreateIntScale(Glyph fontGlyph, float scale, float ascend)
        {
            Rectangle bbox = fontGlyph.GetBBox(scale);
            return new AtlasGlyph
            {
                FontGlyph = fontGlyph,
                Advance = MathF.Round(fontGlyph.AdvanceWidth * scale),
                XMin = MathF.Floor(fontGlyph.XMin * scale),
                Height = -bbox.Y,
                YOffset = MathF.Ceiling(ascend * scale),
                YMin = MathF.Floor(fontGlyph.YMin * scale),
                Size = fontGlyph.GetDrawBox(scale).Size
            };
        }

        /// <summary>
        /// Create a new AtlasGlyph which holds metadata about a rasterized glyph within an atlas texture.
        /// </summary>
        public static AtlasGlyph CreateFloatScale(Glyph fontGlyph, float scale, float ascend)
        {
            Rectangle bbox = fontGlyph.GetBBoxFloat(scale);
            return new AtlasGlyph
            {
                FontGlyph = fontGlyph,
                Advance = fontGlyph.AdvanceWidth * scale,
                XMin = fontGlyph.XMin * scale,
                Height = -bbox.Y,
                YOffset = ascend * scale,
                YMin = fontGlyph.YMin * scale,
                Size = fontGlyph.GetDrawBoxFloat(scale).Size
            };
        }

        public static AtlasGlyph CreateForTest(float advance, float xMin, float yOff, Vector2 size)
        {
            return new AtlasGlyph
            {
                Advance = advance,
                XMin = xMin,
                YOffset = yOff,
                Size = size
            };
        }
    }
}