#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
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

    public Quad3D()
    {
        Size3D = new Vector3(10);
        ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;
    }

    public override Task LoadAssetsAsync()
    {
        Entity = QuadEntity;
        ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;

        return base.LoadAssetsAsync();
    }

    protected override void Resized()
    {
        base.Resized();
        _sizeZ = 1;
    }

    #region Static Entity

    public static MeshEntity? QuadEntity
    {
        get
        {
            _quadEntity ??= CreateQuadEntity();
            return _quadEntity;
        }
    }

    private static MeshEntity? _quadEntity;

    private static MeshEntity CreateQuadEntity()
    {
        var indices = new ushort[6];
        IndexBuffer.FillQuadIndices(indices, 0);

        Mesh quadMesh = new Graphics.ThreeDee.Mesh("Quad", indices, new MeshMaterial() { BackFaceCulling = false });
        quadMesh.VertexFormat = VertexData_Pos_UV_Normal.Descriptor;
        quadMesh.AllocateVertices(4);
        Span<VertexData_Pos_UV_Normal> vertData = quadMesh.VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal>();

        vertData[0].Position = new Vector3(-0.5f, -0.5f, 0);
        vertData[0].UV = new Vector2(-0.5f, -0.5f);
        vertData[0].Normal = new Vector3(0, 0, 1);

        vertData[1].Position = new Vector3(0.5f, -0.5f, 0);
        vertData[1].UV = new Vector2(0.5f, -0.5f);
        vertData[1].Normal = new Vector3(0, 0, 1);

        vertData[2].Position = new Vector3(0.5f, 0.5f, 0);
        vertData[2].UV = new Vector2(0.5f, 0.5f);
        vertData[2].Normal = new Vector3(0, 0, 1);

        vertData[3].Position = new Vector3(-0.5f, 0.5f, 0);
        vertData[3].UV = new Vector2(-0.5f, 0.5f);
        vertData[3].Normal = new Vector3(0, 0, 1);

        return MeshEntity.CreateFromMesh(quadMesh);
    }

    #endregion
}