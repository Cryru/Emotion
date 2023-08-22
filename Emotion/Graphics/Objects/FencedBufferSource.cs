#region Using

using System;
using Emotion.Common;
using Emotion.Utility;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Objects;

/// <summary>
/// A tuple of a data buffer and a VAO
/// </summary>
public sealed class FencedBufferObjects
{
    public DataBuffer DataBuffer { get; private set; }
    public VertexArrayObject? VAO { get; private set; }

    public FencedBufferObjects(DataBuffer dataBuffer, VertexArrayObject? vao)
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
    private IntPtr[] _fences; // Not using GLFence objects to reduce garbage collection load.

    private bool _useMapBufferRange = false;
    public FencedBufferSource(uint size, int backingCount, Func<uint, FencedBufferObjects> backingFactory)
    {
        Size = size;
        BackingBuffersCount = backingCount;
        
        _backingBuffers = new FencedBufferObjects[BackingBuffersCount];
        for (var i = 0; i < BackingBuffersCount; i++)
        {
            _backingBuffers[i] = backingFactory(size);
        }

        _fences = new IntPtr[BackingBuffersCount];

        _useMapBufferRange = Engine.Renderer.FencedBufferUseMapBufferRange;
        if (!_useMapBufferRange) SubDataBuffer_Init();
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
        IntPtr currentFence = _fences[CurrentBufferIdx];
        if (currentFence == IntPtr.Zero) return;

        // Wait on the fence for the new buffer.
        SyncStatus waitResult = Gl.ClientWaitSync(currentFence, SyncObjectMask.SyncFlushCommandsBit, ulong.MaxValue);
        if (Engine.Configuration.GlDebugMode && waitResult != SyncStatus.AlreadySignaled)
            Engine.Log.Info($"Wait sync - {waitResult}", "FenceSource");

        Gl.DeleteSync(currentFence);
        _fences[CurrentBufferIdx] = IntPtr.Zero;
    }

    #region Map Buffer Range

    private unsafe IntPtr MapBufferRange_StartMapping()
    {
        return (IntPtr)CurrentBuffer.DataBuffer.CreateUnsafeMapper((int)CurrentBufferOffset, CurrentBufferSize,
           BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapInvalidateRangeBit
       );
    }

    private void MapBufferRange_Flush(uint sizeInBytes)
    {
        CurrentBuffer.DataBuffer.FinishMappingRange(0, sizeInBytes);
        CurrentBuffer.DataBuffer.FinishMapping();
    }

    #endregion

    #region SubData Buffer

    private static int _idx;
    private IntPtr _memory;

    private IntPtr _currentMappingPtr;
    private uint _currentMappingStart;
    private void SubDataBuffer_Init()
    {
        _memory = UnmanagedMemoryAllocator.MemAllocNamed((int)(BackingBuffersCount * Size), $"FencedBufferSource{_idx}");
        _idx += 1;
    }

    private IntPtr SubDataBuffer_StartMapping()
    {
        if (_currentMappingPtr != IntPtr.Zero)
        {
            Debug.Assert(false, "Double buffer map.");
            return IntPtr.Zero;
        }

        int fencedBufferIndex = CurrentBufferIdx;
        _currentMappingStart = CurrentBufferOffset;
        _currentMappingPtr = IntPtr.Add(_memory, (int)(fencedBufferIndex * Size + _currentMappingStart));
        return _currentMappingPtr;
    }

    private void SubDataBuffer_Flush(uint sizeInBytes)
    {
        if (_currentMappingPtr == IntPtr.Zero)
        {
            Debug.Assert(false, "Flush without buffer map.");
            return;
        }

        CurrentBuffer.DataBuffer.UploadPartial(_currentMappingPtr, sizeInBytes, _currentMappingStart);
        _currentMappingPtr = IntPtr.Zero;
        _currentMappingStart = 0;
    }

    #endregion

    public unsafe IntPtr StartMapping()
    {
        return _useMapBufferRange ? MapBufferRange_StartMapping() : SubDataBuffer_StartMapping();
    }

    public void Flush(uint sizeInBytes)
    {
        if (_useMapBufferRange)
            MapBufferRange_Flush(sizeInBytes);
        else
            SubDataBuffer_Flush(sizeInBytes);
    }
}