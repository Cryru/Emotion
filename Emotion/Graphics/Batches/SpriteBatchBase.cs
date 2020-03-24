#region Using

using System;
using System.Diagnostics;
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
    public abstract class SpriteBatchBase : IRecyclable, IRenderable, IDisposable
    {
        /// <summary>
        /// The length of sprites batched. Zero indexed.
        /// </summary>
        public int BatchedSprites
        {
            get => _mappedTo / 4;
        }

        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices are met, or the maximum textures.
        /// </summary>
        public bool Full { get; protected set; }

        /// <summary>
        /// The number of vertices in use from _batchedVertices.
        /// </summary>
        protected int _mappedTo;

        /// <summary>
        /// Recycle the batch, clearing all batched sprites and textures.
        /// Graphics objects are reused if possible.
        /// </summary>
        public abstract void Recycle();

        /// <summary>
        /// Render the batch.
        /// </summary>
        /// <param name="c">The composer initiating the render.</param>
        public abstract void Render(RenderComposer c);

        /// <summary>
        /// Free resources.
        /// </summary>
        public abstract void Dispose();
    }

    /// <summary>
    /// Generic batch.
    /// </summary>
    public abstract class SpriteBatchBase<T> : SpriteBatchBase
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
        /// The index to start rendering from.
        /// </summary>
        protected uint _startIndex;

        /// <summary>
        /// The index to end rendering at.
        /// </summary>
        protected uint? _endIndex;

        #region Vertex Type

        protected int _structByteSize;
        protected Type _structType;

        #endregion

        protected SpriteBatchBase()
        {
            _structType = typeof(T);
            _structByteSize = Marshal.SizeOf<T>();
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            Full = false;
            TextureSlotUtilization = 0;
            _mappedTo = 0;
            _startIndex = 0;
            _endIndex = null;
        }

        /// <summary>
        /// Returns the data within the batch to map a sprite into, and adds the provided texture to the texture mapping.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <param name="texturePointer">The texture point within the batch's texture binding.</param>
        /// <returns>he data inside the batch to be filled.</returns>
        public abstract Span<T> GetData(Texture texture, out int texturePointer);

        /// <summary>
        /// Returns the data within the batch to map a sprite into, and adds the provided texture to the texture mapping.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <returns>The data inside the batch to be filled.</returns>
        public virtual Span<T> GetData(Texture texture)
        {
            return GetData(texture, out int _);
        }

        /// <summary>
        /// Returns the data associated with the specified batched sprite.
        /// Invalid indices will return null. Modifying the texture pointer will not cause
        /// the internal texture mapping to change. Take care.
        /// </summary>
        /// <param name="idx">The index of the sprite to return.</param>
        /// <returns>The vertices of the sprite under the specified index.</returns>
        public abstract Span<T> GetSpriteAt(int idx);


        /// <summary>
        /// Set the render range for the batch. By default the whole batch is rendered.
        /// </summary>
        /// <param name="start">The sprite index to start rendering from. Inclusive.</param>
        /// <param name="end">The sprite index to stop rendering at. If null will draw to the end. Exclusive.</param>
        public void SetRenderRange(uint start, uint? end = null)
        {
            // Convert to indices.
            _startIndex = start * 6;
            _endIndex = end * 6;
        }

        /// <summary>
        /// Draw the batch. The data is expected to have been uploaded to the vbo at this point.
        /// </summary>
        /// <param name="vao">The vao to use.</param>
        protected void Draw(VertexArrayObject vao)
        {
            // Bind graphics objects.
            VertexArrayObject.EnsureBound(vao);
            if (vao.IBO == null) IndexBuffer.EnsureBound(IndexBuffer.QuadIbo.Pointer);
            BindTextures();

            // Render specified range.
            var startIndex = (IntPtr) (_startIndex * sizeof(ushort));
            uint length = _endIndex - _startIndex ?? (uint) (_mappedTo / 4) * 6 - _startIndex;
            Debug.Assert((uint) _mappedTo <= RenderComposer.MAX_INDICES);
            Debug.Assert((uint) startIndex + length <= RenderComposer.MAX_INDICES * 6);
            Gl.DrawElements(PrimitiveType.Triangles, (int) length, DrawElementsType.UnsignedShort, startIndex);
        }

        #region Texturing API

        /// <summary>
        /// Add the specified pointer to the texture binding, if possible.
        /// </summary>
        /// <param name="pointer">The texture pointer to add.</param>
        /// <param name="bindingPointer">The pointer to the internal binding.</param>
        /// <returns>Whether the texture was added.</returns>
        public virtual bool AddTextureBinding(uint pointer, out int bindingPointer)
        {
            bindingPointer = -1;

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
    }
}