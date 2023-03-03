#region Using

using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;

#endregion

#nullable enable

namespace Emotion.Primitives
{
	/// <summary>
	/// A plane facing the Z axis, with the origin in the middle.
	/// </summary>
	public class Quad3D : Transform3D
	{
		public Color Tint = Color.White;
		public Texture? Texture = null;

		public virtual void Render(RenderComposer c)
		{
			c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);

			Span<VertexData> vertices = c.RenderStream.GetStreamMemory(4, BatchMode.Quad, Texture);
			vertices[0].Vertex = new Vector3(-0.5f, -0.5f, 0);
			vertices[0].UV = new Vector2(-0.5f, -0.5f);
			vertices[1].Vertex = new Vector3(0.5f, -0.5f, 0);
			vertices[1].UV = new Vector2(0.5f, -0.5f);
			vertices[2].Vertex = new Vector3(0.5f, 0.5f, 0);
			vertices[2].UV = new Vector2(0.5f, 0.5f);
			vertices[3].Vertex = new Vector3(-0.5f, 0.5f, 0);
			vertices[3].UV = new Vector2(-0.5f, 0.5f);

			for (var i = 0; i < vertices.Length; i++)
			{
				vertices[i].Color = Tint.ToUint();
			}

			c.PopModelMatrix();
		}
	}
}