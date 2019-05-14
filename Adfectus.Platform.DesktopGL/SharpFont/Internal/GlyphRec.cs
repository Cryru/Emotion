#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphRec
    {
        internal IntPtr library;
        private IntPtr clazz;
        internal GlyphFormat format;
        internal FTVector advance;
    }
}