#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Cache.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SBitRec
    {
        internal byte width;
        internal byte height;
        internal byte left;
        internal byte top;

        internal byte format;
        internal byte max_grays;
        internal short pitch;
        internal byte xadvance;
        internal byte yadvance;

        internal IntPtr buffer;
    }
}