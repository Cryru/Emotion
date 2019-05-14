#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Fnt.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct HeaderRec
    {
        internal ushort version;
        internal FT_ULong file_size;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        internal byte[] copyright;

        internal ushort file_type;
        internal ushort nominal_point_size;
        internal ushort vertical_resolution;
        internal ushort horizontal_resolution;
        internal ushort ascent;
        internal ushort internal_leading;
        internal ushort external_leading;
        internal byte italic;
        internal byte underline;
        internal byte strike_out;
        internal ushort weight;
        internal byte charset;
        internal ushort pixel_width;
        internal ushort pixel_height;
        internal byte pitch_and_family;
        internal ushort avg_width;
        internal ushort max_width;
        internal byte first_char;
        internal byte last_char;
        internal byte default_char;
        internal byte break_char;
        internal ushort bytes_per_row;
        internal FT_ULong device_offset;
        internal FT_ULong face_name_offset;
        internal FT_ULong bits_pointer;
        internal FT_ULong bits_offset;
        internal byte reserved;
        internal FT_ULong flags;
        internal ushort A_space;
        internal ushort B_space;
        internal ushort C_space;
        internal ushort color_table_offset;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal FT_ULong[] reserved1;
    }
}