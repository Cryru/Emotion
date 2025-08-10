#nullable enable

namespace Emotion.Game.World.Components;

public interface IGameObjectTransformProvider
{
    public Vector3 GetForwardModelSpace();

    public void OnModelMatrixInvalidated();

    public Matrix4x4 CalculateModelMatrix(GameObject obj, out Matrix4x4 scaleMatrix, out Matrix4x4 rotationMatrix, out Matrix4x4 translationMatrix);

    public Rectangle GetBoundingRect(GameObject obj);

    public Cube GetBoundingCube(GameObject obj);

    public Sphere GetBoundingSphere(GameObject obj);
}

public struct DefaultGameObjectTransformProvider : IGameObjectTransformProvider
{
    public Matrix4x4 CalculateModelMatrix(GameObject obj, out Matrix4x4 scaleMatrix, out Matrix4x4 rotationMatrix, out Matrix4x4 translationMatrix)
    {
        scaleMatrix = Matrix4x4.CreateScale(obj.Scale3D);

        Vector3 objRotation = obj.Rotation;
        rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(objRotation.Y, objRotation.X, objRotation.Z);

        translationMatrix = Matrix4x4.CreateTranslation(obj.Position3D);

        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    public Cube GetBoundingCube(GameObject obj)
    {
        return new Cube(obj.Position3D, obj.Scale3D / 2f);
    }

    public Rectangle GetBoundingRect(GameObject obj)
    {
        return new Rectangle(obj.Position2D, obj.Scale2D);
    }

    public Sphere GetBoundingSphere(GameObject obj)
    {
        Vector3 scale = obj.Scale3D;
        return new Sphere(obj.Position3D + scale / 2f, MathF.Max(scale.X, MathF.Max(scale.Y, scale.Z)) / 2f);
    }

    public Vector3 GetForwardModelSpace()
    {
        return Renderer.Forward;
    }

    public void OnModelMatrixInvalidated()
    {

    }
}