#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Cache.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ScalerRec
    {
        internal IntPtr face_id;
        internal uint width;
        internal uint height;
        internal int pixel;
        internal uint x_res;
        internal uint y_res;
    }
}