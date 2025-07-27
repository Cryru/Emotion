#nullable enable

using Emotion;
using Emotion.WIPUpdates.Rendering;

namespace Emotion.Graphics.Memory;

public static class GPUMemoryAllocator
{
    private static List<GPUVertexMemory> _freeBuffers = new ();
    private static List<IndexBuffer> _freeIBO = new ();

    private static List<GPUVertexMemory> _nextFrameBuffers = new();
    private static List<IndexBuffer> _nextFrameIBO = new ();

    public static void ProcessFreed()
    {
        _freeBuffers.AddRange(_nextFrameBuffers);
        _freeIBO.AddRange(_nextFrameIBO);

        _nextFrameIBO.Clear();
        _nextFrameBuffers.Clear();
    }

    public static GPUVertexMemory AllocateBuffer(VertexDataFormat format)
    {
        // todo: dictionary
        for (int i = 0; i < _freeBuffers.Count; i++)
        {
            GPUVertexMemory b = _freeBuffers[i];
            if (b.Format == format)
            {
                _freeBuffers.RemoveAt(i);
                return b;
            }
        }

        GPUVertexMemory newBuffer = new GPUVertexMemory(format);
        return newBuffer;
    }

    public static void FreeBuffer(GPUVertexMemory? buffer)
    {
        if (buffer == null) return;
        _nextFrameBuffers.Add(buffer);
    }

    public static IndexBuffer RentIndexBuffer(int desiredLength = -1)
    {
        if (desiredLength == -1)
        {
            if(_freeIBO.Count > 0)
            {
                IndexBuffer last = _freeIBO[^1];
                _freeIBO.RemoveAt(_freeIBO.Count - 1);
                return last;
            }
        }
        else
        {
            uint smallestThatFitsSize = uint.MaxValue;
            int smallestThatFitsIdx = -1;

            int desiredLengthInBytes = desiredLength * sizeof(ushort); // todo: int indices
            for (int i = 0; i < _freeIBO.Count; i++)
            {
                IndexBuffer ibo = _freeIBO[i];
                if (ibo.Size > desiredLengthInBytes && ibo.Size < smallestThatFitsSize)
                {
                    smallestThatFitsSize = ibo.Size;
                    smallestThatFitsIdx = i;
                }
            }

            // If the smallest that fits is still too large, don't get it.
            if (smallestThatFitsIdx != -1 && smallestThatFitsSize <= desiredLengthInBytes * 2f)
            {
                IndexBuffer fit = _freeIBO[smallestThatFitsIdx];
                _freeIBO.RemoveAt(smallestThatFitsIdx);
                return fit;
            }
        }

        var newIbo = new IndexBuffer(0, OpenGL.BufferUsage.StreamDraw);
        return newIbo;
    }

    public static void ReturnIndexBuffer(IndexBuffer ibo)
    {
        if (ibo == null) return;
        _nextFrameIBO.Add(ibo);
    }
}
