#region Using

using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapGlyphRec
    {
        internal GlyphRec root;
        internal int left;
        internal int top;
        internal BitmapRec bitmap;
    }
}