#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.MultipleMasters.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct VarNamedStyleRec
    {
        internal IntPtr coords;
        internal uint strid;
    }
}