#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a GlyphSlot.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="GlyphSlot" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GlyphSlotRec
    {
        internal FT_Long library;
        internal FT_Long face;
        internal FT_Long next;
        internal uint reserved;
        internal GenericRec generic;

        internal GlyphMetricsRec metrics;
        internal FT_Long linearHoriAdvance;
        internal FT_Long linearVertAdvance;
        internal FTVector26Dot6 advance;

        internal GlyphFormat format;

        internal BitmapRec bitmap;
        internal int bitmap_left;
        internal int bitmap_top;

        internal OutlineRec outline;

        internal uint num_subglyphs;
        internal FT_Long subglyphs;

        internal FT_Long control_data;
        internal FT_Long control_len;

        internal FT_Long lsb_delta;
        internal FT_Long rsb_delta;

        internal FT_Long other;

        private FT_Long @internal;
    }
}