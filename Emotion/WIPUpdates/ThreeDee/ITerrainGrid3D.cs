namespace Emotion.WIPUpdates.ThreeDee;

public interface ITerrainGrid3D
{
    public Vector2 TileSize { get; }

    public IEnumerator InitRuntimeDataRoutine();

    public float GetHeightAt(Vector2 worldSpace);

    public void Update(float dt);

    public void Render(RenderComposer c, Rectangle clipRect);
}