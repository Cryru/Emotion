#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PostscriptRec
    {
        internal FT_Long FormatType;
        internal FT_Long italicAngle;
        internal short underlinePosition;
        internal short underlineThickness;
        internal FT_ULong isFixedPitch;
        internal FT_ULong minMemType42;
        internal FT_ULong maxMemType42;
        internal FT_ULong minMemType1;
        internal FT_ULong maxMemType1;
    }
}