#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion.Utility
{
    public static class UnmanagedMemoryAllocator
    {
        // Represents a piece of allocated memory.
        private class UnmanagedMemory
        {
            public int Size;
            public IntPtr Address;
            public bool Owned;
            public string Label;
        }

        public static int AllocatedSize;
        private static List<UnmanagedMemory> _allocatedMemory = new List<UnmanagedMemory>();
        private static Dictionary<IntPtr, UnmanagedMemory> _ptrToHandle = new Dictionary<IntPtr, UnmanagedMemory>();
        private static Dictionary<string, UnmanagedMemory> _labeledMemory = new Dictionary<string, UnmanagedMemory>();

        private static IntPtr AllocInternal(int size, out UnmanagedMemory memoryHandle)
        {
            AllocatedSize += size;
            IntPtr memoryPtr = Marshal.AllocHGlobal(size);
            memoryHandle = new UnmanagedMemory
            {
                Size = size,
                Address = memoryPtr,
                Owned = true
            };
            _allocatedMemory.Add(memoryHandle);
            _ptrToHandle.Add(memoryPtr, memoryHandle);
            return memoryPtr;
        }

        /// <summary>
        /// Allocate a span of unmanaged memory.
        /// </summary>
        /// <param name="size">The size of memory to allocate in bytes.</param>
        /// <returns></returns>
        public static IntPtr MemAlloc(int size)
        {
            return AllocInternal(size, out UnmanagedMemory _);
        }

        /// <summary>
        /// Free allocated memory.
        /// </summary>
        /// <param name="ptr">Address of the memory to free.</param>
        /// <returns></returns>
        public static bool Free(IntPtr ptr)
        {
            UnmanagedMemory handle = _ptrToHandle[ptr];
            if (handle == null) return false;

            handle.Address = IntPtr.Zero;
            _ptrToHandle[ptr] = null;
            if (handle.Label != null) _labeledMemory.Remove(handle.Label);
            _allocatedMemory.Remove(handle);

            Marshal.FreeHGlobal(ptr);
            return true;
        }

        /// <summary>
        /// Allocate a piece of memory with a label.
        /// </summary>
        /// <param name="size">Size in bytes to allocate.</param>
        /// <param name="name">Name for the memory.</param>
        /// <returns></returns>
        public static IntPtr MemAllocNamed(int size, string name)
        {
            IntPtr memoryPtr = AllocInternal(size, out UnmanagedMemory memory);
            memory.Label = name;
            _labeledMemory.Add(name, memory);
            return memoryPtr;
        }

        /// <summary>
        /// Allocate or reallocate owned labeled memory.
        /// </summary>
        /// <param name="size">The new size of the memory if it already exists, or the size to allocate if it doesn't.</param>
        /// <param name="name">The label for the memory.</param>
        /// <returns></returns>
        public static IntPtr MemAllocOrReAllocNamed(int size, string name)
        {
            // Create if label doesn't exist.
            if (!_labeledMemory.TryGetValue(name, out UnmanagedMemory memory)) return MemAllocNamed(size, name);

            // Fast path for size unchanged.
            if (memory.Size == size) return memory.Address;

            // Reallocate.
            AllocatedSize = AllocatedSize - memory.Size + size;
            IntPtr newPtr = Marshal.ReAllocHGlobal(memory.Address, (IntPtr) size);
            memory.Size = size;
            _ptrToHandle[memory.Address] = null;
            _ptrToHandle[newPtr] = memory;
            memory.Address = newPtr;
            return newPtr;
        }

        /// <summary>
        /// Register already allocated memory to be managed by the allocator.
        /// </summary>
        /// <param name="ptr">Pointer to the already allocated memory.</param>
        /// <param name="name">The label for the memory.</param>
        /// <param name="size">Size of the memory.</param>
        public static void RegisterAllocatedMemory(IntPtr ptr, string name, int size)
        {
            var info = new UnmanagedMemory
            {
                Size = size,
                Address = ptr,
                Label = name
            };
            _ptrToHandle[ptr] = info;
            _labeledMemory[name] = info;
            _allocatedMemory.Add(info);
        }

        /// <summary>
        /// Get the address of a span of labeled memory.
        /// </summary>
        /// <param name="name">The label of the memory to return.</param>
        /// <param name="size">The size of the memory span.</param>
        /// <returns></returns>
        public static IntPtr GetNamedMemory(string name, out int size)
        {
            size = 0;
            if (!_labeledMemory.TryGetValue(name, out UnmanagedMemory memoryHandle)) return IntPtr.Zero;
            size = memoryHandle.Size;
            return memoryHandle.Address;
        }

        public static string GetDebugInformation()
        {
            var dbg = new StringBuilder($"Unmanaged Allocated: {Helpers.FormatByteAmountAsString(AllocatedSize)}");
            for (var i = 0; i < _allocatedMemory.Count; i++)
            {
                if (i == 0) dbg.Append("\n");

                UnmanagedMemory handle = _allocatedMemory[i];
                dbg.AppendLine($"{handle.Address} [{handle.Label}]: {Helpers.FormatByteAmountAsString(handle.Size)}");
            }

            return dbg.ToString();
        }
    }
}