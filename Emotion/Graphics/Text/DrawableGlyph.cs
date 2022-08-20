#region Using

using Emotion.Primitives;
using Emotion.Standard.OpenType;

#endregion

namespace Emotion.Graphics.Text
{
    public class DrawableGlyph
    {
        /// <summary>
        /// The character this glyph represents.
        /// </summary>
        public char Character;

        /// <summary>
        /// A reference to the font glyph this drawable glyph was derived from.
        /// </summary>
        public FontGlyph FontGlyph;

        /// <summary>
        /// The width of the glyph's bounding box.
        /// </summary>
        public float Width;

        /// <summary>
        /// The height of the glyph's bounding box.
        /// </summary>
        public float Height;

        /// <summary>
        /// Also known as the LSB (left side bearing) this is the X distance from the pen to
        /// where the glyph box starts.
        /// </summary>
        public float XBearing;

        /// <summary>
        /// The X distance from the pen to where the glyph ends (includes XBearing, glyph width, and right side bearing)
        /// This is essentially what you add to get the next pen location.
        /// </summary>
        public float XAdvance;

        /// <summary>
        /// The UV coordinates of the glyph within the atlas texture.
        /// </summary>
        public Rectangle GlyphUV;

        public float Descent;

        public DrawableGlyph(char c, FontGlyph g, float scale)
        {
            Character = c;
            FontGlyph = g;
            Width = (g.Max.X - g.Min.X) * scale;
            Height = (g.Max.Y - g.Min.Y) * scale;
            XAdvance = g.AdvanceWidth * scale;
            XBearing = g.LeftSideBearing * scale;
            Descent = g.Min.Y * scale;
        }

        public override string ToString()
        {
            return $"[{Character}] {Width}x{Height}";
        }

        protected DrawableGlyph()
        {
        }

        public bool CanBeShown()
        {
            return !(Width == 0 || Height == 0 || FontGlyph.Commands == null || FontGlyph.Commands.Length == 0);
        }

        public static DrawableGlyph CreateForTest(float xAdvance, float xBearing, float width, float height)
        {
            return new DrawableGlyph
            {
                XAdvance = xAdvance,
                XBearing = xBearing,
                Width = width,
                Height = height
            };
        }
    }
}