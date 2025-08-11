#nullable enable

using Emotion.Editor;

namespace Emotion.Game.World.Terrain.GridStreaming;

public struct ChunkStreamRequest(Vector2 chunkCoord, float dist, ChunkState state)
{
    public Vector2 ChunkCoord = chunkCoord;
    public float Distance = dist;
    public ChunkState State = state;
}

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

    private List<GameObject> _streamActors = new();

    private HashSet<Vector2> _chunksTouched = new HashSet<Vector2>(256);
    private List<ChunkStreamRequest> _chunkRequests = new(64);
    private Dictionary<Vector2, int> _chunkRequestDedupe = new(64);

    public ChunkStreamManager(int simRange, int renderRange)
    {
        if (renderRange > simRange)
            simRange = renderRange;

        SimulationRange = simRange;
        RenderRange = renderRange;
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
        }

        if (EngineEditor.IsOpen || _streamActors.Count == 0)
            yield return Engine.Renderer.Camera.Position2;
    }

    public void Update(IStreamableGrid grid)
    {
        _chunksTouched.Clear();
        _chunkRequests.Clear();
        _chunkRequestDedupe.Clear();

        lock (_streamActors)
        {
            foreach (GameObject actor in _streamActors)
                PromoteChunksAround(grid, actor.Position2D, SimulationRange, RenderRange);
        }

        // If no actors, just load around camera (also in the editor)
        if (EngineEditor.IsOpen || _streamActors.Count == 0)
            PromoteChunksAround(grid, Engine.Renderer.Camera.Position2, SimulationRange, RenderRange);

        _chunkRequests.Sort(static (a, b) => MathF.Sign(a.Distance - b.Distance));
        grid.ResolveChunkStateRequests(_chunkRequests, _chunksTouched);
    }

    private void RequestChunkState(ChunkStreamRequest requestState)
    {
        if (_chunkRequestDedupe.TryGetValue(requestState.ChunkCoord, out int requestIndex)) // Already requested
        {
            ChunkStreamRequest existingRequest = _chunkRequests[requestIndex];
            if (existingRequest.State <= requestState.State)
            {
                existingRequest.Distance = MathF.Min(existingRequest.Distance, requestState.Distance);
                existingRequest.State = requestState.State;
                _chunkRequests[requestIndex] = existingRequest;
            }
        }
        else
        {
            _chunkRequests.Add(requestState);
            _chunkRequestDedupe[requestState.ChunkCoord] = _chunkRequests.Count - 1;
        }
    }

    private void PromoteChunksAround(IStreamableGrid grid, Vector2 pos, int simRange, int renderRange)
    {
        Vector2 chunkSize = grid.ChunkSize * grid.TileSize;
        Vector2 half = chunkSize / 2f;

        float unloadRange = simRange * 1.5f;

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
                IStreamableGridChunk? chunk = grid.GetChunk(chunkCoord);

                if (chunk != null)
                {
                    // Chunk is loading something, don't bother it.
                    if (chunk.Busy)
                        continue;

                    _chunksTouched.Add(chunkCoord);
                }

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
                        ChunkState demotedState = chunkState - 1;
                        if (demotedState >= 0)
                            RequestChunkState(new ChunkStreamRequest(chunkCoord, distanceSq, demotedState));
                    }

                    continue;
                }

                // If the chunk is missing request a generation
                if (chunk == null)
                {
                    // This chunk doesn't exist, query loading/generation and skip.
                    RequestChunkState(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.DataOnly));
                    continue;
                }

                if (distanceSq <= renderChunkRangeSq)
                {
                    ChunkState chunkState = chunk.State;

                    // If already in this state, skip
                    if (chunkState == ChunkState.HasGPUData)
                        continue;

                    // Make sure we don't promote straight to HasGPUData from DataOnly
                    if (chunkState == ChunkState.HasMesh)
                        RequestChunkState(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.HasGPUData));
                    else if (chunkState == ChunkState.DataOnly)
                        RequestChunkState(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.HasMesh));
                }
                else // Just within sim range
                {
                    ChunkState chunkState = chunk.State;
                    if (chunkState == ChunkState.HasMesh)
                        continue;

                    RequestChunkState(new ChunkStreamRequest(chunkCoord, distanceSq, ChunkState.HasMesh));
                }
            }
        }
    }
}
