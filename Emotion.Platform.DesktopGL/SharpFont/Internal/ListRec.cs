#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ListRec
    {
        internal IntPtr head;
        internal IntPtr tail;
    }
}