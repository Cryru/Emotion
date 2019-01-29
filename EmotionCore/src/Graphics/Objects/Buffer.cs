// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// A Vertex Buffer Object (VBO) is an OpenGL feature that provides methods for uploading vertex data (position, normal
    /// vector, color, etc.) to the video device for non-immediate-mode rendering.
    /// Legacy object.
    /// </summary>
    [Obsolete]
    public class Buffer : IGLObject
    {
        #region Properties

        /// <summary>
        /// The number of components contained within the data.
        /// </summary>
        public uint ComponentCount { get; private set; }

        /// <summary>
        /// The size of the buffer in vertices.
        /// </summary>
        public int Size { get; private set; }

        #endregion

        #region Static

        /// <summary>
        /// The currently bound buffer.
        /// </summary>
        public static int BoundPointer { get; internal set; }

        #endregion

        /// <summary>
        /// The pointer of the buffer within OpenGL.
        /// </summary>
        internal uint _pointer { get; private set; }

        /// <summary>
        /// Create a new buffer, and allocate empty space for it.
        /// </summary>
        /// <param name="size">The size of buffer to allocate.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        public Buffer(int size, uint componentCount)
        {
            Size = size;
            _pointer = GraphicsManager.CreateDataBuffer((uint) size);
        }

        #region API

        /// <summary>
        /// Use this buffer for any following operations.
        /// </summary>
        public void Bind()
        {
            GraphicsManager.BindDataBuffer(_pointer);
        }

        /// <summary>
        /// Stop using any buffer.
        /// </summary>
        public void Unbind()
        {
            GraphicsManager.BindDataBuffer(0);
        }

        /// <summary>
        /// Uploads an empty size buffer.
        /// </summary>
        /// <param name="size">The size to allocate.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(int size, uint componentCount, BufferUsageHint usageHint)
        {
            byte[] empty = new byte[size];
            Bind();
            GraphicsManager.UploadToDataBuffer(empty);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(float[] data, uint componentCount, BufferUsageHint usageHint)
        {
            Bind();
            GraphicsManager.UploadToDataBuffer(data);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="componentCount">The number of components contained within the data.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(uint[] data, uint componentCount, BufferUsageHint usageHint)
        {
            Bind();
            GraphicsManager.UploadToDataBuffer(data);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(Vector3[] data, BufferUsageHint usageHint)
        {
            Bind();
            GraphicsManager.UploadToDataBuffer(data);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="data">The vertex data for this VBO.</param>
        /// <param name="usageHint">What the buffer will be used for.</param>
        public void Upload(Vector2[] data, BufferUsageHint usageHint)
        {
            Bind();
            GraphicsManager.UploadToDataBuffer(data);
        }

        /// <summary>
        /// Delete the buffer and its data, freeing memory.
        /// </summary>
        public void Delete()
        {
            GraphicsManager.DestroyDataBuffer(_pointer);
            _pointer = 0;
        }

        #endregion
    }
}