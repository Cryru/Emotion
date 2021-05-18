#region Using

using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/hhea
    /// </summary>
    public class HheaTable
    {
        public float Version;
        public short Ascender;
        public short Descender;
        public short LineGap;
        public ushort AdvanceWidthMax;
        public short MinLeftSideBearing;
        public short MinRightSideBearing;
        public short XMaxExtent;
        public short CaretSlopeRise;
        public short CaretSlopeRun;

        public short CaretOffset;

        public short MetricDataFormat;
        public ushort NumberOfHMetrics;

        /// <summary>
        /// Contains information about horizontal layout.
        /// </summary>
        public HheaTable(ByteReader reader)
        {
            Version = reader.ReadOpenTypeVersionBE();
            Ascender = reader.ReadShortBE();
            Descender = reader.ReadShortBE();
            LineGap = reader.ReadShortBE();
            AdvanceWidthMax = reader.ReadUShortBE();
            MinLeftSideBearing = reader.ReadShortBE();
            MinRightSideBearing = reader.ReadShortBE();
            XMaxExtent = reader.ReadShortBE();
            CaretSlopeRise = reader.ReadShortBE();
            CaretSlopeRun = reader.ReadShortBE();
            CaretOffset = reader.ReadShortBE();

            reader.ReadBytes(8);

            MetricDataFormat = reader.ReadShortBE();
            NumberOfHMetrics = reader.ReadUShortBE();
        }
    }
}