#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphToScriptMapPropertyRec
    {
        internal IntPtr face;
        internal IntPtr map;
    }
}