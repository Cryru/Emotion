#region Using

using System.Runtime.InteropServices;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.WIPUpdates.Rendering;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects;

public abstract class VertexArrayObject : IDisposable
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
    /// The offset of the UV field within the structure. The UV field is expected to be at least a Vec2.
    /// </summary>
    public int UVByteOffset { get; set; }

    private uint _lastAttributePosition;

    /// <summary>
    /// Ensures the provided pointer is the currently bound vertex array buffer.
    /// </summary>
    /// <param name="vao">The vertex array object to ensure is bound.</param>
    public static void EnsureBound(VertexArrayObject vao)
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
        if (vao?.IBO != null) IndexBuffer.EnsureBound(vao.IBO.Pointer);
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

    /// <summary>
    /// Setup vertex attributes.
    /// </summary>
    protected void SetupAttributes<T>() where T : struct
    {
        ByteSize = Marshal.SizeOf<T>();

        EnsureBound(this);
        if (VBO != null) VertexBuffer.EnsureBound(VBO.Pointer);
        ApplyVertexAttributes<T>();
    }

    /// <summary>
    /// Append vertex attributes to the VAO.
    /// </summary>
    public void AppendType<T>(VertexBuffer vboExt) where T : struct
    {
        EnsureBound(this);
        VertexBuffer.EnsureBound(vboExt.Pointer);
        ApplyVertexAttributes<T>();
    }

    protected void ApplyVertexAttributes<T>() where T : struct
    {
        int stride = Marshal.SizeOf<T>();
        var vertexType = typeof(T);

        var typeData = ReflectorEngine.GetTypeHandler(vertexType) as IGenericReflectorComplexTypeHandler;
        AssertNotNull(typeData);
        ComplexTypeHandlerMember[] members = typeData.GetMembers();

        uint positionOffset = _lastAttributePosition;
        for (uint i = 0; i < members.Length; i++)
        {
            Standard.Reflector.Handlers.ComplexTypeHandlerMember member = members[i];
            VertexAttributeAttribute vertexAttributeData = member.HasAttribute<VertexAttributeAttribute>();
            if (vertexAttributeData == null) continue;

            string fieldName = member.Name;
            nint offset = Marshal.OffsetOf(vertexType, fieldName);
            Type fieldType = vertexAttributeData.TypeOverride ?? member.Type;
            if (fieldName == "UV") UVByteOffset = (int) offset;

            uint position = positionOffset + i;
            Gl.EnableVertexAttribArray(position);
            Gl.VertexAttribPointer(position, vertexAttributeData.ComponentCount, GetAttribTypeFromManagedType(fieldType), vertexAttributeData.Normalized, stride, offset);

            _lastAttributePosition = position + 1;
        }
    }

    public void Dispose()
    {
        uint ptr = Pointer;
        Pointer = 0;

        if (Engine.Host == null) return;
        if (ptr == Bound) Bound = 0;

        GLThread.ExecuteGLThreadAsync(() => { Gl.DeleteVertexArrays(ptr); });
    }
}

#pragma warning disable CS0162 // Unreachable code detected
/// <summary>
/// An OpenGL object representing the vertex format.
/// Sadly is bound to a specific buffer instead of being a reusable format,
/// unless you use new features.
/// </summary>
public class VertexArrayObjectFromFormat : VertexArrayObject
{
    /// <summary>
    /// The Emotion format of the vertices.
    /// </summary>
    public VertexDataFormat Format { get; init; }

    public VertexArrayObjectFromFormat(VertexDataFormat format, VertexBuffer vbo)
    {
        Format = format;
        Assert(format.Built);

        ByteSize = format.ElementSize;

        Pointer = Gl.GenVertexArray();
        VBO = vbo;

        EnsureBound(this);
        IndexBuffer.EnsureBound(0);
        VertexBuffer.EnsureBound(VBO.Pointer);

        SetupAttributes();
        EnsureBound(null);
    }

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
                Gl.VertexAttribFormat(position, 4, (int)VertexAttribType.Float, false, (uint)byteOffset);
                Gl.VertexAttribBinding(position, 0);
            }
            else
            {
                Gl.VertexAttribPointer(position, 4, VertexAttribType.Float, false, byteStride + sizeof(float) * 4, byteOffset);
            }

            position++;
        }
    }
}
#pragma warning restore CS0162 // Unreachable code detected

/// <summary>
/// Create a vertex array object describing the vertex layout
/// provided as a C# value struct.
/// </summary>
public sealed class VertexArrayObject<T> : VertexArrayObject where T : struct
{
    public VertexArrayObject(VertexBuffer vbo, IndexBuffer ibo = null)
    {
        Pointer = Gl.GenVertexArray();
        VBO = vbo;
        IBO = ibo;
        SetupAttributes<T>();
        EnsureBound(null);
    }
}