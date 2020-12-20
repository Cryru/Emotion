#region Using

using System;
using Emotion.Common;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// A tuple of a data buffer and a VAO
    /// </summary>
    public class FencedBufferObjects
    {
        public DataBuffer DataBuffer { get; private set; }
        public VertexArrayObject VAO { get; private set; }

        public FencedBufferObjects(DataBuffer dataBuffer, VertexArrayObject vao)
        {
            DataBuffer = dataBuffer;
            VAO = vao;
        }
    }

    /// <summary>
    /// An array of buffers with synchronization fences.
    /// </summary>
    public sealed class FencedBufferSource
    {
        #region Settings

        /// <summary>
        /// The byte size of each of the backing buffers in the source.
        /// </summary>
        public uint Size { get; private set; }

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
        public uint CurrentBufferOffset { get; private set; }

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
            get => CurrentBuffer.DataBuffer.Size - CurrentBufferOffset;
        }

        #endregion

        private FencedBufferObjects[] _backingBuffers;
        private int[] _fences;

        public FencedBufferSource(uint size, int backingCount, Func<uint, FencedBufferObjects> backingFactory)
        {
            Size = size;
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
        public void SetUsed(uint bytes)
        {
            CurrentBufferOffset += bytes;
        }

        /// <summary>
        /// Swap the current buffer to the next one in the array.
        /// </summary>
        public void SwapBuffer()
        {
            // Create a fence for the finished buffer.
            _fences[CurrentBufferIdx] = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);

            CurrentBufferIdx++;
            if (CurrentBufferIdx == BackingBuffersCount) CurrentBufferIdx = 0;
            CurrentBufferOffset = 0;

            // Check if waiting on a fence.
            int currentFence = _fences[CurrentBufferIdx];
            if (currentFence == 0) return;
            if (Engine.Renderer.CompatibilityMode)
                if (!Gl.IsSync(currentFence))
                    return;

            // Wait on the fence for the new buffer.
            SyncStatus waitResult = Gl.ClientWaitSync(currentFence, SyncObjectMask.SyncFlushCommandsBit, ulong.MaxValue);
            if (Engine.Configuration.GlDebugMode)
                if (waitResult != SyncStatus.AlreadySignaled)
                    Engine.Log.Info($"Wait sync - {waitResult}", "FenceSource");

            _fences[CurrentBufferIdx] = 0;
        }
    }
}