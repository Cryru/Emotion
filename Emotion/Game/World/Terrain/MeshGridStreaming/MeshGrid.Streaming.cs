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

public struct ChunkStreamRequest(Vector2 chunkCoord, float distance, ChunkState state)
{
    public Vector2 ChunkCoord = chunkCoord;
    public float Distance = distance;
    public ChunkState State = state;
}

public abstract partial class MeshGrid<T, ChunkT, IndexT>
{
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

    private IEnumerator StartStreamRoutineAsync()
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

        yield break;
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
                    {
                        ChunkState chunkState = chunk.State;
                        SubmitChunkStateRequest(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.DataOnly));
                    }

                    continue;
                }

                // If the chunk is missing request a generation
                if (chunk == null)
                {
                    // This chunk doesn't exist, query loading/generation and skip.
                    if (CanGenerateChunks)
                        SubmitChunkStateRequest(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.DataOnly));
                    continue;
                }

                if (distanceSq <= renderChunkRangeSq)
                {
                    ChunkState chunkState = chunk.State;

                    // If already in this state, skip
                    if (chunkState == ChunkState.HasGPUData)
                        continue;

                    SubmitChunkStateRequest(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.HasGPUData));
                }
                else // Just within sim range, demote from HasGPU
                {
                    ChunkState chunkState = chunk.State;
                    if (chunkState == ChunkState.HasMesh)
                        continue;

                    SubmitChunkStateRequest(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.HasMesh));
                }
            }
        }
    }

    private void SubmitChunkStateRequest(ChunkStreamRequest requestState)
    {
        if (_currentChunkStateRequestDedupe.TryGetValue(requestState.ChunkCoord, out int requestIndex)) // Already requested
        {
            ChunkStreamRequest existingRequest = _currentChunkStateRequest[requestIndex];
            if (existingRequest.State <= requestState.State)
            {
                existingRequest.Distance = MathF.Min(existingRequest.Distance, requestState.Distance);
                existingRequest.State = requestState.State;
                _currentChunkStateRequest[requestIndex] = existingRequest;
            }
        }
        else
        {
            _currentChunkStateRequest.Add(requestState);
            _currentChunkStateRequestDedupe[requestState.ChunkCoord] = _currentChunkStateRequest.Count - 1;
        }
    }

    #endregion

    #region Chunk State Update

    private Coroutine _chunkStreamingRoutine = Coroutine.CompletedRoutine;
    private After _chunkStreamUpdate = new After(150);
    private bool _swapped = true;
    private int _currentRequestOffset = 0;
    private int _requestsPerTickLimit = 0;

    private int _currentDemoteRequestOffset = 0;
    private int _currentDemotePerTickLimit = 0;

    private const int PERCENT_CHUNK_REQUEST_PER_TICK = 10;
    private const int MAX_CHUNK_REQUEST_PER_TICK = 20;

    private void TickChunkStateUpdates(float dt)
    {
        if (_chunkStreamingRoutine.Finished)
        {
            if (!_swapped)
            {
                SwapDataFromLastRequest();
                _swapped = true;

                int requestsGotten = _finishedChunkRequest.Count;
                _requestsPerTickLimit = (int)Math.Min(MAX_CHUNK_REQUEST_PER_TICK, (requestsGotten * PERCENT_CHUNK_REQUEST_PER_TICK) / 100f);
                _currentRequestOffset = 0;

                int requestsDemote = _finishedChunkRequestUntouched.Count;
                _currentDemotePerTickLimit = (int)Math.Min(MAX_CHUNK_REQUEST_PER_TICK, (requestsDemote * PERCENT_CHUNK_REQUEST_PER_TICK) / 100f);
                _currentDemoteRequestOffset = 0;
            }

            _chunkStreamUpdate.Update(dt);
            if (_chunkStreamUpdate.Finished)
            {
                ProcessGeneratedChunksAddToList();

                _chunkStreamingRoutine = Engine.Jobs.Add(StartStreamRoutineAsync());
                _chunkStreamUpdate.Restart();
                _swapped = false;
            }
        }

        int requestsPerformed = 0;
        for (int i = _currentRequestOffset; i < _finishedChunkRequest.Count; i++)
        {
            ChunkStreamRequest request = _finishedChunkRequest[i];

            Vector2 chunkCoord = request.ChunkCoord;
            ChunkT? chunk = GetChunk(chunkCoord);
            AttemptChunkStateChange(chunkCoord, chunk, request.State);

            _currentRequestOffset = i + 1;

            requestsPerformed++;
            if (requestsPerformed >= _requestsPerTickLimit)
                break;
        }

        int demotePerformed = 0;
        for (int i = _currentDemoteRequestOffset; i < _finishedChunkRequestUntouched.Count; i++)
        {
            Vector2 chunkCoord = _finishedChunkRequestUntouched[i];
            ChunkT? chunk = GetChunk(chunkCoord);
            AttemptChunkStateChange(chunkCoord, chunk, ChunkState.DataOnly);

            _currentDemoteRequestOffset = i + 1;

            demotePerformed++;
            if (demotePerformed >= _currentDemotePerTickLimit)
                break;
        }
    }

    private void AttemptChunkStateChange(Vector2 chunkCoord, ChunkT? chunk, ChunkState newState)
    {
        if (chunk == null)
        {
            Assert(CanGenerateChunks);
            if (newState == ChunkState.DataOnly)
                AttempQueueChunkGeneration(chunkCoord);
            return;
        }

        if (chunk.Busy) return;
        chunk.Busy = true;

        ChunkState chunkState = chunk.State;
        int diff = Math.Abs(chunkState - newState);
        bool isPromotion = chunkState < newState;
        while (diff != 0)
        {
            chunkState = chunk.State;
            ChunkState nextState = chunkState + (isPromotion ? 1 : -1);

            if (isPromotion)
            {
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

        chunk.Busy = false;
    }

    #endregion

    #region State Change - Generate

    public bool CanGenerateChunks { get; protected set; }

    private HashSet<Vector2> _chunksBeingGenerated = new HashSet<Vector2>();
    private ConcurrentQueue<(Vector2, ChunkT)> _chunksAddToList = new ConcurrentQueue<(Vector2, ChunkT)>();

    private void AttempQueueChunkGeneration(Vector2 chunkCoord)
    {
        if (_chunksBeingGenerated.Contains(chunkCoord)) return;
        if (_chunkGenerationRoutines == null) return; // Not initialized.

        for (int i = 0; i < _chunkGenerationRoutines.Length; i++)
        {
            Coroutine? routine = _chunkGenerationRoutines[i];
            if (routine != null && !routine.Finished) continue;

            ChunkT newChunk = InitializeNewChunk();
            newChunk.Busy = true;
            _chunksBeingGenerated.Add(chunkCoord);
            routine = Engine.Jobs.Add(GenerateChunkRoutineAsync(chunkCoord, newChunk));
            break;
        }
    }

    private IEnumerator GenerateChunkRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
        GenerateChunkInternal(chunkCoord, chunk);

        chunk.State = ChunkState.DataOnly;
        chunk.Busy = false;
        _chunksAddToList.Enqueue((chunkCoord, chunk));

        yield return null;
    }

    protected virtual void GenerateChunkInternal(Vector2 chunkCoord, ChunkT chunk)
    {

    }

    private void ProcessGeneratedChunksAddToList()
    {
        while (_chunksAddToList.TryDequeue(out (Vector2, ChunkT) newChunkData))
        {
            Vector2 chunkCoord = newChunkData.Item1;
            ChunkT newChunk = newChunkData.Item2;

            _chunks.Add(chunkCoord, newChunk);
            _chunksBeingGenerated.Remove(chunkCoord);
            OnChunkCreated(chunkCoord, newChunk);
        }
    }

    #endregion

    #region Chunk Mesh Update

    private HashSet<Vector2> _updateChunks = new HashSet<Vector2>(64);
    private Queue<Vector2> _queuedUpdateChunks = new Queue<Vector2>(64);
    private Queue<Vector2> _queuedUpdateChunksBB = new Queue<Vector2>(64);

    private Queue<Vector2> _queuedUpdateChunksPriority = new Queue<Vector2>(64);
    private Queue<Vector2> _queuedUpdateChunksPriorityBB = new Queue<Vector2>(64);
    private Lock _chunkUpdateLock = new();

    private Coroutine?[]? _chunkGenerationRoutines;
    private Coroutine?[]? _chunkMeshUpdateRoutines;
    private Coroutine?[]? _chunkMeshUpdatePriorityRoutines;
    private bool _chunkUpdateInit;

    protected override void OnChunkChanged(Vector2 chunkCoord, ChunkT newChunk)
    {
        base.OnChunkChanged(chunkCoord, newChunk);
        RequestChunkMeshUpdate(chunkCoord, newChunk, true);
    }

    public void RequestChunkMeshUpdate(Vector2 chunkCoord, ChunkT newChunk, bool highPriority = false)
    {
        lock (_chunkUpdateLock)
        {
            if (_updateChunks.Add(chunkCoord))
            {
                if (highPriority)
                    _queuedUpdateChunksPriority.Enqueue(chunkCoord);
                else
                    _queuedUpdateChunks.Enqueue(chunkCoord);
            }
        }
    }

    private void TickChunkMeshUpdates()
    {
        if (!_chunkUpdateInit)
        {
            if (CanGenerateChunks)
            {
                int maximumChunkGenerations = Math.Max(1, Engine.Jobs.ThreadCount / 2);
                _chunkGenerationRoutines = new Coroutine[maximumChunkGenerations];
            }

            int maximumChunkMeshUpdates = Math.Max(4, Engine.Jobs.ThreadCount);
            _chunkMeshUpdateRoutines = new Coroutine[maximumChunkMeshUpdates];  // Highest priority
            _chunkMeshUpdatePriorityRoutines = new Coroutine[maximumChunkMeshUpdates];  // Highest priority

            _chunkUpdateInit = true;
        }

        AssertNotNull(_chunkMeshUpdateRoutines);
        AssertNotNull(_chunkMeshUpdatePriorityRoutines);

        // Check if any free routines for updating meshes.
        lock (_chunkUpdateLock)
        {
            Assert(_queuedUpdateChunksPriorityBB.Count == 0);
            (_queuedUpdateChunksPriority, _queuedUpdateChunksPriorityBB) = (_queuedUpdateChunksPriorityBB, _queuedUpdateChunksPriority);
            Assert(_queuedUpdateChunksPriority.Count == 0);
            TickMeshUpdatesFromList(_chunkMeshUpdatePriorityRoutines!, _queuedUpdateChunksPriorityBB, _queuedUpdateChunksPriority);
            while (_queuedUpdateChunksPriorityBB.TryDequeue(out Vector2 chunkCoord))
            {
                _queuedUpdateChunksPriority.Enqueue(chunkCoord);
            }
            Assert(_queuedUpdateChunksPriorityBB.Count == 0);

            Assert(_queuedUpdateChunksBB.Count == 0);
            (_queuedUpdateChunks, _queuedUpdateChunksBB) = (_queuedUpdateChunksBB, _queuedUpdateChunks);
            Assert(_queuedUpdateChunks.Count == 0);
            TickMeshUpdatesFromList(_chunkMeshUpdateRoutines!, _queuedUpdateChunksBB, _queuedUpdateChunks);
            while (_queuedUpdateChunksBB.TryDequeue(out Vector2 chunkCoord))
            {
                _queuedUpdateChunks.Enqueue(chunkCoord);
            }
            Assert(_queuedUpdateChunksBB.Count == 0);
        }
    }

    private void TickMeshUpdatesFromList(Coroutine[] updateRoutines, Queue<Vector2> updateChunkQueue, Queue<Vector2> failedAttemptQueue)
    {
        for (int i = 0; i < updateRoutines.Length; i++)
        {
            Coroutine? routine = updateRoutines[i];
            if (routine != null && !routine.Finished) continue;

            while (updateChunkQueue.TryDequeue(out Vector2 chunkCoord))
            {
                ChunkT? chunk = GetChunk(chunkCoord);
                if (chunk == null || chunk.State == ChunkState.DataOnly)
                {
                    _updateChunks.Remove(chunkCoord);
                    continue; // huh?
                }

                Coroutine? newRoutine = AttemptChunkMeshUpdate(chunkCoord, chunk);
                if (newRoutine != null)
                {
                    _updateChunks.Remove(chunkCoord);
                    updateRoutines[i] = newRoutine;
                    break;
                }
                else
                {
                    failedAttemptQueue.Enqueue(chunkCoord);
                }
            }
        }
    }

    private Coroutine? AttemptChunkMeshUpdate(Vector2 chunkCoord, ChunkT chunk)
    {
        if (chunk.Busy) return null;
        chunk.Busy = true;
        return Engine.Jobs.Add(ChunkMeshUpdateRoutineAsync(chunkCoord, chunk));
    }

    private IEnumerator ChunkMeshUpdateRoutineAsync(Vector2 chunkCoord, ChunkT chunk)
    {
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
        chunk.Busy = false;
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

        RequestChunkMeshUpdate(chunkCoord, chunk);
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

        RequestChunkMeshUpdate(chunkCoord, chunk);
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
