using Emotion.Common.Serialization;
using Emotion.Game.Routines;
using Emotion.Game.Terrain.GridStreaming;
using Emotion.Graphics.Memory;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.Rendering;

namespace Emotion.Game.Terrain.MeshGridStreaming;

#nullable enable

public class MeshGridStreamableChunk<T, IndexT> : VersionedGridChunk<T>, IStreamableGridChunk
    where T : struct, IEquatable<T>
    where IndexT : INumber<IndexT>
{
    public bool CanBeSimulated { get => State >= ChunkState.HasMesh; }

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
    public bool LoadingStatePromotion { get => !StatePromotionRoutine.Finished; }

    [DontSerialize]
    public Coroutine StatePromotionRoutine { get; set; } = Coroutine.CompletedRoutine;

    [DontSerialize]
    public bool GPUDirty = true;

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

    #endregion

    #region Collision

    [DontSerialize]
    public List<Cube>? Colliders; // todo: collider type

    #endregion

    public Span<VertexData_Pos_UV_Normal_Color> ResizeVertexMemoryAndGetSpan(Vector2 chunkCoord, int vertexCount)
    {
        Assert(VertexMemory.Allocated);

        if (VertexMemory.VertexCount < vertexCount)
            VertexMemory = VertexDataAllocation.Reallocate(ref VertexMemory, vertexCount);

        return VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal_Color>();
    }

    public void SetIndices(IndexT[] cpuIndices, IndexBuffer gpuIndices, int indexCount)
    {
        CPUIndexBuffer = cpuIndices;
        IndexBuffer = gpuIndices;
        IndicesUsed = indexCount;
    }
}
