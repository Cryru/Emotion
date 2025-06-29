﻿namespace Emotion.WIPUpdates.ThreeDee.GridStreaming;

public interface IStreamableGrid
{
    public ChunkStreamManager ChunkStreamManager { get; }

    public Vector2 TileSize { get; }

    public Vector2 ChunkSize { get; }

    public IStreamableGridChunk? GetChunk(Vector2 chunkCoord);

    public void StreamingChunkSetState(Vector2 chunkCoord, ChunkState newState);

    public void StreamingDemoteAllUntouchedChunks(HashSet<Vector2> touchedChunks);

    public void StreamingGenerateChunk(Vector2 chunkCoord);

    public IEnumerable<(Vector2, IStreamableGridChunk)> DebugOnly_StreamableGridForEachChunk();
}