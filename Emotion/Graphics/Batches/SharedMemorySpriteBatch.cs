#region Using

using System;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Standard.Utility;

#endregion

namespace Emotion.Graphics.Batches
{
    public interface ISharedMemorySpriteBatch
    {
        void SetOwner(NativeMemoryPool owner);
    }

    /// <summary>
    /// Batch which shared GPU and CPU memory with the composer who owns it.
    /// </summary>
    public class SharedMemorySpriteBatch<T> : SpriteBatchBase<T>, ISharedMemorySpriteBatch
    {
        public SharedMemorySpriteBatch()
        {
            _spriteByteSize = _structByteSize * 4;
        }

        protected NativeMemoryPool _owner;
        protected IntPtr _memoryPage;
        protected int _spriteByteSize;

        /// <summary>
        /// Set the pool which owns this batch's memory.
        /// </summary>
        /// <param name="owner">The render composer who owns this batch.</param>
        public void SetOwner(NativeMemoryPool owner)
        {
            _owner = owner;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _memoryPage = IntPtr.Zero;
        }

        /// <inheritdoc />
        public override unsafe Span<T> GetData(Texture texture, out int texturePointer)
        {
            texturePointer = -1;

            // Check if already full (or have no owner - that should never happen).
            if (Full || _owner == null) return null;

            // Check if the texture exists in the binding for this batch.
            // This will also add the texture to this batch.
            // If there is no space the fullness check above will be true - fullness checks are always in advance.
            if (texture != null) AddTextureBinding(texture.Pointer, out texturePointer);

            // Check if have memory.
            if (_memoryPage == IntPtr.Zero)
                _memoryPage = _owner.GetMemory(_spriteByteSize);

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<T>((void*) &((byte*) _memoryPage)[_mappedTo * _structByteSize], 4);
            _mappedTo += 4;

            // Mark memory as used.
            int memoryLeft = _owner.MarkUsed(_spriteByteSize);

            // Check if one more sprite can fit, both in memory and in the IBO. Each sprite is 4 vertices.
            if (memoryLeft < _spriteByteSize || _mappedTo + 4 > RenderComposer.MAX_INDICES) Full = true;

            return data;
        }

        /// <inheritdoc />
        public override unsafe Span<T> GetSpriteAt(int idx)
        {
            if (BatchedSprites < idx) return null;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_memoryPage == IntPtr.Zero) return null;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            return new Span<T>((void*) &((byte*) _memoryPage)[idx * 4 * _structByteSize], 4);
        }

        /// <inheritdoc />
        public override void Render(RenderComposer composer)
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

        /// <inheritdoc />
        public override void Dispose()
        {
            Recycle();
            _owner = null;    
        }
    }
}