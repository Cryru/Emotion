// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.Objects.Buffer;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// A buffer which supports mapping to.
    /// </summary>
    public unsafe class MapBuffer : Buffer, IRenderable
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
        /// The number of total vertices mapped. Also the index of the highest mapped vertex.
        /// </summary>
        public int MappedVertices { get; private set; }

        /// <summary>
        /// The number of objects mapped.
        /// </summary>
        public int MappedObjects
        {
            get => MappedVertices / ObjectSize;
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
        /// The default IBO.
        /// </summary>
        private static IndexBuffer _defaultIbo;

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

        static MapBuffer()
        {
            // Generate indices.
            ushort[] indices = new ushort[Renderer.MaxRenderable * 6];
            uint offset = 0;
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

            _defaultIbo = new IndexBuffer(indices);

            GLThread.CheckError("map buffer - creating ibo");
        }

        /// <summary>
        /// Create a new map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer in objects.</param>
        /// <param name="objectSize">The size of individual objects which will be mapped in vertices.</param>
        /// <param name="ibo">The index buffer to use when drawing. If null the default triangle one will be used.</param>
        /// <param name="indicesPerObject">The number of indices per object.</param>
        /// <param name="drawType">The OpenGL primitive type to draw this buffer with.</param>
        protected MapBuffer(int size, int objectSize, IndexBuffer ibo, int indicesPerObject, PrimitiveType drawType) : base(size * objectSize * VertexData.SizeInBytes, 3, BufferUsageHint.DynamicDraw)
        {
            ObjectSize = objectSize;
            _ibo = ibo ?? _defaultIbo;
            IndicesPerObject = indicesPerObject;
            _drawType = drawType;

            _vao = new VertexArray();

            GLThread.ExecuteGLThread(() =>
            {
                _vao.Bind();
                Bind();

                GL.EnableVertexAttribArray(ShaderProgram.VertexLocation);
                GL.VertexAttribPointer(ShaderProgram.VertexLocation, 3, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Vertex"));

                GL.EnableVertexAttribArray(ShaderProgram.UvLocation);
                GL.VertexAttribPointer(ShaderProgram.UvLocation, 2, VertexAttribPointerType.Float, false, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "UV"));

                GL.EnableVertexAttribArray(ShaderProgram.TidLocation);
                GL.VertexAttribPointer(ShaderProgram.TidLocation, 1, VertexAttribPointerType.Float, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Tid"));

                GL.EnableVertexAttribArray(ShaderProgram.ColorLocation);
                GL.VertexAttribPointer(ShaderProgram.ColorLocation, 4, VertexAttribPointerType.UnsignedByte, true, VertexData.SizeInBytes, (byte) Marshal.OffsetOf(typeof(VertexData), "Color"));

                Unbind();
                _vao.Unbind();

                GLThread.CheckError("map buffer - loading vbo into vao");
            });

            _textureList = new List<Texture>();
        }

        /// <summary>
        /// Destroy the map buffer freeing resources.
        /// </summary>
        public new void Delete()
        {
            GLThread.ExecuteGLThread(() =>
            {
                base.Delete();
                _vao?.Delete();
            });
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
                Context.Log.Warning("Tried to start mapping a buffer which is already mapping.", MessageSource.GL);
                return;
            }

            GLThread.ExecuteGLThread(() =>
            {
                GLThread.CheckError("map buffer - before start");
                Bind();
                _startPointer = (VertexData*) GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, Size, BufferAccessMask.MapWriteBit);
                _dataPointer = _startPointer;
                GLThread.CheckError("map buffer - start");
            });
        }

        /// <summary>
        /// Finish mapping the buffer, flushing changes to the GPU.
        /// </summary>
        private void Flush()
        {
            if (!Mapping)
            {
                Context.Log.Warning("Tried to finish mapping a buffer which never started mapping.", MessageSource.GL);
                return;
            }

            GLThread.ForceGLThread();

            _startPointer = null;
            _dataPointer = null;

            GLThread.CheckError("map buffer - before unmapping");
            Bind();
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GLThread.CheckError("map buffer - unmapping");
        }

        /// <summary>
        /// Resets the buffer's mapping. Does not actually clear anything but rather resets the mapping trackers and cached
        /// textures.
        /// </summary>
        public void Reset()
        {
            _textureList.Clear();
            MappedVertices = 0;

            // Reset pointer if mapping.
            if (Mapping) _dataPointer = _startPointer;
        }

        /// <summary>
        /// Moves the pointer to the specified vertex index.
        /// </summary>
        /// <param name="index">The index to move the pointer to.</param>
        public void MovePointerToVertex(int index)
        {
            _dataPointer = _startPointer + index;
        }

        /// <summary>
        /// Maps a vertex.
        /// </summary>
        /// <param name="color">The color of the vertex as a packed uint.</param>
        /// <param name="tid">The internal id of the texture for the vertex or -1 if none.</param>
        /// <param name="uv">The UV for the texture.</param>
        /// <param name="vertex">The vertex itself.</param>
        protected void UnsafeMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex)
        {
            long currentVertex = _dataPointer - _startPointer;

            // Check if going out of bounds.
            if (currentVertex > SizeInVertices)
            {
                Context.Log.Error($"Exceeding total vertices ({SizeInVertices}) in map buffer {_pointer}.", MessageSource.GL);
                return;
            }

            // Check if indices are going out of bounds.
            if (_ibo != null && currentVertex / ObjectSize > _ibo.Count / IndicesPerObject)
            {
                Context.Log.Error($"Exceeding total indices ({_ibo.Count}) in map buffer {_pointer}.", MessageSource.GL);
                return;
            }

            _dataPointer->Color = color;
            _dataPointer->Tid = tid;
            _dataPointer->UV = uv;
            _dataPointer->Vertex = vertex;

            currentVertex++;

            // Check if the mapped vertices count needs to be updated.
            if (currentVertex > MappedVertices) MappedVertices = (int) currentVertex;
        }

        #endregion

        #region Texturing

        /// <summary>
        /// Returns the texture id of the specified texture within the buffer.
        /// </summary>
        /// <param name="texture">The texture whose id to return</param>
        /// <param name="addIfMissing">Whether to add the texture to the buffer if it is not part of it already.</param>
        /// <returns>The id of the texture.</returns>
        public int GetTid(Texture texture, bool addIfMissing = true)
        {
            // If no texture.
            if (texture == null) return -1;

            int tid = -1;

            // Check if the texture is in the list of loaded textures.
            for (int i = 0; i < _textureList.Count; i++)
            {
                if (_textureList[i].Pointer != texture.Pointer) continue;
                tid = i;
                break;
            }

            // If not add it.
            if (tid != -1) return tid;

            // Check if skipping add.
            if (!addIfMissing) return -1;

            // Check if there is space for adding.
            if (_textureList.Count >= Context.Flags.RenderFlags.TextureArrayLimit)
            {
                throw new Exception($"Texture limit of buffer {_pointer} reached.");
            }

            _textureList.Add(texture);
            tid = _textureList.Count - 1;

            return tid;
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


            GLThread.CheckError("map buffer - texture binding");
        }

        #endregion

        #region Friendly Mapping API

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

            UnsafeMapVertex(color.ToUint(), GetTid(texture), Verify2dUV(texture, uv), vertex);
            _dataPointer++;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uv">The uv to verify.</param>
        /// <returns></returns>
        protected static Vector2 Verify2dUV(Texture texture, Vector2? uv)
        {
            // If no texture, the uv is empty.
            if (texture == null) return Vector2.Zero;

            return uv ?? Vector2.One;
        }

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uvRect">The uv rectangle to verify.</param>
        /// <returns></returns>
        protected static Rectangle VerifyRectUV(Texture texture, Rectangle? uvRect)
        {
            if (texture == null) return Rectangle.Empty;

            // Get the UV rectangle. If none specified then the whole texture area is chosen.
            if (uvRect == null)
                return new Rectangle(0, 0, texture.Size.X, texture.Size.Y);
            return (Rectangle) uvRect;
        }

        #endregion

        /// <summary>
        /// Set the render range for the buffer in objects.
        /// </summary>
        /// <param name="startIndex">The index of the object to start drawing from.</param>
        /// <param name="endIndex">The index of the object to stop drawing at. If -1 will draw to MappedObjects.</param>
        public void SetRenderRange(int startIndex = 0, int endIndex = -1)
        {
            if (startIndex != 0) startIndex = startIndex * ObjectSize;

            if (endIndex != -1) endIndex = endIndex * ObjectSize;

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
                Context.Log.Warning($"Map buffer startIndex {_startIndex} is beyond mapped vertices - {MappedVertices}.", MessageSource.GL);
                _startIndex = 0;
            }

            if (_endIndex > Size)
            {
                Context.Log.Warning($"Map buffer endIndex {_endIndex} is beyond size - {Size}.", MessageSource.GL);
                _endIndex = MappedVertices;
            }

            if (_startIndex > _endIndex)
            {
                Context.Log.Warning($"Map buffer startIndex {_startIndex} is beyond endIndex - {_endIndex}.", MessageSource.GL);
                _startIndex = 0;
                _endIndex = MappedVertices;
            }
        }

        /// <summary>
        /// Render the buffer.
        /// </summary>
        public virtual void Render()
        {
            GLThread.ForceGLThread();

            // Check if anything is mapped.
            if (!AnythingMapped) return;

            // Check if mapping, in which case a flush is needed.
            if (Mapping) Flush();

            // Load textures.
            BindTextures();

            _vao.Bind();
            _ibo?.Bind();
            GLThread.CheckError("map buffer - bind");

            // Convert offset amd length.
            IntPtr indexToPointer = (IntPtr) (_startIndex * sizeof(ushort));
            int length = _endIndex == -1 ? MappedObjects * IndicesPerObject : _endIndex / ObjectSize * IndicesPerObject;

            GL.DrawElements(_drawType, length, DrawElementsType.UnsignedShort, indexToPointer);
            GLThread.CheckError("map buffer - draw");

            _ibo?.Unbind();
            _vao.Unbind();
            GLThread.CheckError("map buffer - unbind");
        }

        #region Buffer API Overwrite

        /// <summary>
        /// Cannot upload to a map buffer directly
        /// </summary>
        /// <param name="size"></param>
        /// <param name="componentCount"></param>
        /// <param name="usageHint"></param>
        public new void Upload(int size, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        /// <summary>
        /// Cannot upload to a map buffer directly
        /// </summary>
        /// <param name="data"></param>
        /// <param name="componentCount"></param>
        /// <param name="usageHint"></param>
        public new void Upload(float[] data, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        /// <summary>
        /// Cannot upload to a map buffer directly
        /// </summary>
        /// <param name="data"></param>
        /// <param name="componentCount"></param>
        /// <param name="usageHint"></param>
        public new void Upload(uint[] data, uint componentCount, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        /// <summary>
        /// Cannot upload to a map buffer directly
        /// </summary>
        /// <param name="data"></param>
        /// <param name="usageHint"></param>
        public new void Upload(Vector3[] data, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        /// <summary>
        /// Cannot upload to a map buffer directly
        /// </summary>
        /// <param name="data"></param>
        /// <param name="usageHint"></param>
        public new void Upload(Vector2[] data, BufferUsageHint usageHint)
        {
            throw new InvalidOperationException("Cannot upload to a map buffer directly.");
        }

        #endregion
    }
}