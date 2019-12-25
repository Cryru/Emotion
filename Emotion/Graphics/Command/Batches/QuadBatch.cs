#region Using

using System;
using System.Diagnostics;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// Batch which handles batching sprites to be drawn together.
    /// Up to 16 textures and around 16k sprites can be batched at once.
    /// </summary>
    public abstract class QuadBatch : RecyclableCommand
    {
        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices are met, or the maximum textures.
        /// </summary>
        public bool Full { get; protected set; }

        /// <summary>
        /// The textures to bind.
        /// </summary>
        protected uint[] _textureBinding = new uint[Texture.Bound.Length];

        /// <summary>
        /// How many texture slots are utilized.
        /// </summary>
        protected int _textureSlotUtilization;

        /// <summary>
        /// Returns the data within the batch to map a sprite into, and adds the provided texture to the texture mapping.
        /// </summary>
        /// <param name="texture">The texture to bind.</param>
        /// <returns>he data inside the batch to be filled.</returns>
        public abstract Span<VertexData> GetData(Texture texture);

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

        /// <summary>
        /// Recycle the batch, clearing all batched sprites and textures.
        /// Graphics objects are reused if possible.
        /// </summary>
        public override void Recycle()
        {
            Full = false;
            _textureSlotUtilization = 0;
        }

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
    }
}