#nullable enable

#region Using

using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Standard.MeshGenerators;

public class CylinderMeshGenerator
{
    public int Sides = 20;

    public float RadiusTop = 1;
    public float RadiusBottom = 1;
    public float Height = 2;

    public bool Capped;

    public Mesh GenerateMesh(string name = "CylinderMesh")
    {
        Assert(Sides >= 3);

        int vertexCount = Sides * 4 + (Capped ? 2 : 0);
        VertexDataAllocation vertData = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal.Format, vertexCount);
        Span<VertexData_Pos_UV_Normal> vertices = vertData.GetAsSpan<VertexData_Pos_UV_Normal>();

        ushort[] indices = new ushort[Sides * 6 + (Capped ? 3 * Sides * 2 : 0)];

        var prevTopX = (float) (RadiusTop * Math.Cos(0));
        var prevTopY = (float) (RadiusTop * Math.Sin(0));

        var prevBottomX = (float) (RadiusBottom * Math.Cos(0));
        var prevBottomY = (float) (RadiusBottom * Math.Sin(0));

        float startingTopX = prevTopX;
        float startingTopY = prevTopY;
        float startingBottomX = prevBottomX;
        float startingBottomY = prevBottomY;

        float step = -2 * MathF.PI / Sides;
        for (var i = 0; i < Sides; i++)
        {
            float angle = (i + 1) * step;

            var topX = (float) (RadiusTop * Math.Cos(angle));
            var topY = (float) (RadiusTop * Math.Sin(angle));

            var bottomX = (float) (RadiusBottom * Math.Cos(angle));
            var bottomY = (float) (RadiusBottom * Math.Sin(angle));

            if (i == Sides - 1)
            {
                topX = startingTopX;
                topY = startingTopY;
                bottomX = startingBottomX;
                bottomY = startingBottomY;
            }

            int vtxStart = i * 4;
            int idxStart = i * 6;

            // Add the vertices for this quad.
            ref VertexData_Pos_UV_Normal v1 = ref vertices[vtxStart];
            v1.Position = new Vector3(prevBottomX, prevBottomY, 0);
            v1.UV = new Vector2((float) i / Sides, 0);

            ref VertexData_Pos_UV_Normal v2 = ref vertices[vtxStart + 1];
            v2.Position = new Vector3(bottomX, bottomY, 0);
            v2.UV = new Vector2((float) (i + 1) / Sides, 0);

            ref VertexData_Pos_UV_Normal v3 = ref vertices[vtxStart + 2];
            v3.Position = new Vector3(prevTopX, prevTopY, Height);
            v3.UV = new Vector2((float) i / Sides, 1);

            ref VertexData_Pos_UV_Normal v6 = ref vertices[vtxStart + 3];
            v6.Position = new Vector3(topX, topY, Height);
            v6.UV = new Vector2((float) (i + 1) / Sides, 1);

            indices[idxStart + 0] = (ushort)(vtxStart + 0);
            indices[idxStart + 1] = (ushort)(vtxStart + 1);
            indices[idxStart + 2] = (ushort)(vtxStart + 2);
            indices[idxStart + 3] = (ushort)(vtxStart + 2);
            indices[idxStart + 4] = (ushort)(vtxStart + 1);
            indices[idxStart + 5] = (ushort)(vtxStart + 3);

            // Set as previous to be used for the next quad.
            prevTopX = topX;
            prevTopY = topY;

            prevBottomX = bottomX;
            prevBottomY = bottomY;

            // 1) Compute one normal per slice
            float midAngle = (i + 0.5f) * step;
            float cx = MathF.Cos(midAngle), sy = MathF.Sin(midAngle);
            float slopeZ = (RadiusBottom - RadiusTop) / Height;
            var rawNormal = new Vector3(cx, sy, slopeZ);
            var sliceNormal = Vector3.Normalize(rawNormal);

            for (int k = 0; k < 4; k++)
            {
                vertices[vtxStart + k].Normal = sliceNormal;
            }
        }

        if (Capped)
        {
            int vtxStart = Sides * 4;

            // Top center point
            ref VertexData_Pos_UV_Normal v1 = ref vertices[vtxStart];
            v1.Position = new Vector3(0, 0, Height);
            v1.UV = new Vector2(0, 1); // todo
            v1.Normal = Renderer.Up;

            // Cap on top
            int idxStart = Sides * 6;
            for (var i = 0; i < Sides; i++)
            {
                int quadStart = i * 4;
                int idxWriting = idxStart + i * 3;

                // Add the indices for the top triangles
                indices[idxWriting] = (ushort) vtxStart;
                indices[idxWriting + 1] = (ushort) (quadStart + 2);
                indices[idxWriting + 2] = (ushort) (quadStart + 3);
            }

            // Bottom center point
            ref VertexData_Pos_UV_Normal v2 = ref vertices[vtxStart + 1];
            v2.Position = new Vector3(0, 0, 0);
            v2.UV = new Vector2(0, 0); // todo
            v2.Normal = -Renderer.Up;

            // Cap on bottom
            idxStart += Sides * 3;
            for (var i = 0; i < Sides; i++)
            {
                int quadStart = i * 4;
                int idxWriting = idxStart + i * 3;

                // Add the indices for the top triangles
                indices[idxWriting + 2] = (ushort) (vtxStart + 1);
                indices[idxWriting + 1] = (ushort) quadStart;
                indices[idxWriting] = (ushort) (quadStart + 1);
            }
        }

        return new Mesh(vertData, indices, MeshMaterial.DefaultMaterialTwoSided, name);
    }
}