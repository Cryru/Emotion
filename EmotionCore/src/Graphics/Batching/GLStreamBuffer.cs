// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <inheritdoc />
    public sealed unsafe class GLStreamBuffer : StreamBuffer
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
        private uint? _endIndex = null;

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
        private List<Texture> _textureList;

        /// <summary>
        /// The primitive type to draw with.
        /// </summary>
        private PrimitiveType _drawType;

        #endregion

        internal GLStreamBuffer(uint vbo, uint vao, uint ibo, uint objectSize, uint size, uint indicesPerObject, PrimitiveType drawType)
        {
            Vbo = vbo;
            Vao = vao;
            Ibo = ibo;
            Size = size;
            ObjectSize = objectSize;
            IndicesPerObject = indicesPerObject;
            _drawType = drawType;

            _textureList = new List<Texture>();
        }

        /// <inheritdoc />
        public override int GetTid(Texture texture)
        {
            return GetTextureOrAdd(texture, false);
        }

        /// <inheritdoc />
        public override void UnsafeMapVertex(uint color, float tid, Vector2 uv, Vector3 vertex)
        {
            uint currentVertex = GetVertexPointer();

            // Check if going out of bounds.
            if (currentVertex > Size)
            {
                Context.Log.Warning($"Exceeding total vertices ({Size}) in map buffer {Vbo}.", MessageSource.GL);
                return;
            }

            _dataPointer->Color = color;
            _dataPointer->Tid = tid;
            _dataPointer->UV = uv;
            _dataPointer->Vertex = vertex;

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
            return (uint)(_dataPointer - _startPointer);
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
        }

        /// <inheritdoc />
        public override void SetRenderRange(uint startIndex = 0, uint? endIndex = null)
        {
            // Convert to vertices.
            startIndex = startIndex * ObjectSize;
            endIndex = endIndex * ObjectSize;

            // Set as vertices.
            SetRenderRangeVertices(startIndex, endIndex);
        }

        /// <inheritdoc />
        public override void SetRenderRangeVertices(uint startIndex = 0, uint? endIndex = null)
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

            GraphicsManager.BindVertexArrayBuffer(Vao);
            GraphicsManager.BindIndexBuffer(Ibo);
            GLThread.CheckError("map buffer - bind");

            // Convert offset amd length.
            IntPtr indexToPointer = (IntPtr)(_startIndex * sizeof(ushort));
            // If the ending index is null then the whole buffer is rendered.
            uint length = _endIndex / ObjectSize * IndicesPerObject ?? MappedObjects * IndicesPerObject;

            GL.DrawElements(_drawType, (int)length, DrawElementsType.UnsignedShort, indexToPointer);
            GLThread.CheckError("map buffer - draw");

            GraphicsManager.BindVertexArrayBuffer(0);
            GraphicsManager.BindIndexBuffer(0);
            GLThread.CheckError("map buffer - unbind");
        }

        /// <inheritdoc />
        public override void Delete()
        {
            GraphicsManager.DestroyDataBuffer(Vbo);
            Vbo = 0;
            GraphicsManager.DestroyVertexArrayBuffer(Vao);
            Vao = 0;
        }

        #region Friendly Mapping

        /// <inheritdoc />
        public override void MapNextVertex(Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            UnsafeMapVertex(color.ToUint(), GetTextureOrAdd(texture), Verify2dUV(texture, uv), vertex);
            UnsafeIncrementPointer(1);
        }

        /// <inheritdoc />
        public override void MapVertexAt(uint index, Vector3 vertex, Color color, Texture texture = null, Vector2? uv = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map the vertex.
            UnsafeMovePointerToVertex(index);
            MapNextVertex(vertex, color, texture, uv);
        }

        /// <inheritdoc />
        public override void MapNextLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            uint c = color.ToUint();
            Vector2 normal = Vector2.Normalize(new Vector2(pointTwo.Y - pointOne.Y, -(pointTwo.X - pointOne.X))) * thickness;
            float z = Math.Max(pointOne.Z, pointTwo.Z);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X + normal.X, pointOne.Y + normal.Y, z));
            UnsafeIncrementPointer(1);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X + normal.X, pointTwo.Y + normal.Y, z));
            UnsafeIncrementPointer(1);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointTwo.X - normal.X, pointTwo.Y - normal.Y, z));
            UnsafeIncrementPointer(1);

            UnsafeMapVertex(c, -1, Vector2.Zero, new Vector3(pointOne.X - normal.X, pointOne.Y - normal.Y, z));
            UnsafeIncrementPointer(1);
        }

        /// <inheritdoc />
        public override void MapLineAt(uint index, Vector3 pointOne, Vector3 pointTwo, Color color, int thickness = 1)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            // Move the pointer and map.
            UnsafeMovePointerToVertex(index * ObjectSize);
            MapNextLine(pointOne, pointTwo, color, thickness);
        }

        /// <inheritdoc />
        public override void MapNextQuad(Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

            Rectangle uv = VerifyRectUV(texture, textureArea);
            float tid = GetTextureOrAdd(texture);
            uint c = color.ToUint();

            // Calculate UV positions
            Vector2 nnUV = texture == null ? Vector2.Zero : Vector2.Transform(uv.Location, texture.TextureMatrix);
            Vector2 pnUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y), texture.TextureMatrix);
            Vector2 ppUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X + uv.Width, uv.Y + uv.Height), texture.TextureMatrix);
            Vector2 npUV = texture == null ? Vector2.Zero : Vector2.Transform(new Vector2(uv.X, uv.Y + uv.Height), texture.TextureMatrix);

            // Calculate vert positions.
            Vector3 pnV = new Vector3(position.X + size.X, position.Y, position.Z);
            Vector3 npV = new Vector3(position.X, position.Y + size.Y, position.Z);
            Vector3 ppV = new Vector3(position.X + size.X, position.Y + size.Y, position.Z);

            UnsafeMapVertex(c, tid, nnUV, position);
            _dataPointer++;

            UnsafeMapVertex(c, tid, pnUV, pnV);
            _dataPointer++;

            UnsafeMapVertex(c, tid, ppUV, ppV);
            _dataPointer++;

            UnsafeMapVertex(c, tid, npUV, npV);
            _dataPointer++;
        }

        /// <inheritdoc />
        public override void MapQuadAt(uint index, Vector3 position, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            // Check if mapping has started.
            if (!Mapping) StartMapping();

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
                Context.Log.Warning("Tried to start mapping a buffer which is already mapping.", MessageSource.GL);
                return;
            }

            GLThread.ExecuteGLThread(() =>
            {
                GLThread.CheckError("map buffer - before start");
                GraphicsManager.BindDataBuffer(Vbo);
                _startPointer = (VertexData*)GL.MapBufferRange(BufferTarget.ArrayBuffer, IntPtr.Zero, (int)(Size * VertexData.SizeInBytes), BufferAccessMask.MapWriteBit);
                if ((long)_startPointer == 0)
                {
                    Context.Log.Warning("Couldn't start mapping buffer. Expect a crash.", MessageSource.GL);
                }
                _dataPointer = _startPointer;
                GLThread.CheckError("map buffer - start");
            });
        }

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
            GraphicsManager.BindDataBuffer(Vbo);
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GLThread.CheckError("map buffer - unmapping");
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
            return (Rectangle)uvRect;
        }

        private int GetTextureOrAdd(Texture texture, bool addIfMissing = true)
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
            if (_textureList.Count >= Context.Flags.RenderFlags.TextureArrayLimit) throw new Exception($"Texture limit of buffer {Vbo} reached.");

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
    }
}