#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SizeRec
    {
        internal IntPtr face;
        internal GenericRec generic;
        internal SizeMetricsRec metrics;
        private IntPtr @internal;
    }
}