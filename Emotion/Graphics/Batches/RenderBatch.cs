#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// Batch which handles batching sprites to be drawn together.
    /// Up to 16 textures and around 16k sprites can be batched at once.
    /// </summary>
    public abstract class RenderBatch : IRenderable, IDisposable
    {
        #region Texturing

        /// <summary>
        /// The textures to bind.
        /// </summary>
        protected uint[] _textureBinding = new uint[Texture.Bound.Length];

        /// <summary>
        /// How many texture slots are utilized.
        /// </summary>
        public int TextureSlotUtilization { get; protected set; }

        #endregion

        /// <summary>
        /// The length of sprites batched. Zero indexed.
        /// </summary>
        public uint BatchedSprites
        {
            get => _mappedTo / 4;
        }

        /// <summary>
        /// Pointer to the memory currently being mapped to.
        /// </summary>
        protected IntPtr _memoryPtr;

        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices, maximum vertices, or the maximum textures are hit.
        /// </summary>
        public bool Full { get; protected set; }

        /// <summary>
        /// The number of vertices in use from _batchedVertices.
        /// </summary>
        protected uint _mappedTo;

        /// <summary>
        /// The number of indices in use. The minimum is one index per vertex.
        /// </summary>
        protected uint _indicesUsed;

        /// <summary>
        /// The total memory of the buffer.
        /// </summary>
        protected uint _bufferSize;

        /// <summary>
        /// The total index capacity of the buffer.
        /// </summary>
        protected uint _indexCapacity;

        /// <summary>
        /// The current number of indices available.
        /// </summary>
        protected uint _indices;

        /// <summary>
        /// The index buffer to use when rendering. If the VAO provides an IBO it will be used instead.
        /// </summary>
        protected IndexBuffer _ibo;

        /// <summary>
        /// The current GL primitive rendering mode.
        /// </summary>
        protected PrimitiveType _primitiveRenderingMode;

        /// <summary>
        /// Reset the batch, clearing all batched sprites and textures.
        /// Memory is reused if possible.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Change the type of batching currently in use.
        /// This affects the primitive mode, IBO capacity and other internal settings.
        /// </summary>
        /// <param name="mode"></param>
        public virtual void SetBatchMode(BatchMode mode)
        {
            if (mode == BatchMode.Quad)
            {
                _indexCapacity = RenderComposer.MAX_INDICES / 4 * 6;
                _indices = _indexCapacity;
                _ibo = IndexBuffer.QuadIbo;
                _primitiveRenderingMode = PrimitiveType.Triangles;
            }
            else
            {
                _indexCapacity = RenderComposer.MAX_INDICES;
                _indices = _indexCapacity;
                _ibo = IndexBuffer.SequentialIbo;
                _primitiveRenderingMode = PrimitiveType.TriangleStrip;
            }
        }

        /// <summary>
        /// Render the batch.
        /// </summary>
        /// <param name="c">The composer initiating the render.</param>
        /// <param name="startIndex">The index to draw from. By default this is the whole batch.</param>
        /// <param name="endIndex">The index to draw to. By default this is up to the end of the batch.</param>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public abstract void Render(RenderComposer c, uint startIndex = 0, int endIndex = -1);

        /// <summary>
        /// Render the batch.
        /// </summary>
        public void Render(RenderComposer c)
        {
            Render(c, 0);
        }

        #region Texture Binding API

        /// <summary>
        /// Add the specified texture to the texture binding, if possible.
        /// </summary>
        /// <param name="texture">The texture to add.</param>
        /// <returns>The texture pointer to map in the vertex data. -1 if invalid.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddTextureBinding(Texture texture)
        {
            // Invalid texture object.
            if (texture == null) return -1;
            AddTextureBinding(texture.Pointer, out int texturePointer);
            return texturePointer;
        }

        /// <summary>
        /// Add the specified pointer to the texture binding, if possible.
        /// </summary>
        /// <param name="pointer">The texture pointer to add.</param>
        /// <param name="bindingPointer">The pointer to the internal binding.</param>
        /// <returns>Whether the texture was added.</returns>
        public virtual bool AddTextureBinding(uint pointer, out int bindingPointer)
        {
            bindingPointer = -1;

            if (Full) return false;
            if (TextureSlotUtilization == _textureBinding.Length) return false;
            if (TextureInBinding(pointer, out bindingPointer)) return true;

            // Add to binding.
            _textureBinding[TextureSlotUtilization] = pointer;
            bindingPointer = TextureSlotUtilization;
            TextureSlotUtilization++;

            // Verify if the texture binding maximum has been reached.
            if (TextureSlotUtilization == _textureBinding.Length) Full = true;

            return true;
        }

        /// <summary>
        /// Check whether the specified texture pointer exists in the batch binding.
        /// </summary>
        /// <param name="pointer">The texture point to check.</param>
        /// <param name="bindingPointer">The internal pointer bound at or -1 if not bound.</param>
        /// <returns>Whether this texture exists in the batch binding.</returns>
        public bool TextureInBinding(uint pointer, out int bindingPointer)
        {
            for (var i = 0; i < TextureSlotUtilization; i++)
            {
                if (_textureBinding[i] != pointer) continue;
                bindingPointer = i;
                return true;
            }

            bindingPointer = -1;
            return false;
        }

        /// <summary>
        /// Bind the textures in use.
        /// </summary>
        public virtual void BindTextures()
        {
            Debug.Assert(GLThread.IsGLThread());

            for (uint i = 0; i < TextureSlotUtilization; i++)
            {
                Texture.EnsureBound(_textureBinding[i], i);
            }
        }

        #endregion

        /// <summary>
        /// Free resources.
        /// </summary>
        public abstract void Dispose();
    }

    public enum BatchMode
    {
        Quad,
        TriangleStrip
    }

    /// <summary>
    /// Generic batch.
    /// </summary>
    public abstract class RenderBatch<T> : RenderBatch
    {
        #region Vertex Type

        protected int _structByteSize;
        protected int _spriteByteSize; // Size of a sprite (4 vertices) in this format. This is the minimum size the buffer can have.
        protected Type _structType;

        #endregion

        /// <summary>
        /// Create a new render batch.
        /// </summary>
        /// <param name="size">The size of the batch. If set to 0 then the default max indices size is used.</param>
        protected RenderBatch(uint size = 0)
        {
            _structType = typeof(T);
            _structByteSize = Marshal.SizeOf<T>();
            _spriteByteSize = _structByteSize * 4;
            _indicesUsed = 0;
            if (size == 0)
                _bufferSize = (uint) (RenderComposer.MAX_INDICES * _structByteSize);
            else
                _bufferSize = size;
            SetBatchMode(BatchMode.Quad);
        }

        /// <inheritdoc />
        public override void Reset()
        {
            _memoryPtr = IntPtr.Zero;
            Full = false;
            TextureSlotUtilization = 0;
            _mappedTo = 0;
            _indicesUsed = 0;
        }

        /// <summary>
        /// Get data in the batch to map to. If the requested data is too much then an empty span will be returned.
        /// The buffer's capacity is not guaranteed in any way, other than if .Full is false it should
        /// be able to serve at least one quad (4 structs)
        /// </summary>
        /// <param name="structCount">The number of structs to return.</param>
        /// <param name="indicesToUse">
        /// The number of indices your vertices will use. This also determines the buffer capacity. At
        /// least one index per struct is required.
        /// </param>
        /// <returns></returns>
        public virtual unsafe Span<T> GetData(uint structCount, uint indicesToUse)
        {
            // Check if marked as full. This happens when either the texture count reaches the limit,
            // or there's not enough memory left to serve a quad (the minimum)
            if (Full) return null;

            // Minimum of one index per struct.
            indicesToUse = Math.Max(indicesToUse, structCount);

            // Get memory if we don't have any.
            if (_memoryPtr == IntPtr.Zero)
                _memoryPtr = GetMemoryPointer();

            // Check if there is enough memory to serve the request.
            var memoryUsed = (uint) (_mappedTo * _structByteSize);
            if (memoryUsed + structCount * _structByteSize > _bufferSize) return null;

            // Check if enough indices to serve the request.
            if (_indicesUsed + indicesToUse > _indices) return null;

            // Get the data, and wrap it in a presentable span.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<T>((void*) &((byte*) _memoryPtr)[_mappedTo * _structByteSize], (int) structCount);
            _mappedTo += structCount;
            _indicesUsed += indicesToUse;

            // Check if one more quad can fit, both in memory and in the IBO. Each sprite is 4 vertices and 6 indices.
            long memoryLeft = _bufferSize - _mappedTo * _structByteSize;
            if (memoryLeft < _spriteByteSize || _indicesUsed + RenderComposer.MINIMUM_INDICES > _indices) Full = true;

            return data;
        }

        #region Internal API

        /// <summary>
        /// Get a pointer to the mapping memory.
        /// </summary>
        /// <returns>A void pointer to the mapping memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract IntPtr GetMemoryPointer();

        /// <summary>
        /// Draw the batch. The data is expected to have been uploaded to the vbo at this point.
        /// </summary>
        /// <param name="vao">The vao to use.</param>
        /// <param name="startIndex">The index to draw from. By default this is the whole batch.</param>
        /// <param name="length">The length to draw to. By default this is up to the end of the batch.</param>
        protected virtual void Render(VertexArrayObject vao, uint startIndex = 0, int length = -1)
        {
            // Bind graphics objects.
            VertexArrayObject.EnsureBound(vao);
            if (vao.IBO == null) IndexBuffer.EnsureBound(_ibo.Pointer);
            BindTextures();

            // Render specified range.
            var startIndexInt = (IntPtr) (startIndex * sizeof(ushort));
            if (length == -1) length = (int) (_indicesUsed - startIndex);
            Debug.Assert(_mappedTo <= RenderComposer.MAX_INDICES);
            Debug.Assert(startIndex + length <= _indexCapacity);
            Gl.DrawElements(_primitiveRenderingMode, length, DrawElementsType.UnsignedShort, startIndexInt);
        }

        #endregion
    }
}