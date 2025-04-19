#nullable enable

using Emotion.Utility;

namespace Emotion.WIPUpdates.Rendering;

public struct VertexDataAllocation
{
    public bool Allocated { get; set; }

    public IntPtr Pointer { get; init; } = IntPtr.Zero;

    public int VertexCount { get; init; } = 0;

    public VertexDataFormat Format { get; init; }

    public VertexDataAllocation(IntPtr pointer, int vertCount, VertexDataFormat description)
    {
        Allocated = pointer != IntPtr.Zero;
        Pointer = pointer;
        VertexCount = vertCount;
        Format = description;
    }

    public static VertexDataAllocation Allocate(VertexDataFormat format, int vertexCount, string? memoryTag = null)
    {
        if (!format.Built)
        {
            Assert(format.Built, "Using unbuilt VertexDataFormat, call .Build()");
            return new VertexDataAllocation(0, 0, format);
        }

        IntPtr allocated = IntPtr.Zero;
        int byteSize = format.ElementSize * vertexCount;
        if (byteSize > 0)
        {
            if (memoryTag != null)
                allocated = UnmanagedMemoryAllocator.MemAllocNamed(byteSize, memoryTag);
            else
                allocated = UnmanagedMemoryAllocator.MemAlloc(byteSize);
        }
        return new VertexDataAllocation(allocated, vertexCount, format);
    }

    public static VertexDataAllocation Reallocate(ref VertexDataAllocation old, int vertexCount)
    {
        if (!old.Allocated)
        {
            Assert(old.Allocated, "Trying to reallocate vertex data that isn't allocated.");
            return Allocate(old.Format, vertexCount);
        }

        old.Allocated = false;
        var newPtr = UnmanagedMemoryAllocator.Realloc(old.Pointer, vertexCount * old.Format.ElementSize);
        return new VertexDataAllocation(newPtr, vertexCount, old.Format);
    }

    public static void FreeAllocated(ref VertexDataAllocation mem)
    {
        if (!mem.Allocated) return;
        mem.Allocated = false;
        UnmanagedMemoryAllocator.Free(mem.Pointer);
    }

    public uint GetAllocationSize()
    {
        return (uint)(VertexCount * Format.ElementSize);
    }

    public unsafe Span<T> GetAsSpan<T>()
    {
        return new Span<T>((void*)Pointer, VertexCount);
    }

    public IEnumerable<Vector3> ForEachVertexPosition()
    {
        if (!Format.HasPosition) yield break;
        if (!Allocated) yield break;

        nint vertMem = Pointer;
        Assert(vertMem != 0);

        Format.GetVertexPositionOffsetAndStride(out int byteOffset, out int byteStride);
        vertMem += byteOffset;

        for (int i = 0; i < VertexCount; i++)
        {
            Vector3 v;
            unsafe
            {
                v = *(Vector3*)vertMem;
            }
            yield return v;

            if (i != VertexCount - 1)
                vertMem += byteStride;
        }
    }

    public IEnumerable<VertexBoneData> ForEachBoneData()
    {
        if (!Format.HasBones) yield break;
        if (!Allocated) yield break;

        nint vertMem = Pointer;
        Assert(vertMem != 0);

        Format.GetBoneDataOffsetAndStride(out int byteOffset, out int byteStride);
        vertMem += byteOffset;

        for (int i = 0; i < VertexCount; i++)
        {
            VertexBoneData v;
            unsafe
            {
                v = *(VertexBoneData*)vertMem;
            }
            yield return v;

            if (i != VertexCount - 1)
                vertMem += byteStride;
        }
    }

    public unsafe Triangle GetTriangleAtIndices(ushort i1, ushort i2, ushort i3)
    {
        if (!Format.HasPosition) return Triangle.Invalid;
        if (!Allocated) return Triangle.Invalid;

        nint vertMem = Pointer;
        Assert(vertMem != 0);

        Format.GetVertexPositionOffsetAndStride(out int byteOffset, out int byteStride);
        vertMem += byteOffset;

        Vector3 v1 = *(Vector3*)(vertMem + byteStride * i1);
        Vector3 v2 = *(Vector3*)(vertMem + byteStride * i2);
        Vector3 v3 = *(Vector3*)(vertMem + byteStride * i3);

        return new Triangle(v1, v2, v3);
    }
}
