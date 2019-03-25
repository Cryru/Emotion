#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct StreamRec
    {
        internal FT_Long @base;
        internal FT_ULong size;
        internal FT_ULong pos;

        internal StreamDescRec descriptor;
        internal StreamDescRec pathname;
        internal StreamIOFunc read;
        internal StreamCloseFunc close;

        internal FT_Long memory;
        internal FT_Long cursor;
        internal FT_Long limit;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<StreamRec>();
        }
    }
}