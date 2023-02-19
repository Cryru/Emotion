#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
	public sealed partial class RenderComposer
	{
		private List<Vector3>? _triangles;
		private List<Mesh>? _spheres;

		public void DbgAddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			if (!Engine.Configuration.DebugMode) return;
			_triangles ??= new List<Vector3>();
			_triangles.Add(p1);
			_triangles.Add(p2);
			_triangles.Add(p3);
		}

		public void DbgAddPoint(Vector3 p)
		{
			if (!Engine.Configuration.DebugMode) return;

			var meshGen = new SphereMeshGenerator();
			_spheres ??= new List<Mesh>();
			_spheres.Add(meshGen.GenerateMesh().TransformMeshVertices(Matrix4x4.CreateTranslation(p)));
		}

		public void DbgClear()
		{
			_triangles?.Clear();
			_spheres?.Clear();
		}

		private void RenderDebugObjects()
		{
			if (_triangles != null && _triangles.Count != 0)
			{
				Span<VertexData> memory = RenderStream.GetStreamMemory((uint) _triangles.Count, BatchMode.SequentialTriangles);
				for (var i = 0; i < memory.Length; i++)
				{
					memory[i].Vertex = _triangles[i];
					memory[i].Color = Color.WhiteUint;
					memory[i].UV = Vector2.Zero;
				}
			}

			if (_spheres != null && _spheres.Count > 0)
			{
				for (var i = 0; i < _spheres.Count; i++)
				{
					_spheres[i].Render(this);
				}
			}
		}
	}
}