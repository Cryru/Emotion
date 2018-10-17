// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Debug;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.GLES.Buffer;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Graphics.Batching
{
    public abstract unsafe class MapBuffer : Buffer, IRenderable
    {
        #region Properties

        /// <summary>
        /// Whether the buffer is currently mapping. Call Draw() to finish mapping.
        /// </summary>
        public bool Mapping
        {
            get => _startPointer != null && _dataPointer != null;
        }

        /// <summary>
        /// Whether anything is currently mapped into the buffer.
        /// </summary>
        public bool AnythingMapped
        {
            get => MappedVertices != 0;
        }

        /// <summary>
        /// The number of vertices mapped. Also the index of the highest mapped vertex.
        /// </summary>
        public int MappedVertices { get; private set; }

        /// <summary>
        /// The number of objects mapped.
        /// </summary>
        public int MappedObjects
        {
            get => MappedVertices / 4;
        }

        /// <summary>
        /// The size of individual objects which will be mapped in vertices.
        /// </summary>
        public int ObjectSize { get; private set; }

        /// <summary>
        /// The number of indices per object.
        /// </summary>
        public int IndicesPerObject { get; private set; }

        /// <summary>
        /// The total number of objects that can fit in the buffer.
        /// </summary>
        public int SizeInObjects
        {
            get => SizeInVertices / ObjectSize;
        }

        /// <summary>
        /// The total number of vertices that can fit in the buffer.
        /// </summary>
        public int SizeInVertices
        {
            get => Size / VertexData.SizeInBytes;
        }

        #endregion

        #region Private Objects

        /// <summary>
        /// The VAO holding the buffer vertex attribute bindings for this map buffer.
        /// </summary>
        private VertexArray _vao;

        /// <summary>
        /// The index buffer to use when drawing.
        /// </summary>
        private IndexBuffer _ibo;

        /// <summary>
        /// The primitive type to draw with.
        /// </summary>
        private PrimitiveType _drawType;

        #endregion

        #region Drawing State

        /// <summary>
        /// The index to start drawing from.
        /// </summary>
        private int _startIndex;

        /// <summary>
        /// The index to stop drawing at.
        /// </summary>
        private int _endIndex = -1;

        /// <summary>
        /// The point where the data starts.
        /// </summary>
        protected VertexData* _startPointer;

        /// <summary>
        /// The pointer currently being mapped to.
        /// </summary>
        protected VertexData* _dataPointer;

        /// <summary>
        /// The list of textures the buffer TIDs (Texture IDs) require, in the correct order.
        /// </summary>
        private List<Texture> _textureList;

        #endregion

        #region Initialization and Deletion

        /// <summary>
        /// Create a new map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer in objects.</param>
        /// <param name="objectSize">The size of individual objects which will be mapped in vertices.</param>
        /// <param name="ibo">The index buffer to use when drawing.</param>
        /// <param name="indicesPerObject">The number of indices per object.</param>
        /// <param name="drawType">The OpenGL primitive type to draw this buffer with.</param>
        protected MapBuffer(int size, int objectSize, IndexBuffer ibo, int indicesPerObject, PrimitiveType drawType) : base(size * objectSize * VertexData.SizeInBytes, 3, BufferUsageHint.DynamicDraw)
        {
            ObjectSize = objectSize;
            _ibo = ibo;
            IndicesPerObject = indicesPerObject;
            _drawType = drawType;

            _vao = new VertexArray();

            _vao.Bind();
            base.Bind();

            GL.EnableVertexAttribArray(ShaderProgram.VertexLocation);
            GL.VertexAttribPointer(ShaderProgram.VertexLocation, 3, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Vertex"));

            GL.EnableVertexAttribArray(ShaderProgram.UvLocation);
            GL.VertexAttribPointer(ShaderProgram.UvLocation, 2, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "UV"));

            GL.EnableVertexAttribArray(ShaderProgram.TidLocation);
            GL.VertexAttribPointer(ShaderProgram.TidLocation, 1, VertexAttribPointerType.Float, true, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Tid"));

            GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
            GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte)Marshal.OffsetOf(typeof(VertexData), "Color"));

            base.Unbind();
            _vao.Unbind();

            Helpers.CheckError("map buffer - loading vbo into vao");

            _textureList = new List<Texture>();
        }

        /// <summary>
        /// Destroy the map buffer freeing resources.
        /// </summary>
        public new void Delete()
        {
            ThreadManager.ForceGLThread();
            base.Delete();
            _vao?.Delete();
        }

        #endregion

        #region Buffer API Overwrite

        public new void Bind()
        {
            throw new InvalidOperationException("You cannot bind a map buffer.");
        }

        public new void Unbind()
        {
            throw new InvalidOperationException("You cannot unbind a map buffer.");
        }

        public new void Upload(int size, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        public new void Upload(float[] data, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        public new void Upload(uint[] data, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        public new void Upload(Vector3[] data, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        public new void Upload(Vector2[] data, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        #endregion

        #region Mapping

        /// <summary>
        /// Start mapping the buffer. Does not have to be explicitly called before mapping.
        /// </summary>
        public virtual void StartMapping()
        {
            if (Mapping)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to start mapping a buffer which is already mapping.");
                return;
            }

            ThreadManager.ForceGLThread();

            Helpers.CheckError("map buffer - before start");
            base.Bind();
            _startPointer = (VertexData*)GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, VertexData.SizeInBytes, BufferAccessMask.MapWriteBit);
            _dataPointer = _startPointer;
            Helpers.CheckError("map buffer - start");
        }

        /// <summary>
        /// Maps the current vertex and advanced the current index by one.
        /// </summary>
        /// <param name="vertex">The location of the vertex AKA the vertex itself.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex, if any.</param>
        /// <param name="uv">The uv of the vertex's texture, if any.</param>
        public void MapNextVertex(Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            InternalMapVertex(ColorToUint(color), GetTid(texture), VerifyUV(texture, uv), vertex);
            _dataPointer++;
        }

        /// <summary>
        /// Moves the pointer to the specified index and maps the vertex.
        /// </summary>
        /// <param name="index">The index of the vertex to map.</param>
        /// <param name="vertex">The location of the vertex AKA the vertex itself.</param>
        /// <param name="color">The color of the vertex.</param>
        /// <param name="texture">The texture of the vertex, if any.</param>
        /// <param name="uv">The uv of the vertex's texture, if any.</param>
        public void MapVertexAt(int index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map the vertex.
            MovePointerToVertex(index);
            MapNextVertex(vertex, color, texture, uv);
        }

        /// <summary>
        /// Finish mapping the buffer, flushing changes to the GPU.
        /// </summary>
        private void Flush()
        {
            if (!Mapping)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Tried to finish mapping a buffer which never started mapping.");
                return;
            }

            ThreadManager.ForceGLThread();

            _startPointer = null;
            _dataPointer = null;

            Helpers.CheckError("map buffer - before unmapping");
            base.Bind();
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            Helpers.CheckError("map buffer - unmapping");
        }

        /// <summary>
        /// Resets the buffer's mapping. Does not actually clear anything but rather resets the mapping trackers and cached textures.
        /// </summary>
        public void Reset()
        {
            _textureList.Clear();
            MappedVertices = 0;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Moves the pointer to the specified vertex index.
        /// </summary>
        /// <param name="index">The index to move the pointer to.</param>
        protected void MovePointerToVertex(int index)
        {
            _dataPointer = _startPointer + index;
        }

        /// <summary>
        /// Converts an Emotion color object to an uint. todo: Move to color class.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns></returns>
        protected static uint ColorToUint(Color color)
        {
            return ((uint)color.A << 24) | ((uint)color.B << 16) | ((uint)color.G << 8) | color.R;
        }

        /// <summary>
        /// Maps a vertex.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="tid"></param>
        /// <param name="uv"></param>
        /// <param name="vertex"></param>
        protected void InternalMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex)
        {
            long currentVertex = (_dataPointer - _startPointer);

            // Check if going out of bounds.
            if (currentVertex > SizeInVertices)
            {
                Debugger.Log(MessageType.Error, MessageSource.GL, $"Exceeding total vertices ({SizeInVertices}) in map buffer {_pointer}.");
                return;
            }

            // Check if indices are going out of bounds.
            if (currentVertex / ObjectSize > _ibo.Count / IndicesPerObject)
            {
                Debugger.Log(MessageType.Error, MessageSource.GL, $"Exceeding total indices ({_ibo.Count}) in map buffer {_pointer}.");
                return;
            }

            _dataPointer->Color = color;
            _dataPointer->Tid = tid;
            _dataPointer->UV = uv;
            _dataPointer->Vertex = vertex;

            currentVertex++;
            // Check if the mapped vertices count needs to be updated.
            if (currentVertex > MappedVertices) MappedVertices = (int)currentVertex;
        }

        #endregion

        #region Texturing

        /// <summary>
        /// Returns the texture id of the specified texture within the buffer.
        /// </summary>
        /// <param name="texture">The texture whose id to return</param>
        /// <returns>The id of the texture.</returns>
        protected float GetTid(Texture texture)
        {
            // If no texture.
            if (texture == null) return -1;

            float tid = -1;

            // Check if the texture is in the list of loaded textures.
            for (int i = 0; i < _textureList.Count; i++)
            {
                if (_textureList[i].Pointer != texture.Pointer) continue; // todo: Try comparing references instead of pointers.
                tid = i;
                break;
            }

            // If not add it.
            if (tid == -1)
            {
                // Check if there is space for adding.
                if (_textureList.Count >= 16) throw new Exception("Texture limit of 16 per buffer reached.");

                _textureList.Add(texture);
                tid = _textureList.Count - 1;
            }

            return tid;
        }

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uv">The uv to verify.</param>
        /// <returns></returns>
        protected virtual Vector2 VerifyUV(Texture texture, Vector2? uv)
        {
            // If no texture, the uv is empty.
            if (texture == null) return Vector2.Zero;

            return uv ?? Vector2.One;
        }

        /// <summary>
        /// Bind all loaded textures for rendering.
        /// </summary>
        private void BindTextures()
        {
            // Bind textures.
            for (int i = 0; i < _textureList.Count; i++)
            {
                _textureList[i].Bind(i);
            }


            Helpers.CheckError("map buffer - texture binding");
        }

        #endregion

        /// <summary>
        /// Set the render range for the buffer in objects.
        /// </summary>
        /// <param name="startIndex">The index of the object to start drawing from.</param>
        /// <param name="endIndex">The index of the object to stop drawing at. If -1 will draw to MappedObjects.</param>
        public void SetRenderRange(int startIndex = 0, int endIndex = -1)
        {
            if (startIndex != 0)
            {
                startIndex = startIndex * ObjectSize;
            }

            if (endIndex != -1)
            {
                endIndex = endIndex * ObjectSize;
            }

            SetRenderRangeVertices(startIndex, endIndex);
        }

        /// <summary>
        /// Set the render range for the buffer in vertices.
        /// </summary>
        /// <param name="startIndex">The index of the vertex to start drawing from.</param>
        /// <param name="endIndex">The index of the vertex to stop drawing at. If -1 will draw to MappedVertices.</param>
        public void SetRenderRangeVertices(int startIndex = 0, int endIndex = -1)
        {
            _startIndex = startIndex;
            _endIndex = endIndex;

            // Check offset.
            if (_startIndex >= MappedVertices)
            {
                Debugger.Log(MessageType.Warning, MessageSource.GL, $"Map buffer startIndex {_startIndex} is beyond mapped vertices - {MappedVertices}.");
                _startIndex = 0;
            }

            if (_endIndex > Size)
            {
                Debugger.Log(MessageType.Warning, MessageSource.GL, $"Map buffer endIndex {_endIndex} is beyond size - {Size}.");
                _endIndex = MappedVertices;
            }

            if (_startIndex > _endIndex)
            {
                Debugger.Log(MessageType.Warning, MessageSource.GL, $"Map buffer startIndex {_startIndex} is beyond endIndex - {_endIndex}.");
                _startIndex = 0;
                _endIndex = MappedVertices;
            }
        }

        /// <summary>
        /// Render the buffer.
        /// </summary>
        /// <param name="renderer">To renderer to render the buffer with.</param>
        public virtual void Render(Renderer renderer)
        {
            ThreadManager.ForceGLThread();

            // Check if anything is mapped.
            if (!AnythingMapped) return;

            // Check if mapping, in which case a flush is needed.
            if (Mapping) Flush();

            // Load textures.
            BindTextures();

            _vao.Bind();
            _ibo.Bind();
            Helpers.CheckError("map buffer - bind");

            // Convert offset amd length.
            IntPtr indexToPointer = (IntPtr)(_startIndex * sizeof(ushort));
            int length = _endIndex == -1 ? MappedObjects * IndicesPerObject : _endIndex / ObjectSize * IndicesPerObject;

            GL.DrawElements(_drawType, length, DrawElementsType.UnsignedShort, indexToPointer);
            Helpers.CheckError("map buffer - draw");

            _ibo.Unbind();
            _vao.Unbind();
            Helpers.CheckError("map buffer - unbind");
        }
    }
}