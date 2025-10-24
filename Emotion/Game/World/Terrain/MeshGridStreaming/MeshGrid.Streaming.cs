#nullable enable

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Core.Utility.Time;
using Emotion.Editor;
using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Game.World.Terrain.MeshGridStreaming;
using Emotion.Graphics.Data;
using Emotion.Graphics.Memory;
using System.Collections.Concurrent;

namespace Emotion.Game.World.Terrain;

public abstract partial class MeshGrid<T, ChunkT, IndexT>
{
    protected struct ChunkStreamRequest(Vector2 chunkCoord, float distance, ChunkState state)
    {
        public Vector2 ChunkCoord = chunkCoord;
        public float Distance = distance;
        public ChunkState State = state;
    }

    #region Chunk State Update logic

    /// <summary>
    /// The range (in world units) to load chunk meshes in.
    /// </summary>
    public int SimulationRange { get; init; }

    /// <summary>
    /// The range (in world units) to prepare chunks for rendering in.
    /// </summary>
    public int RenderRange { get; init; }

    private List<GameObject> _streamActors = new();

    private HashSet<Vector2> _currentChunksTouched = new HashSet<Vector2>(256);
    private List<ChunkStreamRequest> _currentChunkStateRequest = new(64);
    private List<Vector2> _currentChunkStateRequestUntouched = new(64);
    private Dictionary<Vector2, int> _currentChunkStateRequestDedupe = new(64);

    private List<ChunkStreamRequest> _finishedChunkRequest = new(64);
    private List<Vector2> _finishedChunkRequestUntouched = new(64);

    private void SwapDataFromLastRequest()
    {
        Assert(GLThread.IsGLThread()); // We don't need GL, we need main thread.

        _finishedChunkRequest.Clear();
        (_finishedChunkRequest, _currentChunkStateRequest) = (_currentChunkStateRequest, _finishedChunkRequest);

        _finishedChunkRequestUntouched.Clear();
        (_finishedChunkRequestUntouched, _currentChunkStateRequestUntouched) = (_currentChunkStateRequestUntouched, _finishedChunkRequestUntouched);
    }

    public void AddStreamActor(GameObject actor)
    {
        lock (_streamActors)
        {
            _streamActors.Add(actor);
        }
    }

    public void RemoveStreamActor(GameObject actor)
    {
        lock (_streamActors)
        {
            _streamActors.Remove(actor);
        }
    }

    public IEnumerable<Vector2> DebugOnly_ForEachStreamActorPos()
    {
        lock (_streamActors)
        {
            foreach (GameObject actor in _streamActors)
            {
                yield return actor.Position2D;
            }

            if (EngineEditor.IsOpen || _streamActors.Count == 0)
                yield return Engine.Renderer.Camera.Position2;
        }
    }

    private void ChunkStreamStateUpdate()
    {
        _currentChunksTouched.Clear();
        _currentChunkStateRequest.Clear();
        _currentChunkStateRequestDedupe.Clear();

        bool anyStreamActors = false;
        lock (_streamActors)
        {
            foreach (GameObject actor in _streamActors)
            {
                PromoteChunksAround(actor.Position2D, SimulationRange, RenderRange);
                anyStreamActors = true;
            }
        }

        // If no actors, just load around camera (also in the editor)
        if (EngineEditor.IsOpen || !anyStreamActors)
            PromoteChunksAround(Engine.Renderer.Camera.Position2, SimulationRange, RenderRange);

        _currentChunkStateRequest.Sort(static (a, b) => MathF.Sign(a.Distance - b.Distance));

        foreach ((Vector2 chunkCoord, ChunkT chunk) in _chunks)
        {
            if (chunk.State == ChunkState.DataOnly)
                continue;

            if (!_currentChunksTouched.Contains(chunkCoord))
                _currentChunkStateRequestUntouched.Add(chunkCoord);
        }
    }

    private void PromoteChunksAround(Vector2 pos, int simRange, int renderRange)
    {
        Vector2 chunkSize = ChunkSize * TileSize;
        Vector2 half = chunkSize / 2f;

        float unloadRange = simRange;

        Rectangle bounds = new Rectangle
        {
            Size = new Vector2(unloadRange * 2f),
            Center = pos
        };
        bounds.SnapToGrid(chunkSize);
        bounds.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        float loadChunkRangeSq = simRange * simRange;
        float renderChunkRangeSq = renderRange * renderRange;
        float unloadRangeSq = unloadRange * unloadRange;

        for (float y = min.Y; y <= max.Y; y += chunkSize.Y)
        {
            for (float x = min.X; x <= max.X; x += chunkSize.X)
            {
                Vector2 chunkCoord = new Vector2(x, y) / chunkSize;
                chunkCoord = chunkCoord.Round();
                IStreamableGridChunk? chunk = GetChunk(chunkCoord);

                _currentChunksTouched.Add(chunkCoord);

                Vector2 tileCenter = new Vector2(x, y) + half;
                float distanceSq = Vector2.DistanceSquared(tileCenter, pos);
                if (distanceSq > loadChunkRangeSq)
                {
                    // Chunk doesn't exist anyway.
                    if (chunk == null) continue;

                    // Further away than unload range.
                    if (distanceSq > unloadRangeSq)
                        SubmitChunkStateRequest(chunkCoord, distanceSq, ChunkState.DataOnly);
                }
                // If the chunk is missing request a generation
                else if (chunk == null)
                {
                    // This chunk doesn't exist, query loading/generation and skip.
                    if (CanGenerateChunks)
                        SubmitChunkStateRequest(chunkCoord, distanceSq, ChunkState.DataOnly);
                }
                else if (distanceSq <= renderChunkRangeSq)
                {
                    SubmitChunkStateRequest(chunkCoord, distanceSq, ChunkState.HasGPUData, chunk.State);
                }
                else // Just within sim range, demote from HasGPU
                {
                    SubmitChunkStateRequest(chunkCoord, distanceSq, ChunkState.HasMesh, chunk.State);
                }
            }
        }
    }

    private void SubmitChunkStateRequest(Vector2 chunkCoord, float distanceSq, ChunkState state, ChunkState? currentState = null)
    {
        if (currentState == state) return;

        if (_currentChunkStateRequestDedupe.TryGetValue(chunkCoord, out int requestIndex)) // Already requested
        {
            ChunkStreamRequest existingRequest = _currentChunkStateRequest[requestIndex];
            if (existingRequest.State <= state)
            {
                existingRequest.Distance = MathF.Min(existingRequest.Distance, distanceSq);
                existingRequest.State = state;
                _currentChunkStateRequest[requestIndex] = existingRequest;
            }
        }
        else
        {
            _currentChunkStateRequest.Add(new ChunkStreamRequest(chunkCoord, distanceSq, state));
            _currentChunkStateRequestDedupe[chunkCoord] = _currentChunkStateRequest.Count - 1;
        }
    }

    #endregion

    #region Chunk State Update

    private After _chunkStreamUpdate = new After(150);
    private bool _swapped = true;
    private int _currentRequestOffset = 0;
    private int _currentDemoteRequestOffset = 0;

    private const int REQUEST_PER_TICK_LIMIT = 32;

    private void TickChunkStateUpdates(float dt)
    {
        if (!_swapped)
        {
            SwapDataFromLastRequest();
            _swapped = true;

            int requestsGotten = _finishedChunkRequest.Count;
            _currentRequestOffset = 0;

            int requestsDemote = _finishedChunkRequestUntouched.Count;
            _currentDemoteRequestOffset = 0;
        }

        bool doneWithAllRequests = _currentRequestOffset == _finishedChunkRequest.Count - 1 && _currentDemoteRequestOffset == _finishedChunkRequestUntouched.Count - 1;
        _chunkStreamUpdate.Update(dt);
        if (_chunkStreamUpdate.Finished || doneWithAllRequests)
        {
            ChunkStreamStateUpdate();
            _chunkStreamUpdate.Restart();
            _swapped = false;
        }

        // Promote chunks the streaming system wants us to see.
        int requestsPerformed = 0;
        for (int i = _currentRequestOffset; i < _finishedChunkRequest.Count; i++)
        {
            ChunkStreamRequest request = _finishedChunkRequest[i];

            Vector2 chunkCoord = request.ChunkCoord;
            ChunkT? chunk = GetChunk(chunkCoord);
            TryChunkStateChange(chunkCoord, chunk, request.State);

            _currentRequestOffset = i + 1;

            requestsPerformed++;
            if (requestsPerformed >= REQUEST_PER_TICK_LIMIT)
                break;
        }

        // Demote chunks that were untouched.
        int demotePerformed = 0;
        for (int i = _currentDemoteRequestOffset; i < _finishedChunkRequestUntouched.Count; i++)
        {
            Vector2 chunkCoord = _finishedChunkRequestUntouched[i];
            ChunkT? chunk = GetChunk(chunkCoord);
            TryChunkStateChange(chunkCoord, chunk, ChunkState.DataOnly);

            _currentDemoteRequestOffset = i + 1;

            demotePerformed++;
            if (demotePerformed >= REQUEST_PER_TICK_LIMIT)
                break;
        }
    }

    private void TryChunkStateChange(Vector2 chunkCoord, ChunkT? chunk, ChunkState newState)
    {
        if (chunk == null)
        {
            // Chunk didn't exist - now we want it as DataOnly.
            // Todo: if the chunk was written to a file and unloaded, this is where we would load it too.
            Assert(CanGenerateChunks);
            if (newState == ChunkState.DataOnly)
            {
                // Try to generate a new chunk if there are free job slots
                if (Engine.Jobs.NotManyJobsWithTag("ChunkGeneration"))
                {
                    ChunkT newChunk = InitializeNewChunk();
                    newChunk.State = ChunkState.Loading;
                    bool success = newChunk.TryLockChunkData(ChunkDataLockReason.Generation);
                    Assert(success); // I mean...we just created it...
                    _chunks.Add(chunkCoord, newChunk);

                    Engine.Jobs.Add(GenerateChunkRoutineAsync(chunkCoord, newChunk), false, "ChunkGeneration");
                }
            }
            return;
        }

        // If the chunk is currently calculating something,
        // wait for that to finish before we promote or demote it.
        if (!chunk.TryLockChunkData(ChunkDataLockReason.PromotionDemotion)) return;
        chunk.UnlockChunkData();

        bool updateMesh = false;
        ChunkState chunkState = chunk.State;
        int diff = Math.Abs(chunkState - newState);
        bool isPromotion = chunkState < newState;
        while (diff != 0)
        {
            chunkState = chunk.State;
            ChunkState nextState = chunkState + (isPromotion ? 1 : -1);

            if (isPromotion)
            {
                updateMesh = true;
                if (nextState == ChunkState.HasMesh)
                    PromoteChunkToHasMesh(chunkCoord, chunk);
                else if (nextState == ChunkState.HasGPUData)
                    PromoteChunkToHasGPU(chunkCoord, chunk);
            }
            else
            {
                if (nextState == ChunkState.HasMesh)
                    DemoteChunkFromGPUData(chunkCoord, chunk);
                else if (nextState == ChunkState.DataOnly)
                    DemoteChunkFromHasMesh(chunkCoord, chunk);
            }

            chunkState = chunk.State;
            diff = Math.Abs(chunkState - newState);
        }

        if (updateMesh)
            RequestChunkMeshUpdate(chunkCoord, chunk);
    }

    #endregion

    #region State Change - Generate

    public bool CanGenerateChunks { get; protected set; }

    private IEnumerator GenerateChunkRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(chunk.IsChunkDataLocked(ChunkDataLockReason.Generation));
        GenerateChunkInternal(chunkCoord, chunk);

        chunk.State = ChunkState.DataOnly;
        chunk.UnlockChunkData();
        // Lets execute the OnCreated in the main thread...just in case.
        GLThread.ExecuteOnGLThreadAsync(OnChunkCreated, chunkCoord, chunk);

        yield return null;
    }

    protected virtual void GenerateChunkInternal(Vector2 chunkCoord, ChunkT chunk)
    {

    }

    #endregion

    #region Chunk Mesh Update

    private LinkedList<Vector2> _chunkUpdateQueue = new LinkedList<Vector2>();
    private LinkedList<Vector2> _chunkUpdatePriorityQueue = new LinkedList<Vector2>();
    private Lock _chunkUpdateLock = new Lock();
    private const int MESH_UPDATE_THREAD_FACTOR = 3;

    protected override void OnChunkChanged(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkChanged(chunkCoord, newChunk);
        RequestChunkMeshUpdate(chunkCoord, newChunk, true);
    }

    public void RequestChunkMeshUpdate(Vector2 chunkCoord, ChunkT chunk, bool highPriority = false)
    {
        // If important or got free slots, directly add the job
        if (GLThread.IsGLThread() &&
            (highPriority || Engine.Jobs.NotManyJobsWithTag("ChunkMeshUpdate", MESH_UPDATE_THREAD_FACTOR)) &&
            chunk.TryLockChunkData(ChunkDataLockReason.MeshUpdate)
           )
        {
            chunk.MeshUpdateRequested = false;
            chunk.MeshUpdatePriorityUpdateRequested = false;
            Engine.Jobs.Add(ChunkMeshUpdateRoutineAsync(chunkCoord, chunk), highPriority, "ChunkMeshUpdate");
        }
        else // Chunk is busy :/
        {
            bool wasRequested = chunk.MeshUpdateRequested;

            chunk.MeshUpdateRequested = true;
            chunk.MeshUpdatePriorityUpdateRequested = chunk.MeshUpdatePriorityUpdateRequested || highPriority;

            if (!wasRequested)
            {
                lock (_chunkUpdateLock)
                {
                    if (highPriority)
                        _chunkUpdatePriorityQueue.AddLast(chunkCoord);
                    else
                        _chunkUpdateQueue.AddLast(chunkCoord);
                }
            }
        }
    }

    private void TickChunkMeshUpdates()
    {
        lock (_chunkUpdateLock)
        {
            Inner_TickChunkMeshUpdates();
        }
    }

    private void Inner_TickChunkMeshUpdates()
    {
        LinkedListNode<Vector2>? currentNode = _chunkUpdatePriorityQueue.First;
        while (currentNode != null)
        {
            bool remove = false;
            Vector2 chunkCoord = currentNode.Value;
            ChunkT? chunk = GetChunk(chunkCoord);
            if (chunk == null || !chunk.MeshUpdateRequested)
            {
                remove = true;
            }
            else
            {
                if (chunk.TryLockChunkData(ChunkDataLockReason.MeshUpdate))
                {
                    remove = true;

                    chunk.MeshUpdateRequested = false;
                    chunk.MeshUpdatePriorityUpdateRequested = false;
                    Engine.Jobs.Add(ChunkMeshUpdateRoutineAsync(chunkCoord, chunk), true, "ChunkMeshUpdate");
                }
            }

            LinkedListNode<Vector2>? nextNode = currentNode.Next;
            if (remove)
                _chunkUpdatePriorityQueue.Remove(currentNode);
            currentNode = nextNode;
        }

        currentNode = _chunkUpdateQueue.First;
        while (currentNode != null)
        {
            if (!Engine.Jobs.NotManyJobsWithTag("ChunkMeshUpdate", MESH_UPDATE_THREAD_FACTOR)) break;

            bool remove = false;
            Vector2 chunkCoord = currentNode.Value;
            ChunkT? chunk = GetChunk(chunkCoord);
            if (chunk == null || !chunk.MeshUpdateRequested)
            {
                remove = true;
            }
            else
            {
                if (chunk.TryLockChunkData(ChunkDataLockReason.MeshUpdate))
                {
                    remove = true;

                    chunk.MeshUpdateRequested = false;
                    chunk.MeshUpdatePriorityUpdateRequested = false;
                    Engine.Jobs.Add(ChunkMeshUpdateRoutineAsync(chunkCoord, chunk), false, "ChunkMeshUpdate");
                }
            }

            LinkedListNode<Vector2>? nextNode = currentNode.Next;
            if (remove)
                _chunkUpdateQueue.Remove(currentNode);
            currentNode = nextNode;
        }
    }

    public IEnumerator InlinePerformMeshUpdateRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        // The chunk must already be locked, since only the main thread can lock!
        chunk.TransferLockReason(ChunkDataLockReason.MeshUpdate);

        chunk.MeshUpdateRequested = false;
        chunk.MeshUpdatePriorityUpdateRequested = false;
        yield return ChunkMeshUpdateRoutineAsync(chunkCoord, chunk);
    }

    private IEnumerator ChunkMeshUpdateRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(chunk.IsChunkDataLocked(ChunkDataLockReason.MeshUpdate));

        if (chunk.State >= ChunkState.HasMesh)
        {
            UpdateChunkVertices(chunkCoord, chunk);
        }

        if (chunk.State >= ChunkState.HasGPUData)
        {
            yield return GLThread.ExecuteOnGLThreadAsync(static (chunk) =>
            {
                // Assert that the chunk is really in this state.
                Assert(chunk.GPUVertexMemory != null);
                if (chunk.GPUVertexMemory == null) return;

                Assert(chunk.VertexMemory.Allocated);
                if (!chunk.VertexMemory.Allocated) return;

                chunk.GPUVertexMemory.VBO.Upload(chunk.VertexMemory, chunk.VerticesUsed);
                chunk.GPUUploadedVersion = chunk.VerticesGeneratedForVersion;
            }, chunk);
        }
        chunk.UnlockChunkData();
    }

    #endregion

    #region Chunk List

    public IEnumerable<(Vector2, IStreamableGridChunk)> DebugOnly_ForEachStreamableChunk()
    {
        foreach (KeyValuePair<Vector2, ChunkT> item in _chunks)
        {
            yield return (item.Key, item.Value);
        }
    }

    private Dictionary<ChunkState, Dictionary<Vector2, ChunkT>> _chunksLoaded = new();

    public Dictionary<Vector2, ChunkT> GetChunksInState(ChunkState state)
    {
        Assert(GLThread.IsGLThread());

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

    private void RemoveChunkFromLoadedList(Vector2 chunkCoord)
    {
        foreach (KeyValuePair<ChunkState, Dictionary<Vector2, ChunkT>> item in _chunksLoaded)
        {
            item.Value.Remove(chunkCoord);
        }
    }

    #endregion

    #region Promotion and Allocation Logic

    private ObjectPool<VertexDataAllocation> _meshDataAllocations = new ObjectPool<VertexDataAllocation>();
    private ObjectPoolManual<GPUVertexMemory> _gpuDataAllocations = new ObjectPoolManual<GPUVertexMemory>(() => null!);

    private void PromoteChunkToHasMesh(Vector2 chunkCoord, ChunkT chunk)
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

        Dictionary<Vector2, ChunkT> listForNewState = GetChunksInState(ChunkState.HasMesh);
        listForNewState.Add(chunkCoord, chunk);
        chunk.State = ChunkState.HasMesh;
    }

    private void PromoteChunkToHasGPU(Vector2 chunkCoord, ChunkT chunk)
    {
        Assert(chunk.GPUVertexMemory == null);
        if (chunk.GPUVertexMemory != null)
            GPUMemoryAllocator.FreeBuffer(chunk.GPUVertexMemory);

        Assert(chunk.VertexMemory.Allocated);
        if (!chunk.VertexMemory.Allocated)
            return;

        GPUVertexMemory newGpuMemory = _gpuDataAllocations.Get();
        if (newGpuMemory == null)
        {
            Assert(GLThread.IsGLThread());
            chunk.GPUVertexMemory = GPUMemoryAllocator.AllocateBuffer(chunk.VertexMemory.Format);
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
    }

    protected Span<VertexData_Pos_UV_Normal_Color> ResizeVertexMemoryAndGetSpan(ref VertexDataAllocation vertexMemory, Vector2 chunkCoord, int vertexCount)
    {
        Assert(vertexMemory.Allocated);

        if (vertexMemory.VertexCount < vertexCount)
            vertexMemory = VertexDataAllocation.Reallocate(ref vertexMemory, vertexCount);

        return vertexMemory.GetAsSpan<VertexData_Pos_UV_Normal_Color>();
    }

    #endregion

    #region Demotion and Deallocation Logic

    private void DemoteChunkFromGPUData(Vector2 chunkCoord, ChunkT chunk)
    {
        GPUVertexMemory? gpuData = chunk.GPUVertexMemory;
        _gpuDataAllocations.Return(gpuData!);
        chunk.GPUVertexMemory = null;
        chunk.GPUUploadedVersion = -1;

        Dictionary<Vector2, ChunkT> listForOldState = GetChunksInState(ChunkState.HasGPUData);
        listForOldState.Remove(chunkCoord);
        chunk.State = ChunkState.HasMesh;
    }

    private void DemoteChunkFromHasMesh(Vector2 chunkCoord, ChunkT chunk)
    {
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
