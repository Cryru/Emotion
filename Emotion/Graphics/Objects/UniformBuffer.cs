#nullable enable

using OpenGL;

namespace Emotion.Graphics.Objects;

/// <summary>
/// A buffer holding uniform data for shaders.
/// https://wikis.khronos.org/opengl/Uniform_Buffer_Object
/// </summary>
public sealed class UniformBuffer : DataBuffer
{
    public new static uint Bound
    {
        get => DataBuffer.Bound[BufferTarget.UniformBuffer];
        set => DataBuffer.Bound[BufferTarget.UniformBuffer] = value;
    }

    public UniformBuffer(uint byteSize = 0, BufferUsage usage = BufferUsage.DynamicDraw)
        : base(BufferTarget.UniformBuffer, byteSize, usage)
    {
    }

    public static void EnsureBound(uint pointer)
    {
        EnsureBound(pointer, BufferTarget.UniformBuffer);
    }

    /// <summary>
    /// Binds this buffer binding point specified in the shader.
    /// </summary>
    public void SetBindingPoint(uint bindingPoint)
    {
        Gl.BindBufferBase(BufferTarget.UniformBuffer, bindingPoint, Pointer);
    }
}
