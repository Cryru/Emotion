#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont.Cache.Internal
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageTypeRec
    {
        internal IntPtr face_id;
        internal int width;
        internal int height;
        internal LoadFlags flags;
    }
}