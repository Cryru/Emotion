#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.PostScript.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FontInfoRec
    {
        [MarshalAs(UnmanagedType.LPStr)] internal string version;

        [MarshalAs(UnmanagedType.LPStr)] internal string notice;

        [MarshalAs(UnmanagedType.LPStr)] internal string full_name;

        [MarshalAs(UnmanagedType.LPStr)] internal string family_name;

        [MarshalAs(UnmanagedType.LPStr)] internal string weight;

        internal FT_Long italic_angle;
        internal byte is_fixed_pitch;
        internal short underline_position;
        internal ushort underline_thickness;
    }
}