#nullable enable

#region Using

using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Primitives
{
	public class SphereMeshGenerator
	{
		public float Radius = 1;
		public int Segments = 20;
		public int Rings = 10;

		public Mesh GenerateMesh()
		{
			int numVertices = (Segments + 1) * (Rings + 1);
			int numIndices = Segments * Rings * 6;

			var vertices = new VertexData[numVertices];
			var indices = new ushort[numIndices];

			float phiStep = MathF.PI / Rings;
			float thetaStep = 2.0f * MathF.PI / Segments;

			// Generate vertices
			for (var r = 0; r <= Rings; r++)
			{
				float phi = r * phiStep;

				for (var s = 0; s <= Segments; s++)
				{
					float theta = s * thetaStep;

					float x = Radius * MathF.Sin(phi) * MathF.Cos(theta);
					float y = Radius * MathF.Cos(phi);
					float z = Radius * MathF.Sin(phi) * MathF.Sin(theta);

					int index = r * (Segments + 1) + s;

					vertices[index].Vertex = new Vector3(x, y, z);
					vertices[index].UV = new Vector2(s / Segments, r / Rings);
					vertices[index].Color = Color.WhiteUint;
				}
			}

			// Generate indices
			var i = 0;
			for (var r = 0; r < Rings; r++)
			{
				for (var s = 0; s < Segments; s++)
				{
					int v1 = r * (Segments + 1) + s;
					int v2 = v1 + Segments + 1;
					int v3 = v2 + 1;
					int v4 = v1 + 1;

					indices[i++] = (ushort) v1;
					indices[i++] = (ushort) v2;
					indices[i++] = (ushort) v3;

					indices[i++] = (ushort) v3;
					indices[i++] = (ushort) v4;
					indices[i++] = (ushort) v1;
				}
			}

			var sphereMesh = new Mesh
			{
				Vertices = vertices,
				Material = MeshMaterial.DefaultMaterial,
				Indices = indices,
				Name = "SphereMesh"
			};
			return sphereMesh;
		}
	}
}