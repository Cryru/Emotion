#nullable enable

namespace Emotion.Primitives;

public struct Sphere
{
	public Vector3 Origin;
	public float Radius;

	public Sphere(Vector3 origin, float radius)
	{
		Origin = origin;
		Radius = radius;
	}

	public Sphere Transform(Matrix4x4 mat)
	{
		if (!Matrix4x4.Decompose(mat, out Vector3 scale, out Quaternion rot, out Vector3 trans)) return this;

		Vector3 transformedOrigin = Vector3.Transform(Origin, mat);
		float maxScale = Math.Max(Math.Max(scale.X, scale.Y), scale.Z);
		float transformedRadius = Radius * maxScale;
		return new Sphere(transformedOrigin, transformedRadius);
	}
}