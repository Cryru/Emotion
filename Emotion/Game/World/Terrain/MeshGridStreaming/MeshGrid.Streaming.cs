#nullable enable

using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using Emotion.Standard.DataStructures;

namespace Emotion.Game.World.Terrain;

public abstract partial class MeshGrid<T, ChunkT, IndexT>
{
    #region StreamAPI

    public int StreamingMaxMeshCreationsAtOnce = 10;

    private int _meshCreationsRunning;

    private Dictionary<ChunkState, Dictionary<Vector2, ChunkT>> _chunksLoaded = new();

    private Dictionary<Vector2, ChunkT> GetChunksInState(ChunkState state)
    {
        _chunksLoaded.TryGetValue(state, out Dictionary<Vector2, ChunkT>? chunks);
        if (chunks == null)
        {
            if (state == ChunkState.DataOnly)
                chunks = _chunks;
            else
                chunks = new Dictionary<Vector2, ChunkT>();

            _chunksLoaded.Add(state, chunks);
        }
        return chunks;
    }

    public void StreamingChunkSetState(Vector2 chunkCoord, ChunkState newState)
    {
        // Not because we require GL, but because we want to make deallocations happen outside of updates.
        Assert(GLThread.IsGLThread());

        ChunkT? chunk = GetChunk(chunkCoord);
        AssertNotNull(chunk);
        if (chunk == null)
            return;

        ChunkState chunkState = chunk.State;
        Assert(chunkState != newState);
        if (chunkState == newState)
            return;

        // Chunks can only be promoted or demoted one level difference.
        int diff = Math.Abs(chunkState - newState);
        Assert(diff == 1);
        if (diff != 1)
            return;

        Assert(!chunk.LoadingStatePromotion);
        if (chunk.LoadingStatePromotion)
            return;

        bool isPromotion = chunkState < newState;
        if (isPromotion)
        {
            if (newState == ChunkState.HasMesh)
            {
                if (_meshCreationsRunning >= StreamingMaxMeshCreationsAtOnce)
                    return;
                _meshCreationsRunning++;

                chunk.StatePromotionRoutine = Engine.Jobs.Add(ChunkCreateMeshRoutineAsync(chunkCoord, chunk));
            }
            else if (newState == ChunkState.HasGPUData)
            {
                chunk.StatePromotionRoutine = Engine.Jobs.Add(ChunkCreateGPUMemoryRoutineAsync(chunkCoord, chunk));
            }
        }
        else // Demotions are instant
        {
            DemoteChunkInternal(chunkCoord, chunk);
        }
    }

    public void StreamingDemoteAllUntouchedChunks(HashSet<Vector2> touchedChunks)
    {
        // Not because we require GL, but because we want to make deallocations happen outside of updates.
        Assert(GLThread.IsGLThread());

        foreach (KeyValuePair<Vector2, ChunkT> item in _chunks)
        {
            Vector2 chunkCoord = item.Key;

            ChunkT chunk = item.Value;
            if (chunk.LoadingStatePromotion) continue;
            if (touchedChunks.Contains(chunkCoord)) continue;
            DemoteChunkInternal(chunkCoord, chunk);
        }
    }

    private void RemoveChunkFromLoadedList(Vector2 chunkCoord)
    {
        foreach (KeyValuePair<ChunkState, Dictionary<Vector2, ChunkT>> item in _chunksLoaded)
        {
            item.Value.Remove(chunkCoord);
        }
    }

    // no covariant returns for interfaces :(
    IStreamableGridChunk? IStreamableGrid.GetChunk(Vector2 chunkCoord)
    {
        return GetChunk(chunkCoord);
    }

    public IEnumerable<(Vector2, IStreamableGridChunk)> DebugOnly_StreamableGridForEachChunk()
    {
        foreach (KeyValuePair<Vector2, ChunkT> item in _chunks)
        {
            yield return (item.Key, item.Value);
        }
    }

    #endregion

    #region Promotion and Allocation Logic

    private ObjectPool<VertexDataAllocation> _meshDataAllocations = new ObjectPool<VertexDataAllocation>();
    private ObjectPoolManual<GPUVertexMemory> _gpuDataAllocations = new ObjectPoolManual<GPUVertexMemory>(() => null!);

    private IEnumerator ChunkCreateMeshRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(!chunk.VertexMemory.Allocated);
        if (chunk.VertexMemory.Allocated)
            VertexDataAllocation.FreeAllocated(ref chunk.VertexMemory);

        VertexDataAllocation newMemory = _meshDataAllocations.Get();
        if (!newMemory.Allocated)
            newMemory = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal_Color.Format, 1, $"TerrainChunk_{chunkCoord}");
        chunk.VertexMemory = newMemory;

        Assert(chunk.VerticesGeneratedForVersion == -1);
        chunk.VerticesGeneratedForVersion = -1;

        // Create mesh
        UpdateChunkVertices(chunkCoord, chunk);

        Interlocked.Decrement(ref _meshCreationsRunning);

        yield return Engine.CoroutineManager.StartCoroutine(ChunkPromoteStateSwapSynchronizeRoutine(chunkCoord, chunk, ChunkState.HasMesh));
    }

    private IEnumerator ChunkCreateGPUMemoryRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(chunk.GPUVertexMemory == null);
        if (chunk.GPUVertexMemory != null)
            GPUMemoryAllocator.FreeBuffer(chunk.GPUVertexMemory);

        Assert(chunk.VertexMemory.Allocated);
        if (!chunk.VertexMemory.Allocated)
            yield break;

        GPUVertexMemory newGpuMemory = _gpuDataAllocations.Get();
        if (newGpuMemory == null)
        {
            yield return GLThread.ExecuteOnGLThreadAsync(static (chunk) => chunk.GPUVertexMemory = GPUMemoryAllocator.AllocateBuffer(chunk.VertexMemory.Format), chunk);
        }
        else
        {
            chunk.GPUVertexMemory = newGpuMemory;
        }

        chunk.GPUDirty = true;
        yield return Engine.CoroutineManager.StartCoroutine(ChunkPromoteStateSwapSynchronizeRoutine(chunkCoord, chunk, ChunkState.HasGPUData));
    }

    private IEnumerator ChunkPromoteStateSwapSynchronizeRoutine(Vector2 chunkCoord, ChunkT chunk, ChunkState newState)
    {
        yield return null; // Eager routine prevention

        Dictionary<Vector2, ChunkT> listForNewState = GetChunksInState(newState);
        listForNewState.Add(chunkCoord, chunk);
        chunk.State = newState;
    }

    public virtual void StreamingGenerateChunk(Vector2 chunkCoord)
    {

    }

    #endregion

    #region Demotion and Deallocation Logic

    private void DemoteChunkInternal(Vector2 chunkCoord, ChunkT chunk)
    {
        ChunkState oneLevelDown = (chunk.State - 1);
        if (oneLevelDown < 0) return;

        if (oneLevelDown == ChunkState.HasMesh)
        {
            Assert(chunk.State == ChunkState.HasGPUData);
            DemoteChunkFromGPUData(chunkCoord, chunk);
        }
        else if (oneLevelDown == ChunkState.DataOnly)
        {
            Assert(chunk.State == ChunkState.HasMesh);
            DemoteChunkFromHasMesh(chunkCoord, chunk);
        }
    }

    private void DemoteChunkFromGPUData(Vector2 chunkCoord, ChunkT chunk)
    {
        GPUVertexMemory? gpuData = chunk.GPUVertexMemory;
        _gpuDataAllocations.Return(gpuData!);
        chunk.GPUVertexMemory = null;

        ChunkDemoteStateSynchronize(chunkCoord, chunk, ChunkState.HasGPUData, ChunkState.HasMesh);
    }

    private void DemoteChunkFromHasMesh(Vector2 chunkCoord, ChunkT chunk)
    {
        VertexDataAllocation memory = chunk.VertexMemory;
        _meshDataAllocations.Return(memory);
        chunk.VertexMemory = VertexDataAllocation.Empty;
        chunk.VerticesGeneratedForVersion = -1;

        ChunkDemoteStateSynchronize(chunkCoord, chunk, ChunkState.HasMesh, ChunkState.DataOnly);
    }

    private void ChunkDemoteStateSynchronize(Vector2 chunkCoord, ChunkT chunk, ChunkState fromState, ChunkState toState)
    {
        Dictionary<Vector2, ChunkT> listForOldState = GetChunksInState(fromState);
        listForOldState.Remove(chunkCoord);
        chunk.State = toState;
    }

    #endregion
}
