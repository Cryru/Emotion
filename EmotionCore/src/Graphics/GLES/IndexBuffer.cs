// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.System;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
{
    /// <summary>
    /// The index buffer is a list of pointers within the vertex buffer.
    /// </summary>
    public sealed class IndexBuffer : IGLObject
    {
        #region Properties

        /// <summary>
        /// The number of indices.
        /// </summary>
        public int Count { get; private set; }

        #endregion

        #region Static

        /// <summary>
        /// The currently bound buffer.
        /// </summary>
        public static int BoundPointer;

        #endregion

        /// <summary>
        /// The pointer of the buffer within OpenGL.
        /// </summary>
        private int _pointer;

        /// <summary>
        /// Create a new buffer.
        /// </summary>
        /// <param name="data">The initial data to upload.</param>
        public IndexBuffer(ushort[] data)
        {
            _pointer = GL.GenBuffer();
            Upload(data);
        }

        #region API

        /// <summary>
        /// Use this buffer for any following operations.
        /// </summary>
        public void Bind()
        {
            if(BoundPointer == _pointer) return;
            BoundPointer = _pointer;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _pointer);
        }

        /// <summary>
        /// Stop using any buffer.
        /// </summary>
        public void Unbind()
        {
            BoundPointer = 0;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        public void Upload(ushort[] data)
        {
            if (_pointer == -1) throw new Exception("Cannot upload data to a destroyed buffer.");

            Count = data.Length;
            ThreadManager.ExecuteGLThread(() =>
            {
                Bind();
                GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(ushort), data, BufferUsageHint.StaticDraw);
                Unbind();
            });
        }

        /// <summary>
        /// Delete the buffer and its data, freeing memory.
        /// </summary>
        public void Delete()
        {
            GL.DeleteBuffer(_pointer);
            _pointer = -1;
        }

        #endregion
    }
}