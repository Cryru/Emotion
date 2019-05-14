#region Using

using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RasterFuncsRec
    {
        internal GlyphFormat glyph_format;
        internal RasterNewFunc raster_new;
        internal RasterResetFunc raster_reset;
        internal RasterSetModeFunc raster_set_mode;
        internal RasterRenderFunc raster_render;
        internal RasterDoneFunc raster_done;
    }
}