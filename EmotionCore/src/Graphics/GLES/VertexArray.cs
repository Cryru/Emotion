// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.System;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
{
    public sealed class VertexArray : IGLObject
    {
        private List<Buffer> _buffers = new List<Buffer>();

        /// <summary>
        /// The pointer of the vertex array.
        /// </summary>
        private int _pointer;

        public VertexArray()
        {
            _pointer = GL.GenVertexArray();
        }

        /// <summary>
        /// Attach a buffer to the vertex array.
        /// </summary>
        /// <param name="buffer">The buffer to attach to the vertex array.</param>
        /// <param name="index">The shader index of the buffer.</param>
        /// <param name="stride">The byte offset between consecutive generic vertex attributes</param>
        /// <param name="offset">The offset of the first piece of data.</param>
        /// <param name="componentCount">
        /// The component count of the buffer. If under or equal to 0 the component count within the
        /// buffer object will be used.
        /// </param>
        /// <param name="byteType">The type of data within the buffer. Float by default.</param>
        /// <param name="normalized">Whether the value is normalized.</param>
        public void AttachBuffer(Buffer buffer, int index, int stride = 0, int offset = 0, int componentCount = -1, VertexAttribPointerType byteType = VertexAttribPointerType.Float,
            bool normalized = false)
        {
            if (_pointer == -1) throw new Exception("Cannot add a buffer to a destroyed array.");
            if (componentCount <= 0) componentCount = (int) buffer.ComponentCount;

            ThreadManager.ExecuteGLThread(() =>
            {
                Bind();
                buffer.Bind();
                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(index, componentCount, byteType, normalized, stride, offset);
                buffer.Unbind();
                Unbind();

                _buffers.Add(buffer);
            });
        }

        /// <summary>
        /// Use the buffers attached to this vertex array.
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(_pointer);
        }

        /// <summary>
        /// Stop using any vertex array.
        /// </summary>
        public void Unbind()
        {
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Delete the array, freeing memory. Deletes attached buffers.
        /// </summary>
        public void Delete()
        {
            GL.DeleteVertexArray(_pointer);
            _pointer = -1;

            foreach (Buffer b in _buffers)
            {
                b.Delete();
            }

            _buffers.Clear();
            _buffers = null;
        }
    }
}