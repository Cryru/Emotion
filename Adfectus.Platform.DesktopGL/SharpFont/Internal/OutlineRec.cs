#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct OutlineRec
    {
        internal short n_contours;
        internal short n_points;

        internal IntPtr points;
        internal IntPtr tags;
        internal IntPtr contours;

        internal OutlineFlags flags;
    }
}