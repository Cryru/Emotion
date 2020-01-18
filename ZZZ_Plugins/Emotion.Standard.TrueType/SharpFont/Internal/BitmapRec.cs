#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a Bitmap.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="FTBitmap" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapRec
    {
        internal int rows;
        internal int width;
        internal int pitch;
        internal IntPtr buffer;
        internal short num_grays;
        internal PixelMode pixel_mode;
        internal byte palette_mode;
        internal IntPtr palette;
    }
}