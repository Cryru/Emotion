#nullable enable

#region Using

using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Data;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects;

public class VertexArrayObject : IDisposable
{
    /// <summary>
    /// The OpenGL pointer to this VertexArrayObject.
    /// </summary>
    public uint Pointer { get; set; }

    /// <summary>
    /// The bound vertex array buffer.
    /// </summary>
    public static uint Bound;

    /// <summary>
    /// The vertex buffer attached to this vertex array object.
    /// When the vao is bound, this vbo is bound automatically.
    /// </summary>
    public VertexBuffer VBO { get; protected set; }

    /// <summary>
    /// The ibo attached to this vertex array object.
    /// When the vao is bound, this ibo is bound automatically.
    /// </summary>
    public IndexBuffer IBO { get; protected set; }

    /// <summary>
    /// The byte size of one instance of the structure.
    /// </summary>
    public int ByteSize { get; set; }

    /// <summary>
    /// The format of the vertex struct
    /// </summary>
    public VertexDataFormat Format { get; init; }

    /// <summary>
    /// The offset of the UV field within the structure. The UV field is expected to be at least a Vec2.
    /// </summary>
    public int UVByteOffset { get; set; }

    public VertexArrayObject(VertexDataFormat format, VertexBuffer vbo, IndexBuffer? ibo = null)
    {
        Format = format;
        Assert(format.Built);

        format.GetUVOffsetAndStride(0, out int offset, out int _);
        UVByteOffset = offset; // Cache this as it is used by the render stream's uv remapping
        ByteSize = format.ElementSize;

        Pointer = Gl.GenVertexArray();
        VBO = vbo;
        IBO = ibo;

        EnsureBound(this);
        if (ibo == null) IndexBuffer.EnsureBound(0);
        VertexBuffer.EnsureBound(VBO.Pointer);

        SetupAttributes();
        EnsureBound(null);
    }

#pragma warning disable CS0162 // Unreachable code detected
    private void SetupAttributes()
    {
        // OpenGL 4.3 or GL_ARB_vertex_attrib_binding
        // In this case we can reuse VAOs for multiple buffers
        // On mobile and web we might be clear to use this due to ES 3.1 requirement from elsewhere (?)
        const bool separateVertexAttributes = false;

        if (separateVertexAttributes)
            VertexBuffer.EnsureBound(0);

        // This code needs to match the format specification (which is a single one for Emotion).
        // If the order in the VertexDataFormat changes we need to change this too.
        uint position = 0;
        if (Format.HasPosition)
        {
            Format.GetVertexPositionOffsetAndStride(out int byteOffset, out int byteStride);

            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 3, (int)VertexAttribType.Float, false, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 3, VertexAttribType.Float, false, byteStride, byteOffset);
            }

            position++;
        }

        for (int i = 0; i < Format.HasUVCount; i++)
        {
            Format.GetUVOffsetAndStride(i, out int byteOffset, out int byteStride);

            // Vector2
            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 2, (int)VertexAttribType.Float, false, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 2, VertexAttribType.Float, false, byteStride, byteOffset);
            }

            position++;
        }

        if (Format.HasNormals)
        {
            Format.GetNormalOffsetAndStride(out int byteOffset, out int byteStride);

            // Vector3
            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 3, (int)VertexAttribType.Float, false, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 3, VertexAttribType.Float, false, byteStride, byteOffset);
            }

            position++;
        }

        if (Format.HasVertexColors)
        {
            Format.GetVertexColorsOffsetAndStride(out int byteOffset, out int byteStride);

            // Normalized uint to vec4
            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 4, (int)VertexAttribType.UnsignedByte, true, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 4, VertexAttribType.UnsignedByte, true, byteStride, byteOffset);
            }

            position++;
        }

        if (Format.HasBones)
        {
            Format.GetBoneDataOffsetAndStride(out int byteOffset, out int byteStride);

            // Vector4 BoneIds
            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 4, (int)VertexAttribType.Float, false, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 4, VertexAttribType.Float, false, byteStride, byteOffset);
            }

            position++;

            // Vector4 BoneWeights
            Gl.EnableVertexAttribArray(position);
            if (separateVertexAttributes)
            {
                Gl.VertexAttribFormat(position, 4, (int)VertexAttribType.Float, false, (uint)byteOffset + sizeof(float) * 4);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 4, VertexAttribType.Float, false, byteStride, byteOffset + sizeof(float) * 4);
            }

            position++;
        }

        if (Format.CustomData != null)
        {
            int byteStride = Format.ElementSize;
            int byteOffset = Format.CustomDataByteOffset;

            foreach (var customDataElements in Format.CustomData)
            {
                Gl.EnableVertexAttribArray(position);
                Gl.VertexAttribPointer(position, customDataElements, VertexAttribType.Float, false, byteStride, byteOffset);
                position++;

                byteOffset += customDataElements;
            }
        }
    }
#pragma warning restore CS0162 // Unreachable code detected

    /// <summary>
    /// Ensures the provided pointer is the currently bound vertex array buffer.
    /// </summary>
    /// <param name="vao">The vertex array object to ensure is bound.</param>
    public static void EnsureBound(VertexArrayObject? vao)
    {
        // No binding cache, check https://github.com/Cryru/Emotion/issues/56

        uint ptr = vao?.Pointer ?? 0;
        Gl.BindVertexArray(ptr);
        Bound = ptr;

        // Reset the cached binding of VertexBuffers and IndexBuffers as the VAO just bound something else.
        // This does disable the cache until the binding, but otherwise we will be binding twice.
        VertexBuffer.Bound = uint.MaxValue;
        IndexBuffer.Bound = uint.MaxValue;

        // Some Intel drivers don't bind the IBO when the VAO is bound.
        // https://stackoverflow.com/questions/8973690/vao-and-element-array-buffer-state
        if (vao?.IBO != null)
            IndexBuffer.EnsureBound(vao.IBO.Pointer);
    }

    /// <summary>
    /// Convert a managed C# type to a GL vertex attribute type.
    /// </summary>
    /// <param name="type">The managed type to convert.</param>
    /// <returns>The vertex attribute type corresponding to the provided managed type.</returns>
    public static VertexAttribType GetAttribTypeFromManagedType(Type type)
    {
        if (type == typeof(int))
            return VertexAttribType.Int;
        if (type == typeof(uint))
            return VertexAttribType.UnsignedInt;
        if (type == typeof(byte))
            return VertexAttribType.UnsignedByte;
        if (type == typeof(sbyte)) return VertexAttribType.Byte;

        return VertexAttribType.Float;
    }

    public void Dispose()
    {
        uint ptr = Pointer;
        Pointer = 0;

        if (Engine.Host == null) return;
        if (ptr == Bound) Bound = 0;

        GLThread.ExecuteOnGLThreadAsync((uint ptr) => Gl.DeleteVertexArrays(ptr), ptr);
    }
}