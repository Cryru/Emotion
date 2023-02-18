#nullable enable

#region Using

using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics.ThreeDee
{
	/// <summary>
	/// 3D geometry and material that makes up a 3D object.
	/// </summary>
	public class Mesh
	{
		public string Name = null!;
		public MeshMaterial Material = null!;

		/// <summary>
		/// One of these must be present, but not both.
		/// </summary>
		public VertexData[]? Vertices;

		public VertexDataWithBones[]? VerticesWithBones;

		public ushort[] Indices = null!;
		public MeshBone[]? Bones = null;

		public Mesh TransformMeshVertices(Matrix4x4 mat)
		{
			VertexData[]? vertices = Vertices;
			if (vertices == null) return this;
			for (var i = 0; i < vertices.Length; i++)
			{
				ref Vector3 vertex = ref vertices[i].Vertex;
				vertices[i].Vertex = Vector3.Transform(vertex, mat);
			}

			return this;
		}

		public Mesh ColorMeshVertices(Color col)
		{
			uint val = col.ToUint();

			VertexData[]? vertices = Vertices;
			if (vertices == null) return this;
			for (var i = 0; i < vertices.Length; i++)
			{
				ref VertexData vertex = ref vertices[i];
				vertex.Color = val;
			}

			return this;
		}

		public void GetTriangleAtIndex(int index, out Vector3 p1, out Vector3 p2, out Vector3 p3)
		{
			VertexData[] verts = Vertices;
			ushort[] indices = Indices;

			// todo: implement for other vertex types.
			if (Vertices == null)
			{
				p1 = Vector3.Zero;
				p2 = Vector3.Zero;
				p3 = Vector3.Zero;
				return;
			}

			p1 = verts[indices[index]].Vertex;
			p2 = verts[indices[index + 1]].Vertex;
			p3 = verts[indices[index + 2]].Vertex;
		}
	}
}