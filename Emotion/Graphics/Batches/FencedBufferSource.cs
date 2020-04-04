#region Using

using System;
using System.Diagnostics;
using Emotion.Common;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    public class FencedBufferSource
    {
        #region Settings

        /// <summary>
        /// The byte size of each of the backing buffers in the source.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The minimum size before swapping the backing buffer.
        /// </summary>
        public int MinimumSize { get; private set; }

        /// <summary>
        /// The number of backing buffers present.
        /// </summary>
        public int BackingBuffersCount { get; private set; }

        #endregion

        #region State

        /// <summary>
        /// The index of the current backing buffer.
        /// </summary>
        public int CurrentBufferIdx { get; private set; }

        /// <summary>
        /// A byte offset into the current backing buffer.
        /// </summary>
        public int CurrentBufferOffset { get; private set; }

        /// <summary>
        /// The current backing buffer.
        /// </summary>
        public FencedBufferObjects CurrentBuffer
        {
            get => _backingBuffers[CurrentBufferIdx];
        }

        /// <summary>
        /// The size left in the current backing buffer.
        /// </summary>
        public uint CurrentBufferSize
        {
            get => (uint) (CurrentBuffer.VBO.Size - CurrentBufferOffset);
        }

        #endregion

        private FencedBufferObjects[] _backingBuffers;
        private int[] _fences;

        public FencedBufferSource(int size, int minSize, int backingCount, Func<int, FencedBufferObjects> backingFactory)
        {
            Size = size;
            MinimumSize = minSize;
            BackingBuffersCount = backingCount;

            _backingBuffers = new FencedBufferObjects[BackingBuffersCount];
            for (var i = 0; i < BackingBuffersCount; i++)
            {
                _backingBuffers[i] = backingFactory(size);
            }

            _fences = new int[BackingBuffersCount];
        }

        /// <summary>
        /// Mark a number of bytes in the current backing buffer as used.
        /// </summary>
        /// <param name="bytes">The number of bytes to mark as used.</param>
        public void SetUsed(int bytes)
        {
            Debug.Assert(CurrentBufferOffset + bytes <= Size);

            // Add used bytes.
            CurrentBufferOffset += bytes;

            // Check if overflowing into the next buffer.
            if (CurrentBufferOffset <= Size - MinimumSize) return;
            // Create a fence for the finished buffer.
            _fences[CurrentBufferIdx] = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);

            CurrentBufferIdx++;
            if (CurrentBufferIdx == BackingBuffersCount) CurrentBufferIdx = 0;
            CurrentBufferOffset = 0;

            // Wait on the fence for the new buffer.
            int currentFence = _fences[CurrentBufferIdx];
            if (currentFence == 0) return;
            if (Engine.Renderer.CompatibilityMode)
                if (!Gl.IsSync(currentFence))
                    return;

            SyncStatus waitResult = Gl.ClientWaitSync(currentFence, SyncObjectMask.SyncFlushCommandsBit, ulong.MaxValue);
            if (Engine.Configuration.GlDebugMode)
            {
                if (waitResult != SyncStatus.AlreadySignaled) Engine.Log.Info($"Wait sync - {waitResult}", "FenceSource");
            }

            _fences[CurrentBufferIdx] = 0;
        }
    }
}