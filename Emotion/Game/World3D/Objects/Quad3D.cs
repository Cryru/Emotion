#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Game.World3D.Objects;

/// <summary>
/// A plane facing the Z axis, with the origin in the middle.
/// </summary>
public class Quad3D : GameObject3D
{
	public Texture? Texture = null;

	public static MeshEntity? QuadEntity;

	public Quad3D()
	{
		Size3D = new Vector3(10);
	}

	public override Task LoadAssetsAsync()
	{
		if (QuadEntity == null)
		{
			var vertices = new VertexData[4];
			VertexData.SpriteToVertexData(vertices, Vector3.Zero, Vector2.Zero, Color.White);

			vertices[0].Vertex = new Vector3(-0.5f, -0.5f, 0);
			vertices[0].UV = new Vector2(-0.5f, -0.5f);
			vertices[1].Vertex = new Vector3(0.5f, -0.5f, 0);
			vertices[1].UV = new Vector2(0.5f, -0.5f);
			vertices[2].Vertex = new Vector3(0.5f, 0.5f, 0);
			vertices[2].UV = new Vector2(0.5f, 0.5f);
			vertices[3].Vertex = new Vector3(-0.5f, 0.5f, 0);
			vertices[3].UV = new Vector2(-0.5f, 0.5f);

			var indices = new ushort[6];
			IndexBuffer.FillQuadIndices(indices, 0);

			var meshData = new VertexDataMesh3DExtra[4];
			for (var i = 0; i < meshData.Length; i++)
			{
				meshData[i].Normal = new Vector3(0, 0, 1);
			}

			QuadEntity = new MeshEntity
			{
				Name = "Quad",
				Meshes = new[]
				{
					new Mesh("Quad", vertices, meshData, indices)
				},
				BackFaceCulling = false
			};
		}

		Entity = QuadEntity;
		ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;

		return base.LoadAssetsAsync();
	}

	protected override void Resized()
	{
		base.Resized();
		_height = 1;
	}
}