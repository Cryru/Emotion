#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/hmtx
    /// </summary>
    public class HmtxTable
    {
        private ByteReader _reader;

        private HmtxTable(ByteReader reader)
        {
            _reader = reader;
        }

        public static HmtxTable ParseHmtx(ByteReader reader)
        {
            return new HmtxTable(reader);
        }

        public void ApplyToGlyphs(int numberOfHMetrics, FontGlyph[] glyphs)
        {
            _reader.Position = 0;

            for (var i = 0; i < numberOfHMetrics; i++)
            {
                FontGlyph glyph = glyphs[i];
                glyph.Advance = _reader.ReadUShortBE();
                glyph.LeftSideBearing = _reader.ReadShortBE();
            }

            // There can be more left side bearings after the ending.
            // These contain only the LSB.
            if (_reader.Position + 2 <= _reader.Data.Length)
            {
                _reader.ReadBytes(2);
                int extraMetricCount = (_reader.Data.Length - _reader.Position) / 2;
                if (extraMetricCount < 0) extraMetricCount = 0;

                for (int i = numberOfHMetrics; i < extraMetricCount; i++)
                {
                    FontGlyph glyph = glyphs[i];
                    glyph.LeftSideBearing = _reader.ReadShortBE();
                }
            }
        }

        public void ApplyToGlyphsOld(int numberOfHMetrics, Glyph[] glyphs)
        {
            _reader.Position = 0;

            for (var i = 0; i < numberOfHMetrics; i++)
            {
                Glyph glyph = glyphs[i];
                glyph.AdvanceWidth = _reader.ReadUShortBE();
                glyph.LeftSideBearing = _reader.ReadShortBE();
            }

            // There can be more left side bearings after the ending.
            // These contain only the LSB.
            if (_reader.Position + 2 <= _reader.Data.Length)
            {
                _reader.ReadBytes(2);
                int extraMetricCount = (_reader.Data.Length - _reader.Position) / 2;
                if (extraMetricCount < 0) extraMetricCount = 0;

                for (int i = numberOfHMetrics; i < extraMetricCount; i++)
                {
                    Glyph glyph = glyphs[i];
                    glyph.LeftSideBearing = _reader.ReadShortBE();
                }
            }
        }
    }
}