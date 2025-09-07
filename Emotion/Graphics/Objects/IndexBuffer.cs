#nullable enable

using OpenGL;
using System.Runtime.InteropServices;

namespace Emotion.Graphics.Objects;

/// <summary>
/// A buffer holding vertex indices.
/// </summary>
public sealed class IndexBuffer : DataBuffer
{
    /// <summary>
    /// The bound index buffer.
    /// </summary>
    public new static uint Bound
    {
        get => DataBuffer.Bound[BufferTarget.ElementArrayBuffer];
        set => DataBuffer.Bound[BufferTarget.ElementArrayBuffer] = value;
    }

    /// <summary>
    /// The data type of the indices in the buffer.
    /// This is ushort by default.
    /// </summary>
    public DrawElementsType DataType { get; init; }

    /// <summary>
    /// The size of elements in bytes.
    /// </summary>
    public int DataTypeSize { get; init; }

    public IndexBuffer(DrawElementsType dataType, uint byteSize = 0, BufferUsage usage = BufferUsage.StaticDraw) : base(BufferTarget.ElementArrayBuffer, byteSize, usage)
    {
        DataType = dataType;
        DataTypeSize = dataType switch
        {
            DrawElementsType.UnsignedByte => 1,
            DrawElementsType.UnsignedInt => 4,
            DrawElementsType.UnsignedShort => 2,
            _ => 0
        };
        Assert(DataTypeSize > 0);
    }

    public IndexBuffer(uint byteSize = 0, BufferUsage usage = BufferUsage.StaticDraw) : this(DrawElementsType.UnsignedShort, byteSize, usage)
    {
    }

    /// <summary>
    /// Ensures the provided pointer is the currently bound index buffer.
    /// </summary>
    /// <param name="pointer">The pointer to ensure is bound.</param>
    public static void EnsureBound(uint pointer)
    {
        EnsureBound(pointer, BufferTarget.ElementArrayBuffer);
    }

    public static void FillQuadIndices(Span<ushort> indices, int offset)
    {
        for (var i = 0; i < indices.Length; i += 6)
        {
            indices[i] = (ushort) (offset + 0);
            indices[i + 1] = (ushort) (offset + 1);
            indices[i + 2] = (ushort) (offset + 2);
            indices[i + 3] = (ushort) (offset + 2);
            indices[i + 4] = (ushort) (offset + 3);
            indices[i + 5] = (ushort) (offset + 0);

            offset += 4;
        }
    }

    public static void FillQuadIndices<T>(Span<T> indices, uint offset) where T : unmanaged, INumber<T>
    {
        if (typeof(T) == typeof(ushort)) // If T is ushort will it call the overload instead
        {
            FillQuadIndices(MemoryMarshal.Cast<T, ushort>(indices), offset);
            return;
        }

        for (var i = 0; i < indices.Length; i += 6)
        {
            indices[i] =     T.CreateTruncating<uint>(offset + 0);
            indices[i + 1] = T.CreateTruncating<uint>(offset + 1);
            indices[i + 2] = T.CreateTruncating<uint>(offset + 2);
            indices[i + 3] = indices[i + 2]; // ^^
            indices[i + 4] = T.CreateTruncating<uint>(offset + 3);
            indices[i + 5] = indices[i]; // same as first

            offset += 4;
        }
    }
}