#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Helpers
{
    public static class UnmanagedMemoryAllocator
    {
        public static int AllocatedSize;
        private static Dictionary<string, UnmanagedMemory> _namedMemory = new Dictionary<string, UnmanagedMemory>();
        private static Dictionary<string, UnmanagedMemory> _namedMemoryForeign = new Dictionary<string, UnmanagedMemory>();

        public static IntPtr MemAlloc(int size)
        {
            AllocatedSize += size;
            IntPtr memory = Marshal.AllocHGlobal(size);
            return memory;
        }

        public static IntPtr MemAllocNamed(int size, string name)
        {
            IntPtr memory = MemAlloc(size);
            _namedMemory.Add(name, new UnmanagedMemory
            {
                Size = size,
                Memory = memory
            });
            return memory;
        }

        public static IntPtr MemAllocOrReAllocNamed(int size, string name)
        {
            // Check if memory with this name is already allocated.
            IntPtr ptr = GetNamedMemoryOwned(name, out int oldSize);
            if (ptr == IntPtr.Zero) return MemAllocNamed(size, name);

            AllocatedSize = AllocatedSize - oldSize + size;
            IntPtr newPtr = Marshal.ReAllocHGlobal(ptr, (IntPtr) size);
            _namedMemory[name] = new UnmanagedMemory
            {
                Size = size,
                Memory = newPtr
            };

            return newPtr;
        }

        public static void RegisterUnownedNamedMemory(IntPtr ptr, string name, int size)
        {
            _namedMemoryForeign.Add(name, new UnmanagedMemory
            {
                Size = size,
                Memory = ptr
            });
        }

        public static IntPtr GetNamedMemoryOwned(string name, out int size)
        {
            _namedMemory.TryGetValue(name, out UnmanagedMemory value);
            size = value.Size;
            return value.Memory;
        }

        public static IntPtr GetNamedMemory(string name, out int size)
        {
            IntPtr owned = GetNamedMemoryOwned(name, out size);
            if (owned != IntPtr.Zero) return owned;
            _namedMemoryForeign.TryGetValue(name, out UnmanagedMemory value);
            size = value.Size;
            return value.Memory;
        }
    }
}