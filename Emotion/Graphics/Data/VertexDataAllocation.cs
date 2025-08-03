#nullable enable

using Emotion.Standard.Memory;

namespace Emotion.Graphics.Data;

public struct VertexDataAllocation
{
    public static VertexDataAllocation Empty = new();

    public bool Allocated { get; set; }

    public nint Pointer { get; init; } = nint.Zero;

    public int VertexCount { get; init; } = 0;

    public VertexDataFormat Format { get; init; }

    public VertexDataAllocation(nint pointer, int vertCount, VertexDataFormat description)
    {
        Allocated = pointer != nint.Zero;
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

        nint allocated = nint.Zero;
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

    public unsafe IEnumerable<Triangle> ForEachTriangle<TIndex>(TIndex[] indices, int indexCount)
        where TIndex : INumber<TIndex>
    {
        if (!Format.HasPosition) yield break;
        if (!Allocated) yield break;

        nint vertMem = Pointer;
        Assert(vertMem != 0);

        Format.GetVertexPositionOffsetAndStride(out int byteOffset, out int byteStride);
        vertMem += byteOffset;

        Triangle tri;
        if (indices is uint[] indexUint)
        {
            for (int i = 0; i < indexCount; i += 3)
            {
                uint idx1 = indexUint[i];
                uint idx2 = indexUint[i + 1];
                uint idx3 = indexUint[i + 2];

                unsafe
                {
                    Vector3 v1 = *(Vector3*)(vertMem + byteStride * idx1);
                    Vector3 v2 = *(Vector3*)(vertMem + byteStride * idx2);
                    Vector3 v3 = *(Vector3*)(vertMem + byteStride * idx3);
                    tri = new Triangle(v1, v2, v3);
                }

                yield return tri;
            }
        }
        else if (indices is ushort[] indexUshort)
        {
            for (int i = 0; i < indexCount; i += 3)
            {
                ushort idx1 = indexUshort[i];
                ushort idx2 = indexUshort[i + 1];
                ushort idx3 = indexUshort[i + 2];

                unsafe
                {
                    Vector3 v1 = *(Vector3*)(vertMem + byteStride * idx1);
                    Vector3 v2 = *(Vector3*)(vertMem + byteStride * idx2);
                    Vector3 v3 = *(Vector3*)(vertMem + byteStride * idx3);
                    tri = new Triangle(v1, v2, v3);
                }

                yield return tri;
            }
        }

        // Fully generic
        for (int i = 0; i < indexCount; i += 3)
        {
            TIndex idx1 = indices[i];
            TIndex idx2 = indices[i + 1];
            TIndex idx3 = indices[i + 2];

            uint idx1AsInt = uint.CreateTruncating(idx1);
            uint idx2AsInt = uint.CreateTruncating(idx2);
            uint idx3AsInt = uint.CreateTruncating(idx3);

            unsafe
            {
                Vector3 v1 = *(Vector3*)(vertMem + byteStride * idx1AsInt);
                Vector3 v2 = *(Vector3*)(vertMem + byteStride * idx2AsInt);
                Vector3 v3 = *(Vector3*)(vertMem + byteStride * idx3AsInt);
                tri = new Triangle(v1, v2, v3);
            }

            yield return tri;
        }
    }

    public unsafe Triangle GetTriangleAtIndices<TIndex>(TIndex i1, TIndex i2, TIndex i3) where TIndex : INumber<TIndex>
    {
        if (!Format.HasPosition) return Triangle.Invalid;
        if (!Allocated) return Triangle.Invalid;

        nint vertMem = Pointer;
        Assert(vertMem != 0);

        Format.GetVertexPositionOffsetAndStride(out int byteOffset, out int byteStride);
        vertMem += byteOffset;

        uint idx1AsInt = uint.CreateTruncating(i1);
        uint idx2AsInt = uint.CreateTruncating(i2);
        uint idx3AsInt = uint.CreateTruncating(i3);

        Vector3 v1 = *(Vector3*)(vertMem + byteStride * idx1AsInt);
        Vector3 v2 = *(Vector3*)(vertMem + byteStride * idx2AsInt);
        Vector3 v3 = *(Vector3*)(vertMem + byteStride * idx3AsInt);

        return new Triangle(v1, v2, v3);
    }
}
