#region Using

using System;
using Emotion.Standard.Utility;

#endregion

namespace Emotion.Standard.Text.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/head
    /// </summary>
    public class Head
    {
        public float Version;
        public float FontRevision;
        public uint CheckSumAdjustment;
        public uint MagicNumber;
        public ushort Flags;

        public ushort UnitsPerEm;

        public uint Created;
        public uint Modified;
        public short XMin;
        public short YMin;
        public short XMax;
        public short YMax;
        public ushort MacStyle;
        public ushort LowestRecPPEM;
        public short FontDirectionHint;
        public short IndexToLocFormat;
        public short GlyphDataFormat;

        /// <summary>
        /// Essentially the header.
        /// </summary>
        public Head(ByteReader reader)
        {
            Version = reader.ReadOpenTypeVersionBE();
            FontRevision = (float) (Math.Round(reader.ReadFloatBE() * 1000) / 1000);
            CheckSumAdjustment = reader.ReadULongBE();
            MagicNumber = reader.ReadULongBE();
            if (MagicNumber != 0x5F0F3CF5) Console.WriteLine("Font: Font header has wrong magic number.");
            Flags = reader.ReadUShortBE();
            UnitsPerEm = reader.ReadUShortBE();
            Created = reader.ReadULongBETimestamp();
            Modified = reader.ReadULongBETimestamp();
            XMin = reader.ReadShortBE();
            YMin = reader.ReadShortBE();
            XMax = reader.ReadShortBE();
            YMax = reader.ReadShortBE();
            MacStyle = reader.ReadUShortBE();
            LowestRecPPEM = reader.ReadUShortBE();
            FontDirectionHint = reader.ReadShortBE();
            IndexToLocFormat = reader.ReadShortBE();
            GlyphDataFormat = reader.ReadShortBE();
        }
    }
}