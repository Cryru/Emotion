using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.Terrain.MeshGridStreaming;

#nullable enable

public class MeshGridStreamableChunk<T, IndexT> : VersionedGridChunk<T>, IStreamableGridChunk
    where T : unmanaged, IEquatable<T>
    where IndexT : INumber<IndexT>
{
    #region DEBUG

    [DontSerialize]
    public int DEBUG_UpdateVerticesThreadCount = 0;

    #endregion

    [DontSerialize]
    public bool VisualsArentLatestVersion
    {
        get
        {
            if (State == ChunkState.HasGPUData)
                return GPUUploadedVersion != ChunkVersion;

            if (State == ChunkState.HasMesh)
                return VerticesGeneratedForVersion != ChunkVersion;

            return false;
        }
    }

    public bool CanBeSimulated
    {
        get
        {
            if (VerticesGeneratedForVersion != -1)
            {
                Assert(State >= ChunkState.HasMesh);
                return true;
            }
            return false;
        }
    }

    [DontSerialize]
    public ChunkState State { get; set; } = ChunkState.DataOnly;

    public ChunkState DebugOnly_CalculatedState
    {
        get
        {
            if (GPUVertexMemory != null)
                return ChunkState.HasGPUData;
            if (VertexMemory.Allocated)
                return ChunkState.HasMesh;
            return ChunkState.DataOnly;
        }
    }

    [DontSerialize]
    public int GPUUploadedVersion = -1;

    #region Vertices

    [DontSerialize]
    public int VerticesGeneratedForVersion = -1;

    [DontSerialize]
    public VertexDataAllocation VertexMemory;

    [DontSerialize]
    public uint VerticesUsed;

    [DontSerialize]
    public GPUVertexMemory? GPUVertexMemory;

    #endregion

    #region Bounds

    [DontSerialize]
    public Cube Bounds;

    #endregion

    #region Indices

    [DontSerialize]
    public IndexT[]? CPUIndexBuffer { get; private set; } // todo: index allocation type

    [DontSerialize]
    public IndexBuffer? IndexBuffer { get; private set; }

    [DontSerialize]
    public int IndicesUsed { get; private set; } = -1;

    [DontSerialize]
    public int TransparentIndicesUsed { get; set; }

    #endregion

    #region Collision

    [DontSerialize]
    public List<Cube>? Colliders; // todo: collider type

    #endregion

    public void SetIndices(IndexT[] cpuIndices, IndexBuffer gpuIndices, int indexCount)
    {
        CPUIndexBuffer = cpuIndices;
        IndexBuffer = gpuIndices;
        IndicesUsed = indexCount;
    }

    #region Processing

    public bool MeshUpdateRequested { get; set; }

    public bool MeshUpdatePriorityUpdateRequested { get; set; }

    // used to lock the chunk in order to manipulate its data in another thread safely
    [DontSerialize]
    private bool _busy;

    // used for debugging
    [DontSerialize]
    private ChunkDataLockReason _busyReason;

    public bool TryLockChunkData(ChunkDataLockReason reason = ChunkDataLockReason.Generic)
    {
        Assert(GLThread.IsGLThread()); // Only the main thread can lock chunks
        if (_busy) return false;
        _busy = true;
        _busyReason = reason;
        return true;
    }

    public void UnlockChunkData()
    {
        _busy = false;
    }

    public bool IsChunkDataLocked(ChunkDataLockReason? reason = null)
    {
        if (reason != null)
            return _busy && _busyReason == reason;
        return _busy;
    }

    public void TransferLockReason(ChunkDataLockReason toReason)
    {
        Assert(_busy);
        _busyReason = toReason;
    }

    #endregion
}

public enum ChunkDataLockReason
{
    Generic,
    MeshUpdate,
    Generation,
    PromotionDemotion
}