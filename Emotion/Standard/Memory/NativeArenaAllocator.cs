#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Standard.Memory;

public unsafe class NativeArenaAllocator
{
    public void* Memory;
    public nuint MemoryLength = 0;
    public nuint MemoryUsed = 0;

    public NativeArenaAllocator(nuint initialSize)
    {
        Memory = NativeMemory.Alloc(initialSize);
        MemoryUsed = 0;
        MemoryLength = initialSize;
    }

    public void Clear()
    {
        MemoryUsed = 0;
    }

    public unsafe Span<T> AllocateSpan<T>(int size) where T : unmanaged
    {
        void* ptr = Allocate((nuint) (size * sizeof(T)));
        Span<T> span = new Span<T>(ptr, size);
        return span;
    }

    public unsafe T* AllocateOfType<T>(int elements) where T : unmanaged
    {
        void* ptr = Allocate((nuint)(elements * sizeof(T)));
        return (T*)ptr;
    }

    public void* Allocate(nuint size)
    {
        if (MemoryUsed + size > MemoryLength)
        {
            // Oh oh! Resize!
            nuint newLength = MemoryLength * 2;
            void* newMemory = NativeMemory.Alloc(newLength);
            NativeMemory.Copy(Memory, newMemory, MemoryLength);
            NativeMemory.Free(Memory);

            Memory = newMemory;
            MemoryLength = newLength;

            Engine.Log.Warning($"Native arena allocator grew.", "NativeArena");
        }

        void* blockStart = ((byte*)Memory) + MemoryUsed;
        MemoryUsed += size;
        return blockStart;
    }
}
