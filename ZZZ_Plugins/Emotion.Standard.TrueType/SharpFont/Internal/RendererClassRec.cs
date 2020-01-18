#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RendererClassRec
    {
        internal ModuleClassRec root;

        internal GlyphFormat glyph_format;

        internal IntPtr render_glyph;
        internal IntPtr transform_glyph;
        internal IntPtr get_glyph_cbox;
        internal IntPtr set_mode;

        internal IntPtr raster_class;
    }
}