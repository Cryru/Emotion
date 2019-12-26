#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// Batch which handles batching sprites to be drawn together.
    /// Up to 16 textures and around 16k sprites can be batched at once.
    /// </summary>
    public abstract class SpriteBatchBase : RecyclableCommand
    {
        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices are met, or the maximum textures.
        /// </summary>
        public bool Full { get; protected set; }
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
        protected int _textureSlotUtilization;

        #endregion

        /// <summary>
        /// The number of vertices in use from _batchedVertices.
        /// </summary>
        protected int _mappedTo;

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

        /// <summary>
        /// Recycle the batch, clearing all batched sprites and textures.
        /// Graphics objects are reused if possible.
        /// </summary>
        public override void Recycle()
        {
            Full = false;
            _textureSlotUtilization = 0;
            _mappedTo = 0;
            _startIndex = 0;
            _endIndex = null;
        }

        /// <summary>
        /// Returns the data within the batch to map a sprite into, and adds the provided texture to the texture mapping.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <returns>he data inside the batch to be filled.</returns>
        public abstract Span<T> GetData(Texture texture);

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
            Debug.Assert((uint) _mappedTo <= Engine.Renderer.MaxIndices);
            Debug.Assert((uint) startIndex + length <= Engine.Renderer.MaxIndices * 6);
            Gl.DrawElements(PrimitiveType.Triangles, (int) length, DrawElementsType.UnsignedShort, startIndex);
        }

        #region Texturing API

        /// <summary>
        /// Add the specified pointer to the texture binding, if possible.
        /// </summary>
        /// <param name="pointer">The texture pointer to add.</param>
        /// <param name="bindingPointer">The pointer to the internal binding.</param>
        /// <returns>Whether the texture was added.</returns>
        public bool AddTextureBinding(uint pointer, out int bindingPointer)
        {
            bindingPointer = -1;

            if (_textureSlotUtilization == _textureBinding.Length) return false;

            if (TextureInBinding(pointer, out bindingPointer)) return true;

            // Add to binding.
            _textureBinding[_textureSlotUtilization] = pointer;
            bindingPointer = _textureSlotUtilization;
            _textureSlotUtilization++;

            // Verify if the texture binding maximum has been reached.
            if (_textureSlotUtilization == _textureBinding.Length) Full = true;

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
            for (var i = 0; i < _textureSlotUtilization; i++)
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
        public void BindTextures()
        {
            Debug.Assert(GLThread.IsGLThread());

            for (uint i = 0; i < _textureSlotUtilization; i++)
            {
                Texture.EnsureBound(_textureBinding[i], i);
            }
        }

        #endregion
    }
}