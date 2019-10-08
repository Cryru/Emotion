#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a BitmapSize.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="BitmapSize" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitmapSizeRec
    {
        internal short height;
        internal short width;

        internal FT_Long size;

        internal FT_Long x_ppem;
        internal FT_Long y_ppem;

        internal static int SizeInBytes
        {
            get => Marshal.SizeOf<BitmapSizeRec>();
        }
    }
}