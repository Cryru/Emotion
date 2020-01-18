#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.MultipleMasters.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MMAxisRec
    {
        [MarshalAs(UnmanagedType.LPStr)] internal string name;

        internal FT_Long minimum;
        internal FT_Long maximum;
    }
}