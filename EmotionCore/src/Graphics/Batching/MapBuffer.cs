// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.GLES.Buffer;

#endregion

namespace Emotion.Graphics.Batching
{
    public abstract unsafe class MapBuffer
    {
        #region Properties

        /// <summary>
        /// Whether the buffer is currently mapping. Call Draw() to finish mapping.
        /// </summary>
        public bool Mapping
        {
            get => _dataPointer != null;
        }

        /// <summary>
        /// Whether anything is currently mapped into the buffer.
        /// </summary>
        public bool AnythingMapped
        {
            get => _indicesCount != 0;
        }

        /// <summary>
        /// The size of the buffer in vertices.
        /// </summary>
        public int Size { get; private set; }

        #endregion

        #region Draw State

        /// <summary>
        /// The VBO holding the buffer data of this map buffer.
        /// </summary>
        protected Buffer _vbo;

        /// <summary>
        /// The VAO holding the buffer vertex attribute bindings for this map buffer.
        /// </summary>
        protected VertexArray _vao;

        /// <summary>
        /// The pointer currently being mapped to.
        /// </summary>
        protected VertexData* _dataPointer;

        /// <summary>
        /// The number of indices mapped.
        /// </summary>
        protected int _indicesCount;

        #endregion

        #region Initialization and Deletion

        /// <summary>
        /// Create a new map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer in vertices.</param>
        protected MapBuffer(int size)
        {
            Size = size;

            // Calculate the size of the buffer.
            int quadSize = VertexData.SizeInBytes * 4;
            int bufferSize = size * quadSize;

            ThreadManager.ExecuteGLThread(() =>
            {
                _vbo = new Buffer(bufferSize, 3, BufferUsageHint.DynamicDraw);
                _vao = new VertexArray();

                _vao.Bind();
                _vbo.Bind();

                // todo: Move VAO creation to inheritors of the MapBuffer.
                GL.EnableVertexAttribArray(ShaderProgram.VertexLocation);
                GL.VertexAttribPointer(ShaderProgram.VertexLocation, 3, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"));

                GL.EnableVertexAttribArray(ShaderProgram.UvLocation);
                GL.VertexAttribPointer(ShaderProgram.UvLocation, 2, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "UV"));

                GL.EnableVertexAttribArray(ShaderProgram.TidLocation);
                GL.VertexAttribPointer(ShaderProgram.TidLocation, 1, VertexAttribPointerType.Float, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Tid"));

                GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
                GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));

                _vbo.Unbind();
                _vao.Unbind();

                Helpers.CheckError("map buffer - loading vbo into vao");
            });
        }

        /// <summary>
        /// Destroy the map buffer freeing resources.
        /// </summary>
        public void Delete()
        {
            ThreadManager.ForceGLThread();
            _vbo?.Delete();
            _vao?.Delete();
        }

        #endregion

        #region Mapping

        public virtual void Start()
        {
            ThreadManager.ForceGLThread();

            _indicesCount = 0;

            Helpers.CheckError("map buffer - before start");
            _vbo.Bind();
            _dataPointer = (VertexData*) GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, VertexData.SizeInBytes, BufferAccessMask.MapWriteBit);
            Helpers.CheckError("map buffer - start");
        }

        /// <summary>
        /// Finish mapping the buffer.
        /// </summary>
        public virtual void FinishMapping()
        {
            if (!Mapping)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to finish mapping a buffer which never started mapping.");
                return;
            }

            ThreadManager.ForceGLThread();

            _dataPointer = null;

            Helpers.CheckError("map buffer - before unmapping");
            _vbo.Bind();
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            Helpers.CheckError("map buffer - unmapping");
        }

        #endregion

        /// <summary>
        /// Draw the buffer.
        /// </summary>
        /// <param name="bufferMatrix">The matrix4 to upload as an uniform for "bufferMatrix". If null nothing will be uploaded.</param>
        /// <param name="shader">The shader to use. If null the current one will be used.</param>
        public abstract void Draw(Matrix4? bufferMatrix = null, ShaderProgram shader = null);
    }
}