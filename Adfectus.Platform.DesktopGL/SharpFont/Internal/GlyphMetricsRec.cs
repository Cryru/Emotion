#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a GlyphMetrics.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="GlyphMetrics" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphMetricsRec
    {
        internal FT_Long width;
        internal FT_Long height;

        internal FT_Long horiBearingX;
        internal FT_Long horiBearingY;
        internal FT_Long horiAdvance;

        internal FT_Long vertBearingX;
        internal FT_Long vertBearingY;
        internal FT_Long vertAdvance;
    }
}