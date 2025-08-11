#nullable enable

using Emotion.Core.Systems.Input;
using Emotion.Core.Utility.Threading;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;

namespace Emotion.Game.World.Terrain;

public abstract partial class MeshGrid<T, ChunkT, IndexT>
{
    #region Streaming API

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

    public void ResolveChunkStateRequests(List<ChunkStreamRequest> requestState, HashSet<Vector2> touchedChunks)
    {
        // Not because we require GL, but because we want to make deallocations happen outside of updates.
        Assert(GLThread.IsGLThread());

        lock (_chunkUpdateLock)
        {
            // Process new requests
            foreach (ChunkStreamRequest request in requestState)
            {
                Vector2 coord = request.ChunkCoord;
                ChunkState newState = request.State;
                StreamingChunkSetState(coord, newState);
            }

            // Demote all untouched chunks
            foreach (KeyValuePair<Vector2, ChunkT> item in _chunks)
            {
                Vector2 chunkCoord = item.Key;

                ChunkT chunk = item.Value;
                if (touchedChunks.Contains(chunkCoord)) continue;

                ChunkState oneLevelDown = chunk.State - 1;
                if (oneLevelDown < 0) continue;
                StreamingChunkSetState(chunkCoord, oneLevelDown);
            }
        }
    }

    private void StreamingChunkSetState(Vector2 chunkCoord, ChunkState newState)
    {
        ChunkT? chunk = GetChunk(chunkCoord);
        if (chunk == null && newState == ChunkState.DataOnly)
        {
            StreamingGenerateChunk(chunkCoord);
            return;
        }
        AssertNotNull(chunk);

        ChunkState chunkState = chunk.State;
        Assert(chunkState != newState);
        if (chunkState == newState)
            return;

        // Chunks can only be promoted or demoted one level difference.
        int diff = Math.Abs(chunkState - newState);
        Assert(diff == 1);
        if (diff != 1)
            return;

        if (chunk.Busy) return;
        chunk.ChangeToState = newState;
        RequestChunkMeshUpdate(chunkCoord, chunk);
    }

    private IEnumerator CheckStreamingStateChange(Vector2 chunkCoord, ChunkT chunk)
    {
        ChunkState chunkState = chunk.State;
        ChunkState newState = chunk.ChangeToState;
        if (chunkState == newState) yield break;

        bool isPromotion = chunkState < newState;
        if (isPromotion)
        {
            if (newState == ChunkState.HasMesh)
            {
                yield return Engine.CoroutineManager.StartCoroutine(PromoteChunkToHasMesh(chunkCoord, chunk));
            }
            else if (newState == ChunkState.HasGPUData)
            {
                yield return Engine.CoroutineManager.StartCoroutine(PromoteChunkToHasGPU(chunkCoord, chunk));
            }
        }
        else
        {
            if (newState == ChunkState.HasMesh)
            {
                yield return Engine.CoroutineManager.StartCoroutine(DemoteChunkFromGPUData(chunkCoord, chunk));
            }
            else if (newState == ChunkState.DataOnly)
            {
                yield return Engine.CoroutineManager.StartCoroutine(DemoteChunkFromHasMesh(chunkCoord, chunk));
            }
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

    private IEnumerator PromoteChunkToHasMesh(Vector2 chunkCoord, ChunkT chunk)
    {
        yield return null; // Eager routine prevention

        Assert(!chunk.VertexMemory.Allocated);
        if (chunk.VertexMemory.Allocated)
            VertexDataAllocation.FreeAllocated(ref chunk.VertexMemory);

        VertexDataAllocation newMemory = _meshDataAllocations.Get();
        if (!newMemory.Allocated)
            newMemory = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal_Color.Format, 1, $"TerrainChunk_{chunkCoord}");
        chunk.VertexMemory = newMemory;

        Assert(chunk.VerticesGeneratedForVersion == -1);
        chunk.VerticesGeneratedForVersion = -1;

        Dictionary<Vector2, ChunkT> listForNewState = GetChunksInState(ChunkState.HasMesh);
        listForNewState.Add(chunkCoord, chunk);
        chunk.State = ChunkState.HasMesh;

        RequestChunkMeshUpdate(chunkCoord, chunk);
    }

    private IEnumerator PromoteChunkToHasGPU(Vector2 chunkCoord, ChunkT chunk)
    {
        yield return null; // Eager routine prevention

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

        Assert(chunk.GPUUploadedVersion == -1);
        chunk.GPUUploadedVersion = -1;

        Dictionary<Vector2, ChunkT> listForNewState = GetChunksInState(ChunkState.HasGPUData);
        listForNewState.Add(chunkCoord, chunk);
        chunk.State = ChunkState.HasGPUData;

        RequestChunkMeshUpdate(chunkCoord, chunk);
    }

    public virtual void StreamingGenerateChunk(Vector2 chunkCoord)
    {

    }

    #endregion

    #region Demotion and Deallocation Logic

    private IEnumerator DemoteChunkFromGPUData(Vector2 chunkCoord, ChunkT chunk)
    {
        yield return null; // Eager routine prevention

        GPUVertexMemory? gpuData = chunk.GPUVertexMemory;
        _gpuDataAllocations.Return(gpuData!);
        chunk.GPUVertexMemory = null;
        chunk.GPUUploadedVersion = -1;

        Dictionary<Vector2, ChunkT> listForOldState = GetChunksInState(ChunkState.HasGPUData);
        listForOldState.Remove(chunkCoord);
        chunk.State = ChunkState.HasMesh;
    }

    private IEnumerator DemoteChunkFromHasMesh(Vector2 chunkCoord, ChunkT chunk)
    {
        yield return null; // Eager routine prevention

        VertexDataAllocation memory = chunk.VertexMemory;
        _meshDataAllocations.Return(memory);
        chunk.VertexMemory = VertexDataAllocation.Empty;
        chunk.VerticesGeneratedForVersion = -1;

        Dictionary<Vector2, ChunkT> listForOldState = GetChunksInState(ChunkState.HasMesh);
        listForOldState.Remove(chunkCoord);
        chunk.State = ChunkState.DataOnly;
    }

    #endregion
}
