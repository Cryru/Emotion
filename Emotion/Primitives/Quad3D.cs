#nullable enable

#region Using

using Emotion.Graphics.Data;
using Emotion.Game.World.ThreeDee;

#endregion

namespace Emotion.Primitives;

/// <summary>
/// A plane facing the Z axis, with the origin in the middle.
/// </summary>
public class Quad3D
{
    #region Unit Entity

    private static MeshEntity? _entity;
    private static Lock _entityCreationLock = new();

    public static MeshEntity GetEntity()
    {
        // Check if created
        if (_entity != null) return _entity;

        // Create
        lock (_entityCreationLock)
        {
            // Recheck if created
            if (_entity != null) return _entity;

            VertexDataAllocation alloc = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal.Format, 4);
            Span<VertexData_Pos_UV_Normal> vertexData = alloc.GetAsSpan<VertexData_Pos_UV_Normal>();

            ushort[] indices = new ushort[6];
            IndexBuffer.FillQuadIndices(indices, 0);

            vertexData[0].Position = new Vector3(-0.5f, -0.5f, 0);
            vertexData[0].UV = new Vector2(-0.5f, -0.5f);
            vertexData[1].Position = new Vector3(0.5f, -0.5f, 0);
            vertexData[1].UV = new Vector2(0.5f, -0.5f);
            vertexData[2].Position = new Vector3(0.5f, 0.5f, 0);
            vertexData[2].UV = new Vector2(0.5f, 0.5f);
            vertexData[3].Position = new Vector3(-0.5f, 0.5f, 0);
            vertexData[3].UV = new Vector2(-0.5f, 0.5f);

            for (var i = 0; i < vertexData.Length; i++)
            {
                vertexData[i].Normal = new Vector3(0, 0, 1);
            }

            _entity = new MeshEntity([new Mesh(alloc, indices, MeshMaterial.DefaultMaterialOneSided, "Quad")], "Quad");
        }

        return _entity;
    }

    #endregion
}