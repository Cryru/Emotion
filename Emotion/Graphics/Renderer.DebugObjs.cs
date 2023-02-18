#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics
{
	public sealed partial class RenderComposer
	{
		private List<Vector3>? _triangles;

		public void DbgAddTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			if (!Engine.Configuration.DebugMode) return;
			_triangles ??= new List<Vector3>();
			_triangles.Add(p1);
			_triangles.Add(p2);
			_triangles.Add(p3);
		}

		public void DbgClear()
		{
			_triangles?.Clear();
		}

		private void RenderDebugObjects()
		{
			if (_triangles == null || _triangles.Count == 0) return;

			Span<VertexData> memory = RenderStream.GetStreamMemory((uint) _triangles.Count, BatchMode.SequentialTriangles);
			for (var i = 0; i < memory.Length; i++)
			{
				memory[i].Vertex = _triangles[i];
				memory[i].Color = Color.WhiteUint;
				memory[i].UV = Vector2.Zero;
			}
		}
	}
}