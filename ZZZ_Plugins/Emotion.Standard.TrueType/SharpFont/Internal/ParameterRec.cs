#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ParameterRec
    {
        internal FT_ULong tag;
        internal FT_Long data;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<ParameterRec>();
        }
    }
}