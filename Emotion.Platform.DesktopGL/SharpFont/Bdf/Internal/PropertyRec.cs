#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Bdf.Internal
{
    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    internal struct PropertyRec
    {
        [FieldOffset(0)] internal PropertyType type;

        [FieldOffset(4)] internal IntPtr atom;

        [FieldOffset(4)] internal int integer;

        [FieldOffset(4)] internal uint cardinal;
    }
}