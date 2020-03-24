#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Common;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

public class RingGraphicsObjects
{
    public VertexBuffer VBO { get; private set; }
    public VertexArrayObject VAO { get; private set; }

    public RingGraphicsObjects(VertexBuffer vbo, VertexArrayObject vao)
    {
        VBO = vbo;
        VAO = vao;
    }
}

public class RingVertexBuffer
{
    #region Settings

    public int Size { get; private set; }

    public int MinimumSize { get; private set; }

    public int BackingBuffersCount { get; private set; }

    #endregion

    #region State

    public int CurrentBufferIdx { get; private set; }

    public int CurrentBufferOffset { get; private set; }

    public RingGraphicsObjects CurrentBuffer
    {
        get => _backingBuffers[CurrentBufferIdx];
    }

    public uint CurrentBufferSize
    {
        get => (uint) (CurrentBuffer.VBO.Size - CurrentBufferOffset);
    }

    #endregion

    private RingGraphicsObjects[] _backingBuffers;
    private int[] _fences;

    public RingVertexBuffer(int size, int minSize, int backingCount, Func<int, RingGraphicsObjects> backingFactory)
    {
        Size = size;
        MinimumSize = minSize;
        BackingBuffersCount = backingCount;

        _backingBuffers = new RingGraphicsObjects[BackingBuffersCount];
        for (var i = 0; i < BackingBuffersCount; i++)
        {
            _backingBuffers[i] = backingFactory(size);
        }

        _fences = new int[BackingBuffersCount];
    }

    public void SetUsed(int bytes)
    {
        Debug.Assert(CurrentBufferOffset + bytes <= Size);

        // Add used bytes.
        CurrentBufferOffset += bytes;

        // Check if overflowing into the next buffer.
        if (CurrentBufferOffset > Size - MinimumSize)
        {
            // Create a fence for the finished buffer.
            _fences[CurrentBufferIdx] = Gl.FenceSync(SyncCondition.SyncGpuCommandsComplete, 0);

            CurrentBufferIdx++;
            if (CurrentBufferIdx == BackingBuffersCount) CurrentBufferIdx = 0;
            CurrentBufferOffset = 0;

            // Wait on the fence for the new buffer.
            int currentFence = _fences[CurrentBufferIdx];
            if (currentFence != 0)
            {
                if (Engine.Renderer.CompatibilityMode)
                {
                    if(!Gl.IsSync(currentFence)) return;
                }

                SyncStatus waitResult = Gl.ClientWaitSync(currentFence, SyncObjectMask.SyncFlushCommandsBit, ulong.MaxValue);
                if (waitResult != SyncStatus.AlreadySignaled) Engine.Log.Info($"Wait sync - {waitResult}", "Ring");

                _fences[CurrentBufferIdx] = 0;
            }
        }
    }
}