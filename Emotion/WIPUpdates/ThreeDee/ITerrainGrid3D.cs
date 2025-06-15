namespace Emotion.WIPUpdates.ThreeDee;

public interface ITerrainGrid3D
{

    public const float SWEEP_EPSILON = 0.001f;

    public Vector2 TileSize { get; }

    public IEnumerator InitRuntimeDataRoutine();

    public float GetHeightAt(Vector2 worldSpace);

    public void Update(float dt);

    public void Render(RenderComposer c, Rectangle clipRect);

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData);

    public Vector3 SweepCube(Cube cube, Vector3 movement);

    #endregion
}