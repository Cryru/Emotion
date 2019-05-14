#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GenericRec
    {
        internal IntPtr data;
        internal IntPtr finalizer;
    }
}