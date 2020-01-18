#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.PostScript.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FaceDictRec
    {
        internal PrivateRec private_dict;

        internal uint len_buildchar;
        internal FT_Long forcebold_threshold;
        internal FT_Long stroke_width;
        internal FT_Long expansion_factor;

        internal byte paint_type;
        internal byte font_type;
        internal FTMatrix font_matrix;
        internal FTVector font_offset;

        internal uint num_subrs;
        internal FT_ULong subrmap_offset;
        internal int sd_bytes;
    }
}