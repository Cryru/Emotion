#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RasterParamsRec
    {
        internal IntPtr target;
        internal IntPtr source;
        internal RasterFlags flags;
        internal RasterSpanFunc gray_spans;
        internal RasterSpanFunc black_spans;

        [Obsolete("Unused")] internal RasterBitTestFunc bit_test;

        [Obsolete("Unused")] internal RasterBitSetFunc bit_set;

        internal IntPtr user;
        internal BBox clip_box;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<RasterParamsRec>();
        }
    }
}