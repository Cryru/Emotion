#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ListNodeRec
    {
        internal IntPtr prev;
        internal IntPtr next;
        internal IntPtr data;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<ListNodeRec>();
        }
    }
}