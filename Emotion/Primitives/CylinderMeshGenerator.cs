#region Using

using System.Diagnostics;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Primitives
{
	public class CylinderMeshGenerator
	{
		public int Sides = 20;

		public float RadiusTop = 1;
		public float RadiusBottom = 1;
		public float Height = 2;

		public bool Capped;

		public Mesh GenerateMesh(string name = "CylinderMesh")
		{
			Debug.Assert(Sides >= 3);

			var vertices = new VertexData[Sides * 4 + (Capped ? 2 : 0)];
			var indices = new ushort[Sides * 6 + (Capped ? (3 * Sides) * 2 : 0)];

			var prevTopX = (float) (RadiusTop * Math.Cos(0));
			var prevTopY = (float) (RadiusTop * Math.Sin(0));

			var prevBottomX = (float) (RadiusBottom * Math.Cos(0));
			var prevBottomY = (float) (RadiusBottom * Math.Sin(0));

			float startingTopX = prevTopX;
			float startingTopY = prevTopY;
			float startingBottomX = prevBottomX;
			float startingBottomY = prevBottomY;

			float step = 2 * MathF.PI / Sides;
			for (var i = 0; i < Sides; i++)
			{
				float angle = (i + 1) * step;

				var topX = (float) (RadiusTop * Math.Cos(angle));
				var topY = (float) (RadiusTop * Math.Sin(angle));

				var bottomX = (float) (RadiusBottom * Math.Cos(angle));
				var bottomY = (float) (RadiusBottom * Math.Sin(angle));

				if (i == Sides - 1)
				{
					topX = startingTopX;
					topY = startingTopY;
					bottomX = startingBottomX;
					bottomY = startingBottomY;
				}

				int vtxStart = i * 4;
				int idxStart = i * 6;

				// Add the vertices for this quad.
				ref VertexData v1 = ref vertices[vtxStart];
				v1.Vertex = new Vector3(prevBottomX, prevBottomY, 0);
				v1.UV = new Vector2((float) i / Sides, 0);
				v1.Color = Color.WhiteUint;

				indices[idxStart] = (ushort) vtxStart;

				ref VertexData v2 = ref vertices[vtxStart + 1];
				v2.Vertex = new Vector3(bottomX, bottomY, 0);
				v2.UV = new Vector2((float) (i + 1) / Sides, 0);
				v2.Color = Color.WhiteUint;

				indices[idxStart + 1] = (ushort) (vtxStart + 1);

				ref VertexData v3 = ref vertices[vtxStart + 2];
				v3.Vertex = new Vector3(prevTopX, prevTopY, Height);
				v3.UV = new Vector2((float) i / Sides, 1);
				v3.Color = Color.WhiteUint;

				indices[idxStart + 2] = (ushort) (vtxStart + 2);
				indices[idxStart + 3] = (ushort) (vtxStart + 2);
				indices[idxStart + 4] = (ushort) (vtxStart + 1);

				ref VertexData v6 = ref vertices[vtxStart + 3];
				v6.Vertex = new Vector3(topX, topY, Height);
				v6.UV = new Vector2((float) (i + 1) / Sides, 1);
				v6.Color = Color.WhiteUint;
				indices[idxStart + 5] = (ushort) (vtxStart + 3);

				// Set as previous to be used for the next quad.
				prevTopX = topX;
				prevTopY = topY;

				prevBottomX = bottomX;
				prevBottomY = bottomY;

				// Add normals
				//float n1x = bottomX;
				//float n1y = bottomY;
				//float n1z = 0;
				//float n1Length = (float)Math.Sqrt(n1x * n1x + n1y * n1y + n1z * n1z);
				//n1x /= n1Length;
				//n1y /= n1Length;
				//n1z /= n1Length;

				//float n2x = topX;
				//float n2y = topY;
				//float n2z = Height;
				//float n2Length = (float)Math.Sqrt(n2x * n2x + n2y * n2y + n2z * n2z);
				//n2x /= n2Length;
				//n2y /= n2Length;
				//n2z /= n2Length;

				//float n3x = bottomXNext;
				//float n3y = bottomYNext;
				//float n3z = 0;
				//float n3Length = (float)Math.Sqrt(n3x * n3x + n3y * n3y + n3z * n3z);
				//n3x /= n3Length;
				//n3y /= n3Length;
				//n3z /= n3Length;

				//float n4x = topXNext;
				//float n4y = topYNext;
				//float n4z = Height;
				//float n4Length = (float)Math.Sqrt(n4x * n4x + n4y * n4y + n4z * n4z);
				//n4x /= n4Length;
				//n4y /= n4Length;
				//n4z /= n4Length;
			}

			if (Capped)
			{
				int vtxStart = Sides * 4;

				// Top center point
				ref VertexData v1 = ref vertices[vtxStart];
				v1.Vertex = new Vector3(0, 0, Height);
				v1.UV = new Vector2(0, 1); // todo
				v1.Color = Color.WhiteUint;

				// Cap on top
				int idxStart = Sides * 6;
				for (var i = 0; i < Sides; i++)
				{
					int quadStart = i * 4;
					int idxWriting = idxStart + i * 3;

					// Add the indices for the top triangles
					indices[idxWriting] = (ushort) vtxStart;
					indices[idxWriting + 1] = (ushort) (quadStart + 2);
					indices[idxWriting + 2] = (ushort) (quadStart + 3);
				}

				// Bottom center point
				ref VertexData v2 = ref vertices[vtxStart + 1];
				v2.Vertex = new Vector3(0, 0, 0);
				v2.UV = new Vector2(0, 0); // todo
				v2.Color = Color.Red.ToUint();

				// Cap on bottom
				idxStart += Sides * 3;
				for (var i = 0; i < Sides; i++)
				{
					int quadStart = i * 4;
					int idxWriting = idxStart + i * 3;

					// Add the indices for the top triangles
					indices[idxWriting + 2] = (ushort) (vtxStart + 1);
					indices[idxWriting + 1] = (ushort) quadStart;
					indices[idxWriting] = (ushort) (quadStart + 1);
				}
			}

			var mesh = new Mesh
			{
				Name = name,
				Material = MeshMaterial.DefaultMaterial,
				Vertices = vertices,
				Indices = indices
			};

			return mesh;
		}
	}
}