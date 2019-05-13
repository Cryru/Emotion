#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.TrueType;

#endregion

namespace SharpFont.Internal
{
    /// <summary>
    /// Internally represents a CharMap.
    /// </summary>
    /// <remarks>
    /// Refer to <see cref="CharMap" /> for FreeType documentation.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct CharMapRec
    {
        internal IntPtr face;
        internal Encoding encoding;
        internal PlatformId platform_id;
        internal ushort encoding_id;
    }
}