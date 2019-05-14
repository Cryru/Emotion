#region Using

using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont.TrueType.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MaxProfileRec
    {
        internal FT_Long version;
        internal ushort numGlyphs;
        internal ushort maxPoints;
        internal ushort maxContours;
        internal ushort maxCompositePoints;
        internal ushort maxCompositeContours;
        internal ushort maxZones;
        internal ushort maxTwilightPoints;
        internal ushort maxStorage;
        internal ushort maxFunctionDefs;
        internal ushort maxInstructionDefs;
        internal ushort maxStackElements;
        internal ushort maxSizeOfInstructions;
        internal ushort maxComponentElements;
        internal ushort maxComponentDepth;
    }
}