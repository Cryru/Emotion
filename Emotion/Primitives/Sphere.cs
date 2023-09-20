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
		Vector3 transformedOrigin = Vector3.Transform(Origin, mat);

		float scaleX = new Vector3(mat.M11, mat.M21, mat.M31).Length();
		float scaleY = new Vector3(mat.M12, mat.M22, mat.M32).Length();
		float scaleZ = new Vector3(mat.M13, mat.M23, mat.M33).Length();
		float maxScale = Math.Max(Math.Max(scaleX, scaleY), scaleZ);
		float transformedRadius = Radius * maxScale;
		return new Sphere(transformedOrigin, transformedRadius);
	}
}