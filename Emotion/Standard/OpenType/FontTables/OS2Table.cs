#region Using

using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/os2
    /// </summary>
    public class OS2Table
    {
        public ushort Version;
        public short XAvgCharWidth;
        public ushort UsWeightClass;
        public ushort UsWidthClass;
        public ushort FsType;
        public short YSubscriptXSize;
        public short YSubscriptYSize;
        public short YSubscriptXOffset;
        public short YSubscriptYOffset;
        public short YSuperscriptXSize;
        public short YSuperscriptYSize;
        public short YSuperscriptXOffset;
        public short YSuperscriptYOffset;
        public short YStrikeoutSize;
        public short YStrikeoutPosition;
        public short SFamilyClass;

        public byte[] Panose = new byte[10];

        public uint UlUnicodeRange1;
        public uint UlUnicodeRange2;
        public uint UlUnicodeRange3;
        public uint UlUnicodeRange4;
        public string AchVendId;

        public ushort FsSelection;
        public ushort UsFirstCharIndex;
        public ushort UsLastCharIndex;

        public short STypoAscender;
        public short STypoDescender;
        public short STypoLineGap;
        public ushort UsWinAscent;
        public ushort UsWinDescent;

        public uint UlCodePageRange1;
        public uint UlCodePageRange2;

        public short SxHeight;
        public short SCapHeight;
        public ushort UsDefaultChar;
        public ushort UsBreakChar;
        public ushort UsMaxContent;

        /// <summary>
        /// Parse the OS/2 and Windows metrics `OS/2` table
        /// </summary>
        public OS2Table(ByteReader reader)
        {
            Version = reader.ReadUShortBE();
            XAvgCharWidth = reader.ReadShortBE();
            UsWeightClass = reader.ReadUShortBE();
            UsWidthClass = reader.ReadUShortBE();
            FsType = reader.ReadUShortBE();
            YSubscriptXSize = reader.ReadShortBE();
            YSubscriptYSize = reader.ReadShortBE();
            YSubscriptXOffset = reader.ReadShortBE();
            YSubscriptYOffset = reader.ReadShortBE();
            YSuperscriptXSize = reader.ReadShortBE();
            YSuperscriptYSize = reader.ReadShortBE();
            YSuperscriptXOffset = reader.ReadShortBE();
            YSuperscriptYOffset = reader.ReadShortBE();
            YStrikeoutSize = reader.ReadShortBE();
            YStrikeoutPosition = reader.ReadShortBE();
            SFamilyClass = reader.ReadShortBE();

            for (var i = 0; i < 10; i++)
            {
                Panose[i] = reader.ReadByte();
            }

            UlUnicodeRange1 = reader.ReadULongBE();
            UlUnicodeRange2 = reader.ReadULongBE();
            UlUnicodeRange3 = reader.ReadULongBE();
            UlUnicodeRange4 = reader.ReadULongBE();
            AchVendId = new string(reader.ReadChars(4));
            FsSelection = reader.ReadUShortBE();
            UsFirstCharIndex = reader.ReadUShortBE();
            UsLastCharIndex = reader.ReadUShortBE();
            STypoAscender = reader.ReadShortBE();
            STypoDescender = reader.ReadShortBE();
            STypoLineGap = reader.ReadShortBE();
            UsWinAscent = reader.ReadUShortBE();
            UsWinDescent = reader.ReadUShortBE();

            if (Version >= 1)
            {
                UlCodePageRange1 = reader.ReadULongBE();
                UlCodePageRange2 = reader.ReadULongBE();
            }

            if (Version < 2) return;
            SxHeight = reader.ReadShortBE();
            SCapHeight = reader.ReadShortBE();
            UsDefaultChar = reader.ReadUShortBE();
            UsBreakChar = reader.ReadUShortBE();
            UsMaxContent = reader.ReadUShortBE();
        }
    }
}