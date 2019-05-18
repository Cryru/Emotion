#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Logging;
using Adfectus.Platform.DesktopGL.Assets;
using Adfectus.Primitives;
using Adfectus.Utility;
using OpenGL;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    /// <inheritdoc />
    public sealed unsafe class GlStreamBuffer : StreamBuffer
    {
        /// <inheritdoc />
        public override bool Mapping
        {
            get => _startPointer != null && _dataPointer != null;
        }

        #region Drawing State

        /// <summary>
        /// The index to start drawing from.
        /// </summary>
        private uint _startIndex;

        /// <summary>
        /// The index to stop drawing at.
        /// </summary>
        private uint? _endIndex;

        /// <summary>
        /// The point where the data starts.
        /// </summary>
        private VertexData* _startPointer;

        /// <summary>
        /// The pointer currently being mapped to.
        /// </summary>
        private VertexData* _dataPointer;

        /// <summary>
        /// The list of textures the buffer TIDs (Texture IDs) require, in the correct order.
        /// </summary>
        private List<GLTexture> _textureList = new List<GLTexture>();

        /// <summary>
        /// The primitive type to draw with.
        /// </summary>
        private PrimitiveType _drawType;

        /// <summary>
        /// The vertex to start drawing from. Is essentially added to all members of the indices buffer.
        /// </summary>
        private uint _baseVertex;

        #endregion

        internal GlStreamBuffer(uint vbo, uint vao, uint ibo, uint objectSize, uint size, uint indicesPerObject, PrimitiveType drawType)
        {
            Vbo = vbo;
            Vao = vao;
            Ibo = ibo;
            Size = size;
            ObjectSize = objectSize;
            IndicesPerObject = indicesPerObject;
            _drawType = drawType;
        }

        /// <inheritdoc />
        public override int GetTid(Texture texture)
        {
            return GetTextureOrAdd((GLTexture) texture, false);
        }

        /// <inheritdoc />
        public override void UnsafeMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex)
        {
            // Check if mapping has started. If not start it.
            if (!Mapping) StartMapping();

            uint currentVertex = GetVertexPointer();

            // Check if going out of bounds.
            if (currentVertex > Size)
            {
                Engine.Log.Warning($"Exceeding total vertices ({Size}) in map buffer {Vbo}.", MessageSource.GL);
                return;
            }

            _dataPointer->Color = color;
            _dataPointer->Tid = tid;
            _dataPointer->UV = uv;
            _dataPointer->Vertex = vertex;
            _dataPointer++;

            currentVertex++;

            // Check if the mapped vertices count needs to be updated.
            if (currentVertex > MappedVertices) MappedVertices = currentVertex;
        }

        /// <inheritdoc />
        public override void UnsafeIncrementPointer(int amount)
        {
            _dataPointer += amount;
        }

        /// <inheritdoc />
        public override void UnsafeMovePointerToVertex(uint index)
        {
            _dataPointer = _startPointer + index;
        }

        /// <inheritdoc />
        public override uint GetVertexPointer()
        {
            return (uint) (_dataPointer - _startPointer);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            _textureList.Clear();
            MappedVertices = 0;

            // Reset pointer if mapping.
            if (Mapping) _dataPointer = _startPointer;

            // Reset render range.
            _startIndex = 0;
            _endIndex = null;
            _baseVertex = 0;
        }

        /// <inheritdoc />
        public override void SetRenderRange(uint startIndex = 0, uint? endIndex = null)
        {
            // Convert to vertices.
            startIndex = startIndex * IndicesPerObject;
            endIndex = endIndex * IndicesPerObject;

            // Set as vertices.
            SetRenderRangeIndices(startIndex, endIndex);
        }

        /// <inheritdoc />
        public override void SetRenderRangeIndices(uint startIndex = 0, uint? endIndex = null)
        {
            _startIndex = startIndex;
            _endIndex = endIndex;

            // Check offset.
            if (_startIndex >= MappedObjects * IndicesPerObject)
            {
                Engine.Log.Warning($"Stream buffer indices offset {_startIndex} is beyond mapped objects - {MappedObjects * IndicesPerObject}.", MessageSource.GL);
                _startIndex = 0;
            }

            if (_endIndex > Size / ObjectSize * IndicesPerObject)
            {
                Engine.Log.Warning($"Stream buffer indices end {_endIndex} is beyond mapped objects - {Size}.", MessageSource.GL);
                _endIndex = MappedObjects * IndicesPerObject;
            }

            if (_startIndex > _endIndex)
            {
                Engine.Log.Warning($"Stream buffer indices offset {_startIndex} is the indices end - {_endIndex}.", MessageSource.GL);
                _startIndex = 0;
                _endIndex = MappedObjects * IndicesPerObject;
            }
        }

        /// <inheritdoc />
        public override void SetBaseVertex(uint baseVertex)
        {
            _baseVertex = baseVertex;
        }

        /// <inheritdoc />
        public override void Render()
        {
            GLThread.ForceGLThread();

            // Check if anything is mapped.
            if (!AnythingMapped) return;

            // Check if mapping, in which case a flush is needed.
            if (Mapping) Flush();

            // Load textures.
            BindTextures();

            Engine.GraphicsManager.BindVertexArrayBuffer(Vao);
            if (!Engine.Flags.RenderFlags.UseVao) Engine.GraphicsManager.BindDataBuffer(Vbo);
            Engine.GraphicsManager.BindIndexBuffer(Ibo);
            Engine.GraphicsManager.CheckError("map buffer - bind");

            // Convert offset.
            uint startIndex = _startIndex;
            IntPtr indexToPointer = (IntPtr) (startIndex * sizeof(ushort));
            // If the ending index is null then the whole buffer is rendered.
            uint length = _endIndex - _startIndex ?? MappedObjects * IndicesPerObject - _startIndex;

            if (_baseVertex != 0)
                Gl.DrawElementsBaseVertex(_drawType, (int) length, DrawElementsType.UnsignedShort, indexToPointer, (int) _baseVertex);
            else
                Gl.DrawElements(_drawType, (int) length, DrawElementsType.UnsignedShort, indexToPointer);

            Engine.GraphicsManager.CheckError("map buffer - draw");
        }

        /// <inheritdoc />
        public override void Delete()
        {
            Engine.GraphicsManager.DestroyDataBuffer(Vbo);
            Vbo = 0;
            Engine.GraphicsManager.DestroyVertexArrayBuffer(Vao);
            Vao = 0;

            _textureList.Clear();
            Ibo = 0;
        }

        #region Friendly Mapping

        /// <inheritdoc />
        public override void MapNextVertex(Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            UnsafeMapVertex(color.ToUint(), GetTextureOrAdd((GLTexture) texture), Verify2dUV(texture, uv), vertex);
        }

        /// <inheritdoc />
        public override void MapVertexAt(uint index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Move the pointer and map the vertex.
            UnsafeMovePointerToVertex(index);
            MapNextVertex(vertex, color, texture, uv);
        }

        /// <inheritdoc />
        public override void MapNextLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1)
        {
            uint c = color.ToUint();
            Vector2 normal = Vector2.Normalize(new Vector2(pointTwo.Y - pointOne.Y, -(pointTwo.X - pointOne.X))) * thickness;
            float z = Math.Max(pointOne.Z, pointTwo.Z);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X + normal.X, pointOne.Y + normal.Y, z));
            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X + normal.X, pointTwo.Y + normal.Y, z));
            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X - normal.X, pointTwo.Y - normal.Y, z));
            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X - normal.X, pointOne.Y - normal.Y, z));
        }

        /// <inheritdoc />
        public override void MapLineAt(uint index, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1)
        {
            // Move the pointer and map.
            UnsafeMovePointerToVertex(index * ObjectSize);
            MapNextLine(pointOne, pointTwo, color, thickness);
        }

        /// <inheritdoc />
        public override void MapNextQuad(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            Rectangle uv = VerifyRectUV(texture, textureArea);
            float tid = GetTextureOrAdd((GLTexture) texture);
            uint c = color.ToUint();

            // Calculate UV positions
            Vector2 nnUV = Vector2.Zero;
            Vector2 pnUV = Vector2.Zero;
            Vector2 ppUV = Vector2.Zero;
            Vector2 npUV = Vector2.Zero;

            if (texture != null)
            {
                // Get texture matrix.
                Matrix4x4 matrix = ((GLTexture) texture).TextureMatrix;
                nnUV = Vector2.Transform(uv.Location, matrix);
                pnUV = Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y), matrix);
                ppUV = Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y + uv.Height), matrix);
                npUV = Vector2.Transform(new Vector2(uv.X, uv.Y + uv.Height), matrix);
            }

            // Add a small epsilon to prevent the wrong UVs from being sampled.
            nnUV = new Vector2(nnUV.X + MathFloat.Epsilon, nnUV.Y + MathFloat.Epsilon);
            pnUV = new Vector2(pnUV.X - MathFloat.Epsilon, pnUV.Y - MathFloat.Epsilon);
            ppUV = new Vector2(ppUV.X - MathFloat.Epsilon, ppUV.Y - MathFloat.Epsilon);
            npUV = new Vector2(npUV.X + MathFloat.Epsilon, npUV.Y + MathFloat.Epsilon);

            // Calculate vert positions.
            Vector3 pnV = new Vector3(position.X + size.X, position.Y, position.Z);
            Vector3 npV = new Vector3(position.X, position.Y + size.Y, position.Z);
            Vector3 ppV = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);

            UnsafeMapVertex(c, tid, nnUV, position);
            UnsafeMapVertex(c, tid, pnUV, pnV);
            UnsafeMapVertex(c, tid, ppUV, ppV);
            UnsafeMapVertex(c, tid, npUV, npV);
        }

        /// <inheritdoc />
        public override void MapQuadAt(uint index, Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Move the pointer and map.
            UnsafeMovePointerToVertex(index * ObjectSize);
            MapNextQuad(position, size, color, texture, textureArea);
        }

        #endregion

        #region Internal

        private void StartMapping()
        {
            if (Mapping)
            {
                Engine.Log.Warning("Tried to start mapping a buffer which is already mapping.", MessageSource.GL);
                return;
            }

            GLThread.ExecuteGLThread(() =>
            {
                Engine.GraphicsManager.CheckError("map buffer - before start");
                Engine.GraphicsManager.BindDataBuffer(Vbo);
                _startPointer = (VertexData*) Gl.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, (uint) (Size * VertexData.SizeInBytes), BufferAccessMask.MapWriteBit);
                if ((long) _startPointer == 0) Engine.Log.Warning("Couldn't start mapping buffer. Expect a crash.", MessageSource.GL);
                _dataPointer = _startPointer;
                Engine.GraphicsManager.CheckError("map buffer - start");
            });
        }

        private void Flush()
        {
            if (!Mapping)
            {
                Engine.Log.Warning("Tried to finish mapping a buffer which never started mapping.", MessageSource.GL);
                return;
            }

            GLThread.ForceGLThread();

            _startPointer = null;
            _dataPointer = null;

            Engine.GraphicsManager.CheckError("map buffer - before unmapping");
            Engine.GraphicsManager.BindDataBuffer(Vbo);
            Gl.UnmapBuffer(BufferTarget.ArrayBuffer);
            Engine.GraphicsManager.CheckError("map buffer - unmapping");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Verifies the uv.
        /// </summary>
        /// <param name="texture">The texture the uv is for.</param>
        /// <param name="uv">The uv to verify.</param>
        /// <returns></returns>
        private static Vector2 Verify2dUV(Texture texture, Vector2? uv)
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
        private static Rectangle VerifyRectUV(Texture texture, Rectangle? uvRect)
        {
            if (texture == null) return Rectangle.Empty;

            // Get the UV rectangle. If none specified then the whole texture area is chosen.
            if (uvRect == null)
                return new Rectangle(0, 0, texture.Size.X, texture.Size.Y);
            return (Rectangle) uvRect;
        }

        private int GetTextureOrAdd(GLTexture texture, bool addIfMissing = true)
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
            if (_textureList.Count >= Engine.Flags.RenderFlags.TextureArrayLimit)
                ErrorHandler.SubmitError(new Exception($"Texture limit {Engine.Flags.RenderFlags.TextureArrayLimit} of buffer {Vbo} reached."));

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
                Engine.GraphicsManager.BindTexture(_textureList[i].Pointer, (uint) i);
            }

            Engine.GraphicsManager.CheckError("map buffer - texture binding");
        }

        #endregion
    }
}