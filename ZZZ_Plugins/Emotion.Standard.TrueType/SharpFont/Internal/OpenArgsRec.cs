#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OpenArgsRec
    {
        internal OpenFlags flags;
        internal FT_Long memory_base;
        internal FT_Long memory_size;

        [MarshalAs(UnmanagedType.LPStr)] internal string pathname;

        internal FT_Long stream;
        internal FT_Long driver;
        internal int num_params;
        internal FT_Long @params;
    }
}