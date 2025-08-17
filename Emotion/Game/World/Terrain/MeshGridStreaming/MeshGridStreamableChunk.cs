using Emotion.Core.Utility.Coroutines;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.Terrain.MeshGridStreaming;

#nullable enable

public class MeshGridStreamableChunk<T, IndexT> : VersionedGridChunk<T>, IStreamableGridChunk
    where T : struct, IEquatable<T>
    where IndexT : INumber<IndexT>
{
    #region DEBUG

    [DontSerialize]
    public int DEBUG_UpdateVerticesThreadCount = 0;

    #endregion

    /// <summary>
    /// Used by the streaming logic, dont set manually.
    /// </summary>
    [DontSerialize]
    public bool Busy { get; set; }

    [DontSerialize]
    public bool AwaitingUpdate
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
