namespace Emotion.Game.World.Terrain.GridStreaming;

public interface IStreamableGridChunk
{
    public ChunkState State { get; }

    public bool LoadingStatePromotion { get; }

    public ChunkState DebugOnly_CalculatedState { get; }
}
