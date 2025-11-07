using Emotion.Game.World.Terrain.GridStreaming;
using Emotion.Primitives.Grids;

namespace Emotion.Game.World.Terrain;

public interface ITerrainGrid3D : IGridWorldSpaceTiles
{
    public Vector2 ChunkSize { get; }

    public Vector2 TileSize { get; }

    public float GetHeightAt(Vector2 worldSpace);

    public bool IsTileInBounds(Vector2 tile);

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData);

    public Vector3 SweepCube(Cube cube, Vector3 movement);

    #endregion

    #region Streaming

    public int SimulationRange { get; }

    public int RenderRange { get; }

    #endregion

    #region Debug

    public IEnumerable<(Vector2, IStreamableGridChunk)> DebugOnly_ForEachStreamableChunk();

    public IEnumerable<Vector2> DebugOnly_ForEachStreamActorPos();



    #endregion
}