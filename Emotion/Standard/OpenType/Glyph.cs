#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;

#endregion

namespace Emotion.Standard.OpenType
{
    public class Glyph
    {
        /// <summary>
        /// The character this glyph corresponds to.
        /// </summary>
        public List<char> CharIndex { get; set; } = new List<char>(1);

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

        public short XMin { get; set; }
        public short YMin { get; set; }
        public short XMax { get; set; }
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

        public Rectangle GetDrawBox(float scale)
        {
            Rectangle bbox = GetBBox(scale);
            return new Rectangle(0, 0, bbox.Width - bbox.X, bbox.Height - bbox.Y);
        }

        public Rectangle GetBBox(float scale)
        {
            return new Rectangle(
                MathF.Floor(XMin * scale),
                MathF.Floor(-YMax * scale),
                MathF.Ceiling(XMax * scale),
                MathF.Ceiling(-YMin * scale)
            );
        }
    }
}