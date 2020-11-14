#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDataArgs
    {
        public int Target;
        public uint SizeWholeBuffer;
        public IntPtr Ptr;
        public int Usage;
        public int Offset;
        public uint Length;
    }
}