#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Helpers
{
    public static class UnmanagedMemoryAllocator
    {
        public static int AllocatedSize;

        public static void MarkAlloc(IntPtr ptr, int size = -1)
        {
            if (size != -1) AllocatedSize += size;
        }

        public static IntPtr MemAlloc(int size)
        {
            IntPtr memory = Marshal.AllocHGlobal(size);
            MarkAlloc(memory, size);
            return memory;
        }
    }
}