using Emotion.WIPUpdates.Grids;

namespace Emotion.Game.Terrain;

public interface ITerrainGrid3D : IGridWorldSpaceTiles
{
    public Vector2 TileSize { get; }

    public IEnumerator InitRuntimeDataRoutine();

    public void UnloadRuntimeData();

    public float GetHeightAt(Vector2 worldSpace);

    public void Update(float dt);

    public void Render(RenderComposer c, Frustum frustum);

    public bool IsTileInBounds(Vector2 tile);

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData);

    public Vector3 SweepCube(Cube cube, Vector3 movement);

    #endregion
}