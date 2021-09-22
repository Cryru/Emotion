#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;

#endregion

namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// Represents a single font glyph. One glyph can represent multiple characters in a font.
    /// </summary>
    public class Glyph
    {
        /// <summary>
        /// The internal font index of this glyph.
        /// </summary>
        public uint MapIndex { get; set; }

        /// <summary>
        /// The name of the glyph, according to encoding and everything.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The glyphs vertices.
        /// </summary>
        public GlyphVertex[] Vertices;

        #region Metrics

        /// <summary>
        /// The leftmost point of the glyph.
        /// </summary>
        public short XMin { get; set; }

        /// <summary>
        /// The rightmost point of the glyph.
        /// </summary>
        public short XMax { get; set; }

        /// <summary>
        /// The highest point of the glyph. Can extend below the baseline.
        /// </summary>
        public short YMin { get; set; }

        /// <summary>
        /// The highest point of the glyph.
        /// </summary>
        public short YMax { get; set; }

        /// <summary>
        /// The glyph's advance. This is the distance from the baseline to the baseline for the next glyph, kerning excluded.
        /// </summary>
        public ushort AdvanceWidth { get; set; }

        /// <summary>
        /// The glyph's lsb. This is the distance from the baseline to the glyph start.
        /// </summary>
        public short LeftSideBearing { get; set; }

        #endregion

        /// <summary>
        /// The draw box for the glyph. This is essentially the rendering canvas.
        /// </summary>
        /// <param name="scale">The scale to get the drawing box at.</param>
        /// <returns></returns>
        public Rectangle GetDrawBox(float scale)
        {
            Rectangle bbox = GetBBox(scale);
            return new Rectangle(0, 0, bbox.Width - bbox.X, bbox.Height - bbox.Y);
        }

        /// <summary>
        /// The bounding box for the glyph.
        /// </summary>
        /// <param name="scale">The scale to get the bounding box at.</param>
        public Rectangle GetBBox(float scale)
        {
            return new Rectangle(
                MathF.Floor(XMin * scale),
                MathF.Floor(-YMax * scale),
                MathF.Ceiling(XMax * scale),
                MathF.Ceiling(-YMin * scale)
            );
        }

        /// <summary>
        /// The draw box for the glyph. This is essentially the rendering canvas.
        /// </summary>
        /// <param name="scale">The scale to get the drawing box at.</param>
        /// <returns></returns>
        public Rectangle GetDrawBoxFloat(float scale)
        {
            Rectangle bbox = GetBBoxFloat(scale);
            return new Rectangle(0, 0, bbox.Width - bbox.X, bbox.Height - bbox.Y);
        }

        /// <summary>
        /// The bounding box for the glyph.
        /// </summary>
        /// <param name="scale">The scale to get the bounding box at.</param>
        public Rectangle GetBBoxFloat(float scale)
        {
            return new Rectangle(
                XMin * scale,
                -YMax * scale,
                XMax * scale,
                -YMin * scale
            );
        }
    }
}