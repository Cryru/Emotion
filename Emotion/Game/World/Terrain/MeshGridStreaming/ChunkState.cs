namespace Emotion.Game.World.Terrain.MeshGridStreaming;

public enum ChunkState
{
    DataOnly, // Only the flat array structure exists
    HasMesh, // The chunk's vertices are created and can be simulated (if the game allows for simulation of DataOnly chunks then this step is just an optimization)
    HasGPUData // Can be rendered
}
