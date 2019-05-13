#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SizeMetricsRec
    {
        internal ushort x_ppem;
        internal ushort y_ppem;

        internal FT_Long x_scale;
        internal FT_Long y_scale;
        internal FT_Long ascender;
        internal FT_Long descender;
        internal FT_Long height;
        internal FT_Long max_advance;
    }
}