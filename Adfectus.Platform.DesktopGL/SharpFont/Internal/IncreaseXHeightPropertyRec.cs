#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct IncreaseXHeightPropertyRec
    {
        internal IntPtr face;
        internal uint limit;
    }
}