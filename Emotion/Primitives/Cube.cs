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
    [
        [0, 1], // Bottom face
        [1, 3],
        [3, 2],
        [2, 0],
        [4, 5], // Top face
        [5, 7],
        [7, 6],
        [6, 4],
        [0, 4], // Connecting lines
        [1, 5],
        [2, 6],
        [3, 7]
    ];

    public void RenderOutline(RenderComposer c, Color? color = null, float thickness = 1f)
    {
        Span<Vector3> vertices = stackalloc Vector3[8];
        GetVertices(vertices);
        for (var i = 0; i < _outlineEdges.Length; i++)
        {
            int[] edge = _outlineEdges[i];
            c.RenderLine(vertices[edge[0]], vertices[edge[1]], color ?? Color.White, thickness);
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
            // Recheck if created
            if (_unitCubeEntity != null) return _unitCubeEntity;

            // Create the cube entity

            // Cube - 36 vertices, 12 triangles, 6 sides
            // 36 indices, 8 vertices, 6 quads
            var indices = new ushort[]
            {
                // Front
                0,
                1,
                2,
                2,
                3,
                0,

                // Right
                1,
                5,
                6,
                6,
                2,
                1,

                // Back
                7,
                6,
                5,
                5,
                4,
                7,

                // Left
                4,
                0,
                3,
                3,
                7,
                4,

                // Bottom
                4,
                5,
                1,
                1,
                0,
                4,

                // Top
                3,
                2,
                6,
                6,
                7,
                3,
            };

            Mesh cubeMesh = new Graphics.ThreeDee.Mesh("CubeMesh", indices);
            cubeMesh.VertexFormat = VertexData_Pos_UV_Normal.Descriptor;
            IntPtr vertMem = cubeMesh.AllocateVertices(8);
            unsafe
            {
                VertexData_Pos_UV_Normal* vertexData = (VertexData_Pos_UV_Normal*)vertMem;
                vertexData[0].Position = new Vector3(-1, -1, 1);
                vertexData[1].Position = new Vector3(1, -1, 1);
                vertexData[2].Position = new Vector3(1, 1, 1);
                vertexData[3].Position = new Vector3(-1, 1, 1);

                vertexData[4].Position = new Vector3(-1, -1, -1);
                vertexData[5].Position = new Vector3(1, -1, -1);
                vertexData[6].Position = new Vector3(1, 1, -1);
                vertexData[7].Position = new Vector3(-1, 1, -1);

                vertexData[0].UV = new Vector2(0, 0);
                vertexData[1].UV = new Vector2(0.5f, 0);
                vertexData[2].UV = new Vector2(0.5f, 0.5f);
                vertexData[3].UV = new Vector2(0, 0.5f);

                vertexData[4].UV = new Vector2(0, 0);
                vertexData[5].UV = new Vector2(0.5f, 0);
                vertexData[6].UV = new Vector2(0.5f, 0.5f);
                vertexData[7].UV = new Vector2(0, 0.5f);

                vertexData[0].Normal = new Vector3(0, 0, 1);
                vertexData[1].Normal = new Vector3(0, 0, 1);
                vertexData[2].Normal = new Vector3(0, 0, 1);
                vertexData[3].Normal = new Vector3(0, 0, 1);
                vertexData[4].Normal = new Vector3(0, 0, -1);
                vertexData[5].Normal = new Vector3(0, 0, -1);
                vertexData[6].Normal = new Vector3(0, 0, -1);
                vertexData[7].Normal = new Vector3(0, 0, -1);
            }

            _unitCubeEntity = MeshEntity.CreateFromMesh(cubeMesh);
        }

        return _unitCubeEntity;
    }
}