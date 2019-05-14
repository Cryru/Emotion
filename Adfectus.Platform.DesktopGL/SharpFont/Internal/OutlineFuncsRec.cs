#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OutlineFuncsRec
    {
        internal FT_Long moveTo;
        internal FT_Long lineTo;
        internal FT_Long conicTo;
        internal FT_Long cubicTo;
        internal int shift;
        internal FT_Long delta;
    }
}