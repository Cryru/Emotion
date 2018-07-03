// Emotion - https://github.com/Cryru/Emotion

#region Using

using OpenTK;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.GLES
{
    /// <summary>
    /// A Vertex Buffer Object (VBO) is an OpenGL feature that provides methods for uploading vertex data (position, normal
    /// vector, color, etc.) to the video device for non-immediate-mode rendering.
    /// </summary>
    public class VBO
    {
        #region Properties

        internal int Pointer;

        /// <summary>
        /// The length of the uploaded vertex array.
        /// </summary>
        public int UploadedLength;

        #endregion

        /// <summary>
        /// Create a new VBO.
        /// </summary>
        public VBO()
        {
            // Generate a new vertex buffer object.
            Pointer = GL.GenBuffer();
        }

        public void Use()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Pointer);
        }

        public void Destroy()
        {
            GL.DeleteBuffer(Pointer);
        }

        public void StopUsing()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        #region Upload

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="vertices">The vertex data for this VBO.</param>
        public void Upload(Primitives.Vector2[] vertices)
        {
            Use();

            // Set the internal length variable.
            UploadedLength = vertices.Length;

            // Load the data into the buffer.
            GL.BufferData(BufferTarget.ArrayBuffer, Primitives.Vector2.SizeInBytes * vertices.Length, vertices, BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="vertices">The vertex data for this VBO.</param>
        public void Upload(Vector3[] vertices)
        {
            Use();

            // Set the internal length variable.
            UploadedLength = vertices.Length;

            // Calculate array size.
            int size = Vector3.SizeInBytes * vertices.Length;
            // Load the data into the buffer.
            GL.BufferData(BufferTarget.ArrayBuffer, size, vertices, BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Uploads the provided vertex data to the GPU.
        /// </summary>
        /// <param name="vertices">The vertex data for this VBO.</param>
        public void Upload(Vector4[] vertices)
        {
            Use();

            // Set the internal length variable.
            UploadedLength = vertices.Length;

            // Calculate array size.
            int size = Vector4.SizeInBytes * vertices.Length;
            // Load the data into the buffer.
            GL.BufferData(BufferTarget.ArrayBuffer, size, vertices, BufferUsageHint.DynamicDraw);
        }

        #endregion
    }
}