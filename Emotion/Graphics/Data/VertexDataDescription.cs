#nullable enable

using Emotion.Utility;
using System.Text;

namespace Emotion.Graphics.Data;

public struct VertexDataAllocation
{
    public bool Allocated { get => Pointer != IntPtr.Zero; }

    public IntPtr Pointer { get; init; } = IntPtr.Zero;

    public int VertexCount { get; init; } = 0;

    public VertexDataDescription Format { get; init; }

    public VertexDataAllocation(IntPtr pointer, int vertCount, VertexDataDescription description)
    {
        Pointer = pointer;
        VertexCount = vertCount;
        Format = description;
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

        Vector3 v1 = *(Vector3*) (vertMem + byteStride * i1);
        Vector3 v2 = *(Vector3*)(vertMem + byteStride * i2);
        Vector3 v3 = *(Vector3*)(vertMem + byteStride * i3);

        return new Triangle(v1, v2, v3);
    }
}

public sealed class VertexDataDescription
{
    public static VertexDataDescription Default2D = new VertexDataDescription()
        .AddVertexPosition()
        .AddUV(1)
        .AddVertexColor()
        .Build();

    public bool Built { get; private set; }

    public int ElementSize { get; private set; }

    public bool HasPosition { get; private set; }

    public int HasUVCount { get; private set; }

    public bool HasNormals { get; private set; }

    public bool HasVertexColors { get; private set; }

    public bool HasBones { get; private set; }

    private const int POSITION_SIZE = sizeof(float) * 3; // Vector3
    private const int UV_SIZE = sizeof(float) * 2; // Vector2 (per UV)
    private const int NORMAL_SIZE = sizeof(float) * 3; // Vector3
    private const int COLOR_SIZE = sizeof(uint);
    private const int BONE_DATA_SIZE = sizeof(float) * 8; // VertexBoneData (Vector4 * 2)

    #region Build

    public VertexDataDescription AddVertexPosition()
    {
        if (!Built)
            HasPosition = true;
        return this;
    }

    public VertexDataDescription AddUV(int count)
    {
        if (!Built)
            HasUVCount += count;
        return this;
    }

    public VertexDataDescription AddNormal()
    {
        if (!Built)
            HasNormals = true;
        return this;
    }

    public VertexDataDescription AddVertexColor()
    {
        if (!Built)
            HasVertexColors = true;
        return this;
    }

    public VertexDataDescription AddBoneData()
    {
        if (!Built)
            HasBones = true;
        return this;
    }

    public VertexDataDescription Build()
    {
        if (Built) return this;

        int elementSize = 0;
        if (HasPosition) elementSize += POSITION_SIZE;
        if (HasUVCount > 0) elementSize += UV_SIZE * HasUVCount;
        if (HasNormals) elementSize += NORMAL_SIZE;
        if (HasVertexColors) elementSize += COLOR_SIZE;
        if (HasBones) elementSize += BONE_DATA_SIZE;
        ElementSize = elementSize;

        Built = true;
        return this;
    }

    public VertexDataAllocation GetAllocation(int vertexCount, string? memoryTag = null)
    {
        if (!Built) return new VertexDataAllocation(0, 0, this);

        IntPtr allocated = IntPtr.Zero;
        int byteSize = ElementSize * vertexCount;
        if (byteSize > 0)
        {
            if (memoryTag != null)
                allocated = UnmanagedMemoryAllocator.MemAllocNamed(byteSize, memoryTag);
            else
                allocated = UnmanagedMemoryAllocator.MemAlloc(byteSize);
        }
        return new VertexDataAllocation(allocated, vertexCount, this);
    }

    #endregion

    #region Enum Helpers

    public void GetVertexPositionOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasPosition);

        offset = 0;
        stride = ElementSize;
    }

    public void GetUVOffsetAndStride(int uvIndex, out int offset, out int stride)
    {
        Assert(HasUVCount > uvIndex);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < uvIndex; i++)
        {
            byteOffset += UV_SIZE;
        }

        offset = byteOffset;
        stride = ElementSize;
    }

    public void GetNormalOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasNormals);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < HasUVCount; i++)
        {
            byteOffset += UV_SIZE;
        }

        offset = byteOffset;
        stride = ElementSize;
    }

    public void GetBoneDataOffsetAndStride(out int offset, out int stride)
    {
        Assert(HasBones);

        int byteOffset = 0;
        if (HasPosition) byteOffset += POSITION_SIZE;
        for (int i = 0; i < HasUVCount; i++)
        {
            byteOffset += UV_SIZE;
        }
        if (HasVertexColors) byteOffset += COLOR_SIZE;

        offset = byteOffset;
        stride = ElementSize;
    }

    #endregion

    #region Shader Interop

    public void GetDescriptionForShader(StringBuilder b)
    {
        // $"VERTEX_ATTRIBUTE({nextLocation}, Vector3, vertPos)"

        int nextLocation = 0;
        if (HasPosition)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec3 vertPos");
            nextLocation++;
        }
        if (HasUVCount > 0)
        {
            for (int i = 0; i < HasUVCount; i++)
            {
                if (i == 0)
                    b.AppendLine($"layout(location = {nextLocation})in vec2 uv");
                else
                    b.AppendLine($"layout(location = {nextLocation})in vec2 uv_{i + 1}");

                nextLocation++;
            }
        }
        if (HasNormals)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec3 normal");
            nextLocation++;
        }
        if (HasVertexColors)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec4 color");
            nextLocation++;
        }
        if (HasBones)
        {
            b.AppendLine($"layout(location = {nextLocation})in vec4 boneIds");
            nextLocation++;
            b.AppendLine($"layout(location = {nextLocation})in vec4 boneWeights");
            nextLocation++;
        }
    }

    #endregion
}
