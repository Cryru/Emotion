#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using System;


#endregion

namespace Emotion.Graphics.ThreeDee;

/// <summary>
/// 3D geometry and material that makes up a 3D object.
/// </summary>
public class Mesh
{
	private const string DEFAULT_MESH_NAME = "Untitled";

	public string Name;

	public MeshMaterial Material;
	public ushort[] Indices;

	public VertexData[] Vertices;
	public VertexDataMesh3DExtra[] ExtraVertexData;

	public Mesh3DVertexDataBones[]? BoneData;
	public MeshBone[]? Bones = null;

	public Mesh(VertexData[] vertices, VertexDataMesh3DExtra[] extraData, ushort[] indices)
	{
		Name = DEFAULT_MESH_NAME;
		Vertices = vertices;
		ExtraVertexData = extraData;
		Indices = indices;
		Material = MeshMaterial.DefaultMaterial;
	}

	public Mesh(string name, VertexData[] vertices, VertexDataMesh3DExtra[] extraData, ushort[] indices)
	{
		Name = name;
		Vertices = vertices;
		ExtraVertexData = extraData;
		Indices = indices;
		Material = MeshMaterial.DefaultMaterial;
	}

	// Serialization constructor.
	protected Mesh()
	{
		Name = DEFAULT_MESH_NAME;
		Vertices = null!;
        ExtraVertexData = null!;
		Indices = null!;
		Material = MeshMaterial.DefaultMaterial;
	}

	#region Transformations

	public Mesh TransformMeshVertices(Matrix4x4 mat)
	{
		VertexData[]? vertices = Vertices;
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

	public Mesh SetVerticesAlpha(byte alpha)
	{
		VertexData[]? vertices = Vertices;
		for (var i = 0; i < vertices.Length; i++)
		{
			ref VertexData vertex = ref vertices[i];
			vertex.Color = new Color(vertex.Color).SetAlpha(alpha).ToUint();
		}

		return this;
	}

	public static Mesh CombineMeshes(Mesh m1, Mesh m2, string name)
	{
		var m = new Mesh(
			name,
			new VertexData[m1.Vertices.Length + m2.Vertices.Length],
			new VertexDataMesh3DExtra[m1.ExtraVertexData.Length + m2.ExtraVertexData.Length],
			new ushort[m1.Indices.Length + m2.Indices.Length]);

		m1.Vertices.CopyTo(new Span<VertexData>(m.Vertices));
		m2.Vertices.CopyTo(new Span<VertexData>(m.Vertices, m1.Vertices.Length, m2.Vertices.Length));
		m1.ExtraVertexData.CopyTo(new Span<VertexDataMesh3DExtra>(m.ExtraVertexData));
		m2.ExtraVertexData.CopyTo(new Span<VertexDataMesh3DExtra>(m.ExtraVertexData, m1.ExtraVertexData.Length, m2.ExtraVertexData.Length));
		m1.Indices.CopyTo(new Span<ushort>(m.Indices));
		m2.Indices.CopyTo(new Span<ushort>(m.Indices, m1.Indices.Length, m2.Indices.Length));

		int vertexOffset = m1.Vertices.Length;
		for (int i = m1.Indices.Length; i < m.Indices.Length; i++)
		{
			m.Indices[i] = (ushort) (m.Indices[i] + vertexOffset);
		}

		m.Material = m1.Material;

		return m;
	}

	#endregion

	// deprecate?
	public void Render(RenderComposer c)
	{
		VertexData[]? vertData = Vertices;
		if (vertData == null) return; // Animated meshes need to be rendered by Object3D as it holds the animation context.

		ushort[] indices = Indices;
		Texture? texture = null;
		if (Material.DiffuseTexture != null) texture = Material.DiffuseTexture;
		StreamData<VertexData> memory = c.RenderStream.GetStreamMemory((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

		vertData.CopyTo(memory.VerticesData);
		indices.CopyTo(memory.IndicesData);

		ushort structOffset = memory.StructIndex;
		for (var j = 0; j < memory.IndicesData.Length; j++)
		{
			memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
		}
	}

	#region Cache

	[DontSerialize] public Dictionary<string, MeshBone>? BoneNameCache;

	public void BuildRuntimeBoneCache()
	{
		if (BoneNameCache != null) return;
		if (Bones == null) return;

		BoneNameCache = new Dictionary<string, MeshBone>();
		for (var i = 0; i < Bones.Length; i++)
		{
			MeshBone bone = Bones[i];
			BoneNameCache.Add(bone.Name, bone);
		}
	}

	#endregion
}