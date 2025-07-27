using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.One;

namespace Emotion.Game.Terrain.GridStreaming;

#nullable enable

public class ChunkStreamManager
{
    /// <summary>
    /// The range (in world units) to load chunk meshes in.
    /// </summary>
    public int SimulationRange { get; init; }

    /// <summary>
    /// The range (in world units) to prepare chunks for rendering in.
    /// </summary>
    public int RenderRange { get; init; }

    private List<MapObject> _streamActors = new();

    private HashSet<Vector2> _chunksTouched = new HashSet<Vector2>(256);
    private Dictionary<Vector2, ChunkState> _chunkRequestState = new(64);

    public ChunkStreamManager(int simRange, int renderRange)
    {
        if (renderRange > simRange)
            simRange = renderRange;

        SimulationRange = simRange;
        RenderRange = renderRange;
    }

    public void AddStreamActor(MapObject actor)
    {
        lock (_streamActors)
        {
            _streamActors.Add(actor);
        }
    }

    public void RemoveStreamActor(MapObject actor)
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
            foreach (MapObject actor in _streamActors)
            {
                yield return actor.Position2D;
            }
        }

        if (EngineEditor.IsOpen || _streamActors.Count == 0)
            yield return Engine.Renderer.Camera.Position2;
    }

    public void Update(IStreamableGrid grid)
    {
        _chunksTouched.Clear();
        _chunkRequestState.Clear();

        lock (_streamActors)
        {
            foreach (MapObject actor in _streamActors)
                PromoteChunksAround(grid, actor.Position2D, SimulationRange, RenderRange);
        }

        // If no actors, just load around camera (also in the editor)
        if (EngineEditor.IsOpen || _streamActors.Count == 0)
            PromoteChunksAround(grid, Engine.Renderer.Camera.Position2, SimulationRange, RenderRange);

        // Process new requests
        foreach (KeyValuePair<Vector2, ChunkState> request in _chunkRequestState)
        {
            Vector2 coord = request.Key;

            if (request.Value == ChunkState.HasMesh)
                grid.StreamingChunkSetState(coord, ChunkState.HasMesh);
            else if (request.Value == ChunkState.HasGPUData)
                grid.StreamingChunkSetState(coord, ChunkState.HasGPUData);
        }

        grid.StreamingDemoteAllUntouchedChunks(_chunksTouched);
    }

    private void RequestChunkState(Vector2 chunkCoord, ChunkState stateWantToBe)
    {
        bool alreadyRequested = _chunkRequestState.TryGetValue(chunkCoord, out ChunkState currentRequest);
        if (!alreadyRequested || currentRequest < stateWantToBe)
            _chunkRequestState[chunkCoord] = stateWantToBe;
    }

    private void PromoteChunksAround(IStreamableGrid grid, Vector2 pos, int simRange, int renderRange)
    {
        Vector2 chunkSize = grid.ChunkSize * grid.TileSize;
        Vector2 half = chunkSize / 2f;

        Rectangle bounds = new Rectangle
        {
            Size = new Vector2(simRange * 2f),
            Center = pos
        };
        bounds.SnapToGrid(chunkSize);
        bounds.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        float loadChunkRangeSq = simRange * simRange;
        float renderChunkRangeSq = renderRange * renderRange;

        for (float y = min.Y; y <= max.Y; y += chunkSize.Y)
        {
            for (float x = min.X; x <= max.X; x += chunkSize.X)
            {
                Vector2 tileCenter = new Vector2(x, y) + half;
                float len = Vector2.DistanceSquared(tileCenter, pos);
                if (len <= loadChunkRangeSq)
                {
                    Vector2 chunkCoord = new Vector2(x, y) / chunkSize;
                    chunkCoord = chunkCoord.Round();

                    // Attempt to get the chunk
                    IStreamableGridChunk? chunk = grid.GetChunk(chunkCoord);
                    if (chunk == null)
                    {
                        // This chunk doesn't exist, query loading/generation and skip.
                        grid.StreamingGenerateChunk(chunkCoord);
                        continue;
                    }

                    // Chunk is loading a state promotion, don't bother it.
                    if (chunk.LoadingStatePromotion)
                        continue;

                    _chunksTouched.Add(chunkCoord);

                    ChunkState chunkState = chunk.State;
                    if (len <= renderChunkRangeSq)
                    {
                        // If already in this state, skip
                        if (chunkState == ChunkState.HasGPUData)
                            continue;

                        // Make sure we don't promote straight to HasGPUData from DataOnly
                        if (chunkState == ChunkState.HasMesh)
                            RequestChunkState(chunkCoord, ChunkState.HasGPUData);
                        else if (chunkState == ChunkState.DataOnly)
                            RequestChunkState(chunkCoord, ChunkState.HasMesh);
                    }
                    else // Just within sim range
                    {
                        if (chunkState == ChunkState.HasMesh)
                            continue;

                        RequestChunkState(chunkCoord, ChunkState.HasMesh);
                    }
                }
            }
        }
    }

    protected virtual IEnumerator CreateNewChunkRoutineAsync(Vector2 chunkCoord)
    {
        yield break;
    }
}
