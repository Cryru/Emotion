// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Graphics.GLES;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.GLES.Buffer;

#endregion

namespace Emotion.Graphics
{
    public sealed unsafe class MapBuffer
    {
        #region Properties

        public Buffer VBO { get; private set; }
        public IndexBuffer Ibo { get; private set; }
        public VertexArray VAO { get; private set; }

        #endregion

        #region Draw State

        private VertexData* _dataPointer;

        #endregion

        /// <summary>
        /// Create a new map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer.</param>
        public MapBuffer(int size)
        {
            VBO = new Buffer(size, 3, BufferUsageHint.DynamicDraw);
            VAO = new VertexArray();

            VAO.Bind();
            VBO.Bind();

            GL.EnableVertexAttribArray(ShaderProgram.VertexLocation);
            GL.VertexAttribPointer(ShaderProgram.VertexLocation, 3, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"));

            GL.EnableVertexAttribArray(ShaderProgram.UvLocation);
            GL.VertexAttribPointer(ShaderProgram.UvLocation, 2, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "UV"));

            //GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
            //GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));

            GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
            GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));

            VBO.Unbind();
            VAO.Unbind();

            Helpers.CheckError("map buffer - loading vbo into vao");

            ushort[] indices = new ushort[size * 6];
            ushort offset = 0;
            for (int i = 0; i < indices.Length; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }

            Ibo = new IndexBuffer(indices);

            Helpers.CheckError("map buffer - creating ibo");
        }

        /// <summary>
        /// Start mapping the buffer.
        /// </summary>
        public VertexData* Start()
        {
            VBO.Bind();
            _dataPointer = (VertexData*) GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, VertexData.SizeInBytes, BufferAccessMask.MapWriteBit);
            Helpers.CheckError("map buffer - start");

            return _dataPointer;
        }

        /// <summary>
        /// Finish mapping the buffer and draw.
        /// </summary>
        /// <param name="mappedIndices">The number of indices mapped.</param>
        /// <param name="primitiveType">The type of primitive to draw as.</param>
        public void Draw(int mappedIndices, PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            VBO.Unbind();

            VAO.Bind();
            Ibo.Bind();

            GL.DrawElements(primitiveType, mappedIndices, DrawElementsType.UnsignedShort, IntPtr.Zero);
            Helpers.CheckError("map buffer - draw");

            Ibo.Unbind();
            VAO.Unbind();

            Helpers.CheckError("map buffer - flush");
        }

        /// <summary>
        /// Destroy the map buffer freeing resources.
        /// </summary>
        public void Destroy()
        {
            VBO?.Destroy();
            Ibo?.Destroy();
            VAO?.Destroy();
        }
    }
}