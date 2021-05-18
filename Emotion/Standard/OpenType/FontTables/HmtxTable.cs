#region Using

using System;
using System.Collections.Generic;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType.FontTables
{
    public static class HmtxTable
    {
        public static void ParseHmtx(ByteReader reader, ushort numberOfHMetrics, IEnumerable<Glyph> glyphs)
        {
            var metrics = new Tuple<ushort, short>[numberOfHMetrics];

            // Read metrics.
            for (var i = 0; i < numberOfHMetrics; i++)
            {
                metrics[i] = new Tuple<ushort, short>(reader.ReadUShortBE(), reader.ReadShortBE());
            }

            // There can be more left side bearings after the ending.
            if (reader.Position + 2 <= reader.Data.Length) reader.ReadBytes(2);

            int extraMetricCount = (reader.Data.Length - reader.Position) / 2;
            if (extraMetricCount < 0) extraMetricCount = 0;
            var extraMetrics = new short[extraMetricCount];

            for (var i = 0; i < extraMetrics.Length; i++)
            {
                extraMetrics[i] = reader.ReadShortBE();
            }

            // Assign metrics to glyphs.
            int lastMetric = metrics.Length - 1;
            foreach (Glyph glyph in glyphs)
            {
                if (glyph.MapIndex < lastMetric)
                {
                    glyph.AdvanceWidth = metrics[glyph.MapIndex].Item1;
                    glyph.LeftSideBearing = metrics[glyph.MapIndex].Item2;
                }
                else
                {
                    glyph.AdvanceWidth = metrics[lastMetric].Item1;
                    var extraMetric = (int) (glyph.MapIndex - (numberOfHMetrics + 1));

                    if (extraMetric >= 0 && extraMetric < extraMetrics.Length)
                        glyph.LeftSideBearing = extraMetrics[extraMetric];
                    else
                        glyph.LeftSideBearing = 0;
                }
            }
        }
    }
}