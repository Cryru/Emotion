#nullable enable

namespace Emotion.WIPUpdates.Rendering;

public static class GPUMemoryAllocator
{
    private static List<GPUVertexMemory> _freeBuffers = new List<GPUVertexMemory>();

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
        _freeBuffers.Add(buffer);
    }
}
