#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryRec
    {
        internal IntPtr user;
        internal AllocFunc alloc;
        internal FreeFunc free;
        internal ReallocFunc realloc;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<MemoryRec>();
        }
    }
}