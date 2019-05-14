#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.PostScript.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PrivateRec
    {
        internal int unique_id;
        internal int lenIV;

        internal byte num_blue_values;
        internal byte num_other_blues;
        internal byte num_family_blues;
        internal byte num_family_other_blues;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        internal short[] blue_values;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        internal short[] other_blues;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        internal short[] family_blues;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        internal short[] family_other_blues;

        internal FT_Long blue_scale;
        internal int blue_shift;
        internal int blue_fuzz;

        internal ushort standard_width;
        internal ushort standard_height;

        internal byte num_snap_widths;
        internal byte num_snap_heights;
        internal byte force_bold;
        internal byte round_stem_up;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        internal short[] snap_widths;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        internal short[] snap_heights;

        internal FT_Long expansion_factor;

        internal FT_Long language_group;
        internal FT_Long password;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        internal short[] min_feature;
    }
}