#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct StreamDescRec
    {
        [FieldOffset(0)] internal FT_Long value;

        [FieldOffset(0)] internal FT_Long pointer;
    }
}