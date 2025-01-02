#region Using

using System.Runtime.InteropServices;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
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