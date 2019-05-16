#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HeaderRec
    {
        internal FT_Long Table_Version;
        internal FT_Long Font_Revision;

        internal FT_Long Checksum_Adjust;
        internal FT_Long Magic_Number;

        internal ushort Flags;
        internal ushort Units_Per_EM;

        internal FT_Long created1;

        internal FT_Long created2;
        //internal FT_Long[] Created { get { return new[] {created1, created2}; } }

        internal FT_Long modified1;

        internal FT_Long modified2;
        //internal FT_Long[] Modified { get { return new[] { modified1, modified2 }; } }

        internal short xMin;
        internal short yMin;
        internal short xMax;
        internal short yMax;

        internal ushort Mac_Style;
        internal ushort Lowest_Rec_PPEM;

        internal short Font_Direction;
        internal short Index_To_Loc_Format;
        internal short Glyph_Data_Format;
    }
}