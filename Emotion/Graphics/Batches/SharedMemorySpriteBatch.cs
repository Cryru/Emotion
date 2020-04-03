#region Using

using System;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    public interface ISharedMemorySpriteBatch
    {
        void SetMemory(FencedBufferSource owner);
    }

    /// <summary>
    /// Batch which doesn't own neither GPU nor CPU memory.
    /// </summary>
    public class SharedMemorySpriteBatch<T> : SpriteBatchBase<T>, ISharedMemorySpriteBatch
    {
        public SharedMemorySpriteBatch()
        {
            _spriteByteSize = _structByteSize * 4;
        }

        protected FencedBufferSource _memoryOwner;
        protected IntPtr _memoryPtr;
        protected int _spriteByteSize;

        /// <summary>
        /// Set the vbo which owns this batch's memory.
        /// </summary>
        /// <param name="owner">The vbp who owns this batch's memory.</param>
        public void SetMemory(FencedBufferSource owner)
        {
            _memoryOwner = owner;
        }

        /// <inheritdoc />
        public override void Recycle()
        {
            base.Recycle();
            _memoryPtr = IntPtr.Zero;
        }

        /// <inheritdoc />
        public override unsafe Span<T> GetData(Texture texture, out int texturePointer)
        {
            texturePointer = -1;

            // Check if already full (or have no owner - that should never happen).
            if (Full || _memoryOwner == null) return null;

            // Check if the texture exists in the binding for this batch.
            // This will also add the texture to this batch.
            // If there is no space the fullness check above will be true - fullness checks are always in advance.
            if (texture != null) AddTextureBinding(texture.Pointer, out texturePointer);

            // Check if have memory.
            if (_memoryPtr == IntPtr.Zero)
                _memoryPtr = (IntPtr) _memoryOwner.CurrentBuffer.VBO.CreateUnsafeMapper(_memoryOwner.CurrentBufferOffset, _memoryOwner.CurrentBufferSize,
                    BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit);

            // Get the data.
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            var data = new Span<T>((void*) &((byte*) _memoryPtr)[_mappedTo * _structByteSize], 4);
            _mappedTo += 4;

            // Mark memory as used.
            var memoryLeft = (uint) (_memoryOwner.CurrentBufferSize - _mappedTo * _structByteSize);

            // Check if one more sprite can fit, both in memory and in the IBO. Each sprite is 4 vertices.
            if (memoryLeft < _spriteByteSize || _mappedTo + 4 > RenderComposer.MAX_INDICES) Full = true;

            return data;
        }

        /// <inheritdoc />
        public override unsafe Span<T> GetSpriteAt(int idx)
        {
            if (BatchedSprites < idx) return null;
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_memoryPtr == IntPtr.Zero) return null;

            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once RedundantCast
            return new Span<T>((void*) &((byte*) _memoryPtr)[idx * 4 * _structByteSize], 4);
        }

        /// <inheritdoc />
        public override void Render(RenderComposer composer)
        {
            // If nothing mapped - or no memory (which shouldn't happen), do nothing.
            if (_mappedTo == 0 || _memoryPtr == IntPtr.Zero) return;

            // Upload the data if needed.
            int mapped = _mappedTo * _structByteSize;
            _memoryOwner.CurrentBuffer.VBO.FinishMappingRange(0, (uint) mapped);
            _memoryOwner.CurrentBuffer.VBO.FinishMapping();

            // Convert the range within the current buffer to indices.
            _startIndex = (uint) (_memoryOwner.CurrentBufferOffset / _structByteSize / 4 * 6);
            _endIndex = (uint) (_startIndex + _mappedTo / 4 * 6);
            Draw(_memoryOwner.CurrentBuffer.VAO);

            _memoryOwner.SetUsed(mapped);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Recycle();
            _memoryOwner = null;
        }
    }
}