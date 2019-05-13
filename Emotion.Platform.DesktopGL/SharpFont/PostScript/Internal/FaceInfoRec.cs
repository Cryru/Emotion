#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.PostScript.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FaceInfoRec
    {
        [MarshalAs(UnmanagedType.LPStr)] internal string cid_font_name;
        internal FT_Long cid_version;
        internal int cid_font_type;

        [MarshalAs(UnmanagedType.LPStr)] internal string registry;

        [MarshalAs(UnmanagedType.LPStr)] internal string ordering;
        internal int supplement;

        internal FontInfoRec font_info;
        internal BBox font_bbox;
        internal FT_ULong uid_base;

        internal int num_xuid;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal FT_ULong[] xuid;

        internal FT_ULong cidmap_offset;
        internal int fd_bytes;
        internal int gd_bytes;
        internal FT_ULong cid_count;

        internal int num_dicts;
        internal FT_Long font_dicts;

        internal FT_ULong data_offset;
    }
}