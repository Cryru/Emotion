#region Using

using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SpanRec
    {
        internal short x;
        internal ushort len;
        internal byte coverage;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<SpanRec>();
        }
    }
}