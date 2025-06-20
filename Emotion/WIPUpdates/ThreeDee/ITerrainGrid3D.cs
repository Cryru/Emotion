﻿namespace Emotion.WIPUpdates.ThreeDee;

public interface ITerrainGrid3D
{
    public Vector2 TileSize { get; }

    public IEnumerator InitRuntimeDataRoutine();

    public float GetHeightAt(Vector2 worldSpace);

    public void Update(float dt);

    public void Render(RenderComposer c, Frustum frustum);

    #region Collision

    public bool CollideWithCube<TUserData>(Cube cube, Func<Cube, TUserData, bool> onIntersect, TUserData userData);

    public Vector3 SweepCube(Cube cube, Vector3 movement);

    #endregion
}