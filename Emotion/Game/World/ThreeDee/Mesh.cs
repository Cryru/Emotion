#nullable enable

#region Using

using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Game.World.ThreeDee;

/// <summary>
/// 3D geometry and material that makes up a 3D object.
/// </summary>
public class Mesh
{
    private const string DEFAULT_MESH_NAME = "Untitled";

    public bool Valid;
    public string Name;

    public MeshMaterial Material = MeshMaterial.DefaultMaterial;
    public ushort[] Indices;

    public VertexData[]? Vertices { get => null; }
    public VertexDataMesh3DExtra[]? ExtraVertexData { get => null; }
    public Mesh3DVertexDataBones[]? BoneData { get => null; }

    public int AnimationSkin = 0;

    public VertexDataFormat VertexFormat { get => VertexAllocation.Format; }
    public VertexDataAllocation VertexAllocation;

    public Mesh(VertexDataAllocation memory, ushort[] indices, MeshMaterial? material = null, string? name = null)
    {
        VertexAllocation = memory;
        Indices = indices;
        Material = material ?? MeshMaterial.DefaultMaterial;
        Name = name ?? DEFAULT_MESH_NAME;
        Valid = true;
    }

    public void Dispose()
    {
        if (!Valid) return;

        VertexDataAllocation.FreeAllocated(VertexAllocation);
        VertexAllocation = null!;

        Indices = null!;
        Valid = false;
    }

    public override string ToString()
    {
        return $"Mesh {Name}";
    }

    #region Transformations

    public Mesh SetVerticesAlpha(byte alpha)
    {
        return this;
    }

    public static unsafe Mesh CombineMeshes(
        Mesh m1,
        Matrix4x4 transformM1,
        Mesh m2,
        Matrix4x4 transformM2,
        MeshMaterial material,
        string name
    )
    {
        var alloc1 = m1.VertexAllocation;
        var alloc2 = m2.VertexAllocation;

        if (m1.VertexFormat != m2.VertexFormat)
        {
            Assert(false, "Tried combining meshes with different vertex formats.");
            return m1;
        }

        int totalVertices = alloc1.VertexCount + alloc2.VertexCount;
        VertexDataAllocation combinedVerts = VertexDataAllocation.Allocate(alloc1.Format, totalVertices);

        Span<byte> mem1 = new Span<byte>((byte*)alloc1.Pointer, alloc1.VertexCount * alloc1.Format.ElementSize);
        Span<byte> mem2 = new Span<byte>((byte*)alloc2.Pointer, alloc2.VertexCount * alloc2.Format.ElementSize);

        Span<byte> memNew = new Span<byte>((byte*)combinedVerts.Pointer, totalVertices * alloc1.Format.ElementSize);
        mem1.CopyTo(memNew);
        mem2.CopyTo(memNew.Slice(mem1.Length));

        ushort[] combinedIndices = new ushort[m1.Indices.Length + m2.Indices.Length];
        m1.Indices.CopyTo(combinedIndices);
        m2.Indices.CopyTo(combinedIndices, m1.Indices.Length);

        // Offset the indices in the second part
        int vertexOffset = alloc1.VertexCount;
        for (int i = m1.Indices.Length; i < combinedIndices.Length; i++)
        {
            combinedIndices[i] = (ushort)(combinedIndices[i] + vertexOffset);
        }

        bool hasNormals = alloc1.Format.HasNormals;
        for (int i = 0; i < alloc1.VertexCount; i++)
        {
            Vector3 vert = combinedVerts.GetVertexPositionAtIndex(i);
            vert = Vector3.Transform(vert, transformM1);
            combinedVerts.SetVertexPositionAtIndex(i, vert);

            if (hasNormals)
            {
                Vector3 norm = combinedVerts.GetNormalAtIndex(i);
                norm = Vector3.TransformNormal(norm, transformM1);
                combinedVerts.SetNormalAtIndex(vertexOffset + i, norm);
            }
        }

        for (int i = 0; i < alloc2.VertexCount; i++)
        {
            Vector3 vert = combinedVerts.GetVertexPositionAtIndex(vertexOffset + i);
            vert = Vector3.Transform(vert, transformM2);
            combinedVerts.SetVertexPositionAtIndex(vertexOffset + i, vert);

            if (hasNormals)
            {
                Vector3 norm = combinedVerts.GetNormalAtIndex(vertexOffset + i);
                norm = Vector3.TransformNormal(norm, transformM2);
                combinedVerts.SetNormalAtIndex(vertexOffset + i, norm);
            }
        }

        Mesh combinedMesh = new Mesh(combinedVerts, combinedIndices, material, name);

        m1.Dispose();
        m2.Dispose();

        return combinedMesh;
    }

    #endregion
}