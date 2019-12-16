#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Command
{
    /// <summary>
    /// Batch which handles batching sprites to be drawn together.
    /// Up to 16 textures and around 16k sprites can be batched at once.
    /// </summary>
    public class QuadBatch : RecyclableCommand
    {
        /// <summary>
        /// Whether the batch is full.
        /// This happens when the maximum indices are met, or the maximum textures.
        /// </summary>
        public bool Full { get; protected set; }

        /// <summary>
        /// The sprite commands bound to this batch.
        /// </summary>
        public List<RenderSpriteCommand> BatchedTexturables { get; protected set; } = new List<RenderSpriteCommand>();

        /// <summary>
        /// The textures to bind.
        /// </summary>
        protected uint[] _textureBinding = new uint[Texture.Bound.Length];

        /// <summary>
        /// How many texture slots are utilized.
        /// </summary>
        protected int _textureSlotUtilization;

        protected VertexBuffer _vbo;
        protected VertexArrayObject _vao;
        protected int _mappedTo; // How far the buffer is mapped.
        protected bool _rebindTextures; // Whether to rebind textures, after mapping has occured.
        protected uint _startIndex;
        protected uint? _endIndex;

        /// <summary>
        /// Add a sprite to this batch.
        /// </summary>
        /// <param name="spriteCommand">The sprite to add.</param>
        public void PushSprite(RenderSpriteCommand spriteCommand)
        {
            if (Full) return;

            // Check if the texture exists in the binding for this batch.
            if (spriteCommand.Texture != null)
                TextureBindingUpdate(spriteCommand.Texture.Pointer);

            BatchedTexturables.Add(spriteCommand);

            // Check if one more sprite can fit. Each sprite is 4 vertices.
            // Each sprite is 4 vertices.
            if ((BatchedTexturables.Count + 1) * 4 > Engine.Renderer.MaxIndices) Full = true;
        }

        /// <summary>
        /// Get the batched sprite at the specific index.
        /// Changes to the returned object will cause undefined results.
        /// </summary>
        /// <param name="index">The index of the batched sprite.</param>
        /// <returns>The sprite at that index or null if invalid.</returns>
        public RenderSpriteCommand GetSpriteAt(int index)
        {
            if (index < 0 || index >= BatchedTexturables.Count) return null;
            return BatchedTexturables[index];
        }

        /// <summary>
        /// Set a sprite at a valid batched sprite index. It will cause a remapping and texture rebinding.
        /// If the batch cannot bind anymore textures, and the texture doesn't exist in the batch, the sprite will not be modified.
        /// </summary>
        /// <param name="index">The index of the batched sprite.</param>
        /// <param name="sprite">The sprite to set to that index.</param>
        public void SetSpriteAt(int index, RenderSpriteCommand sprite)
        {
            // Check if the index is valid.
            if (index < 0 || index >= BatchedTexturables.Count) return;

            // Check if the texture is valid.
            if(sprite.Texture != null)
            {
                // No space for the texture.
                if (!TextureBindingUpdate(sprite.Texture.Pointer)) return;
            }

            BatchedTexturables[index] = sprite;
            // Flip the "Processed" flag.
            sprite.Recycle();
            // Reset the mapped index to cause a remapping.
            _mappedTo = 0;
            // Rebind textures, as the texture could be modified.
            _rebindTextures = true;
        }

        /// <summary>
        /// Process the batch, mapping the buffer with the batched sprites.
        /// Should only be called by the composer itself.
        /// </summary>
        public override void Process(RenderComposer composer)
        {
            // Check if anything to map.
            if(BatchedTexturables.Count == 0) return;
            // Check if all are mapped.
            if(_mappedTo == BatchedTexturables.Count * 4) return;

            // Create graphics objects, if missing.
            var bufferLengthNeeded = (uint) (BatchedTexturables.Count * 4 * VertexData.SizeInBytes);
            // When resizing (or creating) create a buffer twice as big as what's needed, but don't go over the max which can be used with the default IBOs.
            // The "fullness" check in PushSprite will ensure that the needed data isn't larger than what the default IBOs can fit.
            uint resizeAmount = Math.Min(bufferLengthNeeded * 2, (uint) (Engine.Renderer.MaxIndices * VertexData.SizeInBytes));
            if (_vbo == null)
            {
                _vbo = new VertexBuffer(resizeAmount);
                _vao = new VertexArrayObject<VertexData>(_vbo, IndexBuffer.QuadIbo);
            }

            // Check if a resize is needed.
            if (_vbo.Size < bufferLengthNeeded) _vbo.Upload(IntPtr.Zero, resizeAmount);

            // Open a mapper to the vbo.
            Span<VertexData> mapper = _vbo.CreateMapper<VertexData>(0, (int) bufferLengthNeeded);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var j = 0; j < BatchedTexturables.Count; j++)
            {
                RenderSpriteCommand x = BatchedTexturables[j];

                // Check if already mapped.
                if (j * 4 < _mappedTo) continue;

                // If the sprite is processed it is also considered mapped.
                // This happens when updating a single sprite once the initial mapping has occured.
                if (x.Processed)
                {
                    _mappedTo += 4;
                    continue;
                }

                if (_rebindTextures)
                {
                    TextureBindingUpdate(x.Texture.Pointer);
                }

                x.Process(composer);

                // Set the texture id in vertices with the one the batch will bind.
                int tid = -1;
                if (x.Texture != null)
                {
                    uint tidLookingFor = x.Texture.Pointer;
                    for (var i = 0; i < _textureSlotUtilization; i++)
                    {
                        if (_textureBinding[i] == tidLookingFor) tid = i;
                    }

                    // Texture id should have been found.
                    Debug.Assert(tid != -1);
                }

                // Attach texture ids and map into the vbo.
                for (var i = 0; i < x.Vertices.Length; i++)
                {
                    x.Vertices[i].Tid = tid;
                    mapper[_mappedTo] = x.Vertices[i];
                    _mappedTo++;
                }
            }

            _vbo.FinishMapping();
        }

        public override void Execute(RenderComposer composer)
        {
            if (BatchedTexturables.Count == 0) return;

            // Bind the vao. This will also bind the vbo and ibo.
            VertexArrayObject.EnsureBound(_vao);
            BindTextures();

            // Render specified range.
            var startIndex = (IntPtr)(_startIndex * sizeof(ushort));
            uint length = _endIndex - _startIndex ?? (uint) BatchedTexturables.Count * 6 - _startIndex;
            Debug.Assert(length <= Engine.Renderer.MaxIndices * 6);

            Gl.DrawElements(PrimitiveType.Triangles, (int) length, DrawElementsType.UnsignedShort, startIndex);
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

        /// <summary>
        /// Recycle the batch, clearing all batched sprites and textures.
        /// Graphics objects are reused if possible.
        /// </summary>
        public override void Recycle()
        {
            BatchedTexturables.Clear();
            Full = false;
            _textureSlotUtilization = 0;
            _mappedTo = 0;
            _startIndex = 0;
            _endIndex = null;
        }

        /// <summary>
        /// Add the specified pointer to the texture binding, if possible.
        /// </summary>
        /// <param name="pointer">The texture pointer to add.</param>
        /// <returns>Whether the texture was added.</returns>
        protected bool TextureBindingUpdate(uint pointer)
        {
            if (_textureSlotUtilization == _textureBinding.Length) return false;

            if (TextureInBinding(pointer)) return true;

            // Add to binding.
            _textureBinding[_textureSlotUtilization] = pointer;
            _textureSlotUtilization++;

            // Verify if the texture binding maximum has been reached.
            if (_textureSlotUtilization == _textureBinding.Length) Full = true;

            return true;
        }

        /// <summary>
        /// Check whether the specified texture pointer exists in the batch binding.
        /// </summary>
        /// <param name="pointer">The texture point to check.</param>
        /// <returns>Whether this texture exists in the batch binding.</returns>
        protected bool TextureInBinding(uint pointer)
        {
            for (var i = 0; i < _textureSlotUtilization; i++)
            {
                if (_textureBinding[i] == pointer) return true;
            }

            return false;
        }

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

        public override void Dispose()
        {
            _vbo?.Dispose();
            _vao?.Dispose();
        }
    }
}