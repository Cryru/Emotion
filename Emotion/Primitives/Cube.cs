#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Graphics;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Primitives;

/// <summary>
/// Represents an AABB.
/// </summary>
public struct Cube
{
    public Vector3 Origin;
    public Vector3 HalfExtents;

    public Cube(Vector3 center, Vector3 halfExtent)
    {
        Origin = center;
        HalfExtents = halfExtent;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3[] GetVertices()
    {
        var arr = new Vector3[8];
        GetVertices(arr);
        return arr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetVertices(Span<Vector3> vertices)
    {
        Assert(vertices.Length >= 8);
        vertices[0] = new Vector3(Origin.X - HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z - HalfExtents.Z);
        vertices[1] = new Vector3(Origin.X + HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z - HalfExtents.Z);
        vertices[2] = new Vector3(Origin.X - HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z - HalfExtents.Z);
        vertices[3] = new Vector3(Origin.X + HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z - HalfExtents.Z);
        vertices[4] = new Vector3(Origin.X - HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z + HalfExtents.Z);
        vertices[5] = new Vector3(Origin.X + HalfExtents.X, Origin.Y - HalfExtents.Y, Origin.Z + HalfExtents.Z);
        vertices[6] = new Vector3(Origin.X - HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z + HalfExtents.Z);
        vertices[7] = new Vector3(Origin.X + HalfExtents.X, Origin.Y + HalfExtents.Y, Origin.Z + HalfExtents.Z);
    }

    public Cube Transform(Matrix4x4 mat)
    {
        var first = true;
        var min = new Vector3(0);
        var max = new Vector3(0);

        Vector3[] vertices = GetVertices();
        for (var i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = Vector3.Transform(vertices[i], mat);
            if (first)
            {
                min = vertex;
                max = vertex;
                first = false;
            }
            else
            {
                // Find the minimum and maximum extents of the vertices
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }
        }

        Vector3 center = (min + max) / 2f;

        Vector3 halfExtent = (max - min) / 2f;
        return new Cube(center, halfExtent);
    }

    private static int[][] _outlineEdges =
    {
        new[] {0, 1}, // Bottom face
        new[] {1, 3},
        new[] {3, 2},
        new[] {2, 0},
        new[] {4, 5}, // Top face
        new[] {5, 7},
        new[] {7, 6},
        new[] {6, 4},
        new[] {0, 4}, // Connecting lines
        new[] {1, 5},
        new[] {2, 6},
        new[] {3, 7}
    };

    public void RenderOutline(RenderComposer c, Color? color = null, float thickness = 1f)
    {
        Span<Vector3> vertices = stackalloc Vector3[8];
        GetVertices(vertices);
        for (var i = 0; i < _outlineEdges.Length; i++)
        {
            int[] edge = _outlineEdges[i];
            c.RenderLine(vertices[edge[0]], vertices[edge[1]], color ?? Color.White, thickness, false);
        }
    }

    private static MeshEntity? _unitCubeEntity;
    private static object _unitCubeCreationLock = new object();

    /// <summary>
    /// Get a mesh entity of a unit cube.
    /// </summary>
    public static MeshEntity GetEntity()
    {
        // Check if created
        if (_unitCubeEntity != null) return _unitCubeEntity;

        // Wait for creation
        lock (_unitCubeCreationLock)
        {
        }

        // Recheck if created
        if (_unitCubeEntity != null) return _unitCubeEntity;

        // Create
        lock (_unitCubeCreationLock)
        {
            var vertexData = new VertexData[8];
            var extraData = new VertexDataMesh3DExtra[8];
            var indices = new ushort[36];

            for (var ic = 0; ic < vertexData.Length; ic++)
            {
                vertexData[ic].Color = Color.WhiteUint;
            }

            // Cube - 36 vertices, 12 triangles, 6 sides
            // Cube - 36 indices, 8 vertices, 6 quads
            vertexData[0].Vertex = new Vector3(-1, -1, 1);
            vertexData[1].Vertex = new Vector3(1, -1, 1);
            vertexData[2].Vertex = new Vector3(1, 1, 1);
            vertexData[3].Vertex = new Vector3(-1, 1, 1);

            vertexData[4].Vertex = new Vector3(-1, -1, -1);
            vertexData[5].Vertex = new Vector3(1, -1, -1);
            vertexData[6].Vertex = new Vector3(1, 1, -1);
            vertexData[7].Vertex = new Vector3(-1, 1, -1);

            vertexData[0].UV = new Vector2(0, 0);
            vertexData[1].UV = new Vector2(0.5f, 0);
            vertexData[2].UV = new Vector2(0.5f, 0.5f);
            vertexData[3].UV = new Vector2(0, 0.5f);

            vertexData[4].UV = new Vector2(0, 0);
            vertexData[5].UV = new Vector2(0.5f, 0);
            vertexData[6].UV = new Vector2(0.5f, 0.5f);
            vertexData[7].UV = new Vector2(0, 0.5f);

            extraData[0].Normal = new Vector3(0, 0, 1);
            extraData[1].Normal = new Vector3(0, 0, 1);
            extraData[2].Normal = new Vector3(0, 0, 1);
            extraData[3].Normal = new Vector3(0, 0, 1);
            extraData[4].Normal = new Vector3(0, 0, -1);
            extraData[5].Normal = new Vector3(0, 0, -1);
            extraData[6].Normal = new Vector3(0, 0, -1);
            extraData[7].Normal = new Vector3(0, 0, -1);

            // Front
            indices[00] = 0;
            indices[01] = 1;
            indices[02] = 2;
            indices[03] = 2;
            indices[04] = 3;
            indices[05] = 0;

            // Right
            indices[06] = 1;
            indices[07] = 5;
            indices[08] = 6;
            indices[09] = 6;
            indices[10] = 2;
            indices[11] = 1;

            // Back
            indices[12] = 7;
            indices[13] = 6;
            indices[14] = 5;
            indices[15] = 5;
            indices[16] = 4;
            indices[17] = 7;

            // Left
            indices[18] = 4;
            indices[19] = 0;
            indices[20] = 3;
            indices[21] = 3;
            indices[22] = 7;
            indices[23] = 4;

            // Bottom
            indices[24] = 4;
            indices[25] = 5;
            indices[26] = 1;
            indices[27] = 1;
            indices[28] = 0;
            indices[29] = 4;

            // Top
            indices[30] = 3;
            indices[31] = 2;
            indices[32] = 6;
            indices[33] = 6;
            indices[34] = 7;
            indices[35] = 3;

            _unitCubeEntity = new MeshEntity
            {
                Name = "Cube",
                BackFaceCulling = true,
                Scale = 1f,
                Meshes = new[]
                {
                    new Mesh(vertexData, extraData, indices)
                }
            };
        }

        return _unitCubeEntity;
    }
}