#region Using

using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OutlineGlyphRec
    {
        internal GlyphRec root;
        internal OutlineRec outline;
    }
}