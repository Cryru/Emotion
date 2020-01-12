#region Using

using System;
using Emotion.Common;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Command.Batches
{
    /// <summary>
    /// Batch which shared GPU and CPU memory with the composer who owns it.
    /// </summary>
    public class SharedMemorySpriteBatch : SpriteBatchBase<VertexData>
    {
        public SharedMemorySpriteBatch()
        {
            _spriteByteSize = _structByteSize * 4;
        }

        protected RenderComposer _owner;
        protected IntPtr _memoryPage;
        protected int _spriteByteSize;

        /// <summary>
        /// Set the owning render composer.
        /// </summary>
        /// <param name="owner">The render composer who owns this batch.</param>
        public void SetOwner(RenderComposer owner)
        {
            _owner = owner;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _owner = null;
            _memoryPage = IntPtr.Zero;
        }

        /// <inheritdoc />
        public override unsafe Span<VertexData> GetData(Texture texture)
        {
            // Check if already full (or have no owner - that should never happen).
            if (Full || _owner == null) return null;

            // Check if the texture exists in the binding for this batch.
            // This will also add the texture to this batch.
            // If there is no space the fullness check above will be true - fullness checks are always in advance.
            int texturePointer = -1;
            if (texture != null) AddTextureBinding(texture.Pointer, out texturePointer);

            // Check if have memory.
            if (_memoryPage == IntPtr.Zero)
                _memoryPage = _owner.MemoryPool.GetMemory(_spriteByteSize);

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<VertexData>((void*) &((byte*) _memoryPage)[_mappedTo * _structByteSize], 4);
            _mappedTo += 4;

            // Set the Tid.
            for (var i = 0; i < data.Length; i++)
            {
                data[i].Tid = texturePointer;
            }

            // Mark memory as used.
            int memoryLeft = _owner.MemoryPool.MarkUsed(_spriteByteSize);

            // Check if one more sprite can fit, both in memory and in the IBO. Each sprite is 4 vertices.
            if (memoryLeft < _spriteByteSize || _mappedTo + 4 > Engine.Renderer.MaxIndices) Full = true;

            return data;
        }

        /// <inheritdoc />
        public override unsafe Span<VertexData> GetSpriteAt(int idx)
        {
            if (BatchedSprites < idx) return null;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_memoryPage == IntPtr.Zero) return null;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            return new Span<VertexData>((void*) &((byte*) _memoryPage)[idx * 4 * _structByteSize], 4);
        }

        /// <inheritdoc />
        public override void Execute(RenderComposer composer)
        {
            // If nothing mapped - or no memory (which shouldn't happen), do nothing.
            if (_mappedTo == 0 || _memoryPage == IntPtr.Zero) return;

            // Get the right graphics objects to use for the drawing.
            VertexBuffer vbo = composer.VertexBuffer;
            VertexArrayObject vao = composer.CommonVao;

            // Upload the data if needed.
            vbo.Upload(_memoryPage, (uint) (_mappedTo * _structByteSize));

            // Draw.
            Draw(vao);
        }
    }
}