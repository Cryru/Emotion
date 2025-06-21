#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.WIPUpdates.Rendering;

#endregion

namespace Emotion.Graphics.ThreeDee;

/// <summary>
/// 3D geometry and material that makes up a 3D object.
/// </summary>
public class Mesh
{
    private const string DEFAULT_MESH_NAME = "Untitled";

    public string Name;

    public MeshMaterial Material = MeshMaterial.DefaultMaterial;
    public ushort[] Indices;

    public VertexData[] Vertices;
    public VertexDataMesh3DExtra[] ExtraVertexData;
    public Mesh3DVertexDataBones[]? BoneData;

    public int AnimationSkin = 0;

    public VertexDataFormat VertexFormat;
    public VertexDataAllocation VertexAllocation;

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

    public Mesh(VertexDataFormat format, VertexDataAllocation memory, ushort[] indices)
    {
        VertexFormat = format;
        VertexAllocation = memory;
        Indices = indices;
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

    public override string ToString()
    {
        return $"Mesh {Name}";
    }

    #region Iterators

    public IEnumerable<Triangle> ForEachTriangle()
    {
        for (int i = 0; i < Indices.Length; i += 3)
        {
            int i1 = Indices[i];
            int i2 = Indices[i + 1];
            int i3 = Indices[i + 2];

            VertexData v1 = Vertices[i1];
            VertexData v2 = Vertices[i2];
            VertexData v3 = Vertices[i3];

            yield return new Triangle(v1.Vertex, v2.Vertex, v3.Vertex);
        }
    }

    #endregion

    #region Transformations

    public Mesh TransformMeshVertices(Matrix4x4 mat)
    {
        VertexData[]? vertices = Vertices;
        for (var i = 0; i < vertices.Length; i++)
        {
            ref Vector3 vertex = ref vertices[i].Vertex;
            vertices[i].Vertex = Vector3.Transform(vertex, mat);
        }

        VertexDataMesh3DExtra[] extraData = ExtraVertexData;
        if (extraData != null)
        {
            for (var i = 0; i < extraData.Length; i++)
            {
                ref Vector3 vertex = ref extraData[i].Normal;
                extraData[i].Normal = Vector3.TransformNormal(vertex, mat);
            }
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

    public static Mesh ShallowCopyMesh(Mesh m1)
    {
        return new Mesh()
        {
            Vertices = m1.Vertices,
            ExtraVertexData = m1.ExtraVertexData,
            Indices = m1.Indices,
            BoneData = m1.BoneData,
            Material = m1.Material,
            Name = m1.Name + "_Copy"
        };
    }

    public static Mesh ShallowCopyMesh_DeepCopyVertexData(Mesh m1)
    {
        return new Mesh()
        {
            Vertices = (VertexData[]) m1.Vertices.Clone(),
            ExtraVertexData = (VertexDataMesh3DExtra[]) m1.ExtraVertexData.Clone(),
            Indices = m1.Indices,
            BoneData = m1.BoneData,
            Material = m1.Material,
            Name = m1.Name + "_Copy"
        };
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

        ushort[] indices = Indices;
        Texture? texture = null;
        if (Material.DiffuseTexture != null) texture = Material.DiffuseTexture;
        StreamData<VertexData> memory = c.RenderStream.GetStreamMemory((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

        vertData.CopyTo(memory.VerticesData);
        indices.CopyTo(memory.IndicesData);

        for (int i = 0; i < memory.VerticesData.Length; i++)
        {
            ref VertexData vert = ref memory.VerticesData[i];
            vert.Color = Material.DiffuseColor.ToUint();
        }

        ushort structOffset = memory.StructIndex;
        for (var j = 0; j < memory.IndicesData.Length; j++)
        {
            memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
        }
    }
}