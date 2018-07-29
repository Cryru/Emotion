// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
{
    /// <summary>
    /// A Vertex Buffer Object (VBO) is an OpenGL feature that provides methods for uploading vertex data (position, normal
    /// vector, color, etc.) to the video device for non-immediate-mode rendering.
    /// </summary>
    public sealed class Buffer : IGLObject
    {
        #region Properties

        /// <summary>
        /// The number of components contained within the data.
        /// </summary>
        public uint ComponentCount { get; private set; }

        #endregion

        /// <summary>
        /// The pointer of the buffer within OpenGL.
        /// </summary>
        private int _pointer;

        /// <summary>
        /// Create a new buffer.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public Buffer(float[] data, uint componentCount, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            _pointer = GL.GenBuffer();
            Upload(data, componentCount, usageHint);
        }

        /// <summary>
        /// Create a new buffer, and allocate empty space for it.
        /// </summary>
        /// <param name="size">The size of buffer to allocate.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public Buffer(int size, uint componentCount, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            _pointer = GL.GenBuffer();
            Upload(size, componentCount, usageHint);
        }

        #region API

        /// <summary>
        /// Use this buffer for any following operations.
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _pointer);
        }

        /// <summary>
        /// Stop using any buffer.
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// Uploads an empty size buffer.
        /// </summary>
        /// <param name="size">The size to allocate.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(int size, uint componentCount, BufferUsageHint usageHint)
        {
            if (_pointer == -1) throw new Exception("Cannot allocate in a destroyed buffer.");

            ComponentCount = componentCount;

            ThreadManager.ExecuteGLThread(() =>
            {
                Bind();
                GL.BufferData(BufferTarget.ArrayBuffer, size, IntPtr.Zero, usageHint);
                Unbind();
            });
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(float[] data, uint componentCount, BufferUsageHint usageHint)
        {
            if (_pointer == -1) throw new Exception("Cannot upload data ot a destroyed buffer.");

            ComponentCount = componentCount;

            ThreadManager.ExecuteGLThread(() =>
            {
                Bind();
                GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usageHint);
                Unbind();
            });
        }

        /// <summary>
        /// Destroy the buffer and its data, freeing memory.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteBuffer(_pointer);
            _pointer = -1;
        }

        #endregion
    }
}