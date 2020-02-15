#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// Batch which owns it's own CPU memory, and optionally GPU memory.
    /// </summary>
    public class SpriteBatch<T> : SpriteBatchBase<T>
    {
        #region Own CPU Memory

        /// <summary>
        /// Pointer to the unmanaged batch memory.
        /// </summary>
        protected IntPtr _batchedVertices;

        /// <summary>
        /// The size of the memory in vertex data.
        /// </summary>
        protected int _size;

        #endregion

        #region Own Graphics Memory

        /// <summary>
        /// Whether to create gl objects if missing.
        /// </summary>
        protected bool _createGl;

        /// <summary>
        /// Whether data needs to be uploaded.
        /// </summary>
        protected bool _upload = true;

        protected VertexBuffer _vbo;
        protected VertexArrayObject _vao;

        #endregion

        /// <summary>
        /// Create a new batch for sprites.
        /// </summary>
        /// <param name="ownGraphicsMemory">
        /// Whether to use own graphics memory.
        /// If you are caching a render it is better to set this to true as it will prevent the data from being re-uploaded.
        /// If set to false the VBO of the RenderComposer is used instead.
        /// </param>
        public SpriteBatch(bool ownGraphicsMemory = false)
        {
            _size = 400; // Initial size is 100 sprites.
            _batchedVertices = Marshal.AllocHGlobal(_size * _structByteSize);
            _createGl = ownGraphicsMemory;
        }

        /// <inheritdoc />
        public override unsafe Span<T> GetData(Texture texture, out int texturePointer)
        {
            texturePointer = -1;

            // Check if already full.
            if (Full) return null;

            // Check if the texture exists in the binding for this batch.
            // This will also add the texture to this batch.
            // If there is no space the fullness check above will be true - fullness checks are always in advance.
            if (texture != null) AddTextureBinding(texture.Pointer, out texturePointer);

            // Check if enough data.
            if (_mappedTo == _size || _mappedTo + 4 > _size)
            {
                // When resizing, go for a buffer twice as big as what's needed.
                // The "fullness" check in PushSprite will ensure that the needed data isn't larger than what the default IBOs can batch at once.
                var resizeAmount = (int) Math.Min(_size * 2, Engine.Renderer.MaxIndices);
                _batchedVertices = Marshal.ReAllocHGlobal(_batchedVertices, (IntPtr) (resizeAmount * _structByteSize));
                _size = resizeAmount;
            }

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<T>((void*) &((byte*) _batchedVertices)[_mappedTo * _structByteSize], 4);
            _mappedTo += 4;

            // Check if one more sprite can fit. Each sprite is 4 vertices.
            if (_mappedTo + 4 > Engine.Renderer.MaxIndices) Full = true;

            // New data will need to be uploaded.
            _upload = true;

            return data;
        }

        /// <summary>
        /// Get the batched sprite at the specific index.
        /// Changes to the returned object will modify the sprite itself.
        /// To change the texture request a binding from AddTextureBinding()
        /// To clear unused texture bindings call RemapTextures().
        /// </summary>
        /// <param name="index">The index of the batched sprite.</param>
        /// <returns>The sprite at that index or null if invalid.</returns>
        public override unsafe Span<T> GetSpriteAt(int index)
        {
            if (index < 0 || index * 4 >= _mappedTo) return null;

            // Mark state as dirty.
            _upload = true;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            return new Span<T>((void*) &((byte*) _batchedVertices)[index * 4 * _structByteSize], 4);
        }

        /// <inheritdoc />
        public override void Render(RenderComposer composer)
        {
            if (_mappedTo == 0) return;

            // Get the right graphics objects to use for the drawing.
            VertexBuffer vbo;
            VertexArrayObject vao;
            if (_createGl)
            {
                if (_vbo == null)
                {
                    _vbo = new VertexBuffer();
                    _vao = new VertexArrayObject<T>(_vbo, IndexBuffer.QuadIbo);
                }

                vbo = _vbo;
                vao = _vao;
            }
            else
            {
                vbo = composer.VertexBuffer;
                composer.VaoCache.TryGetValue(_structType, out vao);
                if (vao == null)
                {
                    vao = new VertexArrayObject<T>(vbo);
                    composer.VaoCache.Add(_structType, vao);
                }
            }

            // Upload the data if needed.
            if (_upload)
            {
                vbo.Upload(_batchedVertices, (uint) (_mappedTo * _structByteSize));
                _upload = false;
            }

            Draw(vao);
        }

        /// <summary>
        /// Remaps all textures in the current binding, removing unused bindings.
        /// </summary>
        public virtual void RemapTextures()
        {
            throw new NotImplementedException("ReMapTextures is not implemented in the base SpriteBatch as it's unknown where the texture is stored.");
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _upload = false;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Clear CPU memory.
            Marshal.FreeHGlobal(_batchedVertices);

            // If owning graphics memory, clear that too.
            if (_createGl)
                GLThread.ExecuteGLThreadAsync(() =>
                {
                    _vbo?.Dispose();
                    _vao?.Dispose();
                });
        }
    }
}