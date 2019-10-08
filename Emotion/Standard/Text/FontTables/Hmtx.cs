#region Using

using System;
using Emotion.Standard.Utility;

#endregion

namespace Emotion.Standard.Text.FontTables
{
    public static class Hmtx
    {
        public static void ParseHmtx(ByteReader reader, ushort numberOfHMetrics, Glyph[] glyphs)
        {
            var metrics = new Tuple<ushort, short>[numberOfHMetrics];

            // Read metrics.
            for (var i = 0; i < numberOfHMetrics; i++)
            {
                metrics[i] = new Tuple<ushort, short>(reader.ReadUShortBE(), reader.ReadShortBE());
            }

            // Assign metrics to glyphs.
            int lastMetric = metrics.Length - 1;
            for (var i = 0; i < glyphs.Length; i++)
            {
                Glyph glyph = glyphs[i];
                if (glyph.MapIndex > 0 && glyph.MapIndex < lastMetric)
                {
                    glyph.AdvanceWidth = metrics[glyph.MapIndex].Item1;
                    glyph.LeftSideBearing = metrics[glyph.MapIndex].Item2;
                }
                else
                {
                    glyph.AdvanceWidth = metrics[lastMetric].Item1;
                    glyph.LeftSideBearing = metrics[lastMetric].Item2;
                }
            }
        }
    }
}