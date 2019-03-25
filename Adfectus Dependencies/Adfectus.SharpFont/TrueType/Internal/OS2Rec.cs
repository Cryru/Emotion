#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct OS2Rec
    {
        internal ushort version;
        internal short xAvgCharWidth;
        internal ushort usWeightClass;
        internal ushort usWidthClass;
        internal EmbeddingTypes fsType;
        internal short ySubscriptXSize;
        internal short ySubscriptYSize;
        internal short ySubscriptXOffset;
        internal short ySubscriptYOffset;
        internal short ySuperscriptXSize;
        internal short ySuperscriptYSize;
        internal short ySuperscriptXOffset;
        internal short ySuperscriptYOffset;
        internal short yStrikeoutSize;
        internal short yStrikeoutPosition;
        internal short sFamilyClass;

        private fixed byte _panose[10];

        internal byte[] panose
        {
            get
            {
                byte[] array = new byte[10];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = _panose[i];
                }

                return array;
            }
        }

        internal FT_ULong ulUnicodeRange1;
        internal FT_ULong ulUnicodeRange2;
        internal FT_ULong ulUnicodeRange3;
        internal FT_ULong ulUnicodeRange4;

        private fixed byte _achVendID[4];

        internal byte[] achVendID
        {
            get
            {
                byte[] array = new byte[4];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = _achVendID[i];
                }

                return array;
            }
        }

        internal ushort fsSelection;
        internal ushort usFirstCharIndex;
        internal ushort usLastCharIndex;
        internal short sTypoAscender;
        internal short sTypoDescender;
        internal short sTypoLineGap;
        internal ushort usWinAscent;
        internal ushort usWinDescent;

        internal FT_ULong ulCodePageRange1;
        internal FT_ULong ulCodePageRange2;

        internal short sxHeight;
        internal short sCapHeight;
        internal ushort usDefaultChar;
        internal ushort usBreakChar;
        internal ushort usMaxContext;

        internal ushort usLowerOpticalPointSize;
        internal ushort usUpperOpticalPointSize;
    }
}