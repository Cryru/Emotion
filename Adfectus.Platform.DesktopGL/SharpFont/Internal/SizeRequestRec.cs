#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a <see cref="SizeRequest" />.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="SizeRequest" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SizeRequestRec
    {
        internal SizeRequestType type;
        internal FT_Long width;
        internal FT_Long height;
        internal uint horiResolution;
        internal uint vertResolution;
    }
}