#region Using

using Emotion.Game.ThreeDee;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Primitives
{
	/// <summary>
	/// A struct representing a ray.
	/// </summary>
	public struct Ray3D
	{
		public Vector3 Start;
		public Vector3 Direction;

		/// <summary>
		/// Create a ray from a starting position, direction and length.
		/// </summary>
		/// <param name="start">The ray's start.</param>
		/// <param name="direction">The direction of the ray.</param>
		public Ray3D(Vector3 start, Vector3 direction)
		{
			Start = start;
			Direction = direction;
		}

		public bool IntersectWithObject(Object3D obj, out Mesh collidedMesh, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
		{
			collidedMesh = null;
			collisionPoint = Vector3.Zero;
			normal = Vector3.Zero;
			triangleIndex = -1;

			Matrix4x4 matrix = obj.GetModelMatrix();
			MeshEntity entity = obj.Entity;

			for (var i = 0; i < entity.Meshes.Length; i++)
			{
				Mesh mesh = entity.Meshes[i];
				if (IntersectWithMesh(mesh, matrix, out collisionPoint, out normal, out triangleIndex))
				{
					collidedMesh = mesh;
					return true;
				}
			}

			return false;
		}

		public bool IntersectWithMesh(Mesh mesh, Matrix4x4 matrix, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
		{
			collisionPoint = Vector3.Zero;
			normal = Vector3.Zero;
			triangleIndex = -1;

			var closestDistance = float.MaxValue;
			var intersectionFound = false;

			VertexData[] meshVertices = mesh.Vertices;
			ushort[] meshIndices = mesh.Indices;

			// todo: support alternate vertex types.
			if (meshVertices == null) return false;

			for (var i = 0; i < meshIndices.Length; i += 3)
			{
				Vector3 p1 = meshVertices[meshIndices[i]]!.Vertex;
				Vector3 p2 = meshVertices[meshIndices[i + 1]]!.Vertex;
				Vector3 p3 = meshVertices[meshIndices[i + 2]]!.Vertex;

				p1 = Vector4.Transform(new Vector4(p1, 1.0f), matrix).ToVec3();
				p2 = Vector4.Transform(new Vector4(p2, 1.0f), matrix).ToVec3();
				p3 = Vector4.Transform(new Vector4(p3, 1.0f), matrix).ToVec3();

				Vector3 triangleNormal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));

				if (!IntersectWithTriangle(p1, p2, p3, triangleNormal, out float t)) continue;

				if (t < closestDistance)
				{
					closestDistance = t;
					normal = triangleNormal;
					triangleIndex = i;
					intersectionFound = true;
				}
			}

			if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

			return intersectionFound;
		}

		public bool IntersectWithTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, out float distance)
		{
			// Check if the ray is parallel to the triangle
			float normalRayDot = Vector3.Dot(normal, Direction);
			if (Math.Abs(normalRayDot) < float.Epsilon)
			{
				distance = 0;
				return false;
			}

			// Calculate the intersection point
			Vector3 rayToTriangle = p1 - Start;
			distance = Vector3.Dot(rayToTriangle, normal) / normalRayDot;

			if (distance < 0)
			{
				// The intersection point is behind the ray's origin
				return false;
			}

			// Calculate the barycentric coordinates of the intersection point
			Vector3 edge1 = p2 - p1;
			Vector3 edge2 = p3 - p1;
			Vector3 intersectionPoint = Start + distance * Direction;
			Vector3 c = intersectionPoint - p1;
			float d00 = Vector3.Dot(edge1, edge1);
			float d01 = Vector3.Dot(edge1, edge2);
			float d11 = Vector3.Dot(edge2, edge2);
			float denom = d00 * d11 - d01 * d01;
			float u = (d11 * Vector3.Dot(edge1, c) - d01 * Vector3.Dot(edge2, c)) / denom;
			float v = (d00 * Vector3.Dot(edge2, c) - d01 * Vector3.Dot(edge1, c)) / denom;

			// Check if the intersection point is inside the triangle
			return (u >= 0) && (v >= 0) && (u + v <= 1);
		}
	}
}