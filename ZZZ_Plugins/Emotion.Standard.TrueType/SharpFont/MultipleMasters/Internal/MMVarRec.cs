#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.MultipleMasters.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MMVarRec
    {
        internal uint num_axis;
        internal uint num_designs;
        internal uint num_namedstyles;
        internal IntPtr axis;
        internal IntPtr namedstyle;
    }
}