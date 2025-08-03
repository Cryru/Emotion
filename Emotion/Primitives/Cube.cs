#nullable enable

#region Using

using System.Runtime.CompilerServices;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Primitives;

public enum CubeFace
{
    PositiveX,
    NegativeX,
    PositiveY,
    NegativeY,
    PositiveZ,
    NegativeZ
}

/// <summary>
/// Represents an axis aligned cube.
/// </summary>
public struct Cube
{
    public static Cube Empty { get; } = new Cube();

    public bool IsEmpty
    {
        get => Origin == Vector3.Zero && HalfExtents == Vector3.Zero;
    }

    public Vector3 Origin;
    public Vector3 HalfExtents;

    public Cube(Vector3 center, Vector3 halfExtent)
    {
        Origin = center;
        HalfExtents = halfExtent;
    }

    public static Cube FromCenterAndSize(Vector3 origin, Vector3 size)
    {
        return new Cube(origin, size / 2f);
    }

    public static Cube FromMinAndMax(Vector3 min, Vector3 max)
    {
        Vector3 origin = (min + max) / 2f;
        Vector3 halfExtents = (max - min) / 2f;
        return new Cube(origin, halfExtents);
    }

    public (Vector3, Vector3) GetMinMax()
    {
        return (Origin - HalfExtents, Origin + HalfExtents);
    }

    public Cube Union(Cube other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;

        (Vector3 minA, Vector3 maxA) = GetMinMax();
        (Vector3 minB, Vector3 maxB) = other.GetMinMax();

        Vector3 unionMin = Vector3.Min(minA, minB);
        Vector3 unionMax = Vector3.Max(maxA, maxB);

        Vector3 newOrigin = (unionMin + unionMax) / 2;
        Vector3 newHalfExtents = (unionMax - unionMin) / 2;

        return new Cube { Origin = newOrigin, HalfExtents = newHalfExtents };
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

    public bool Intersects(Cube cube)
    {
        (Vector3 minA, Vector3 maxA) = GetMinMax();
        (Vector3 minB, Vector3 maxB) = cube.GetMinMax();

        bool intersectsX = minA.X <= maxB.X && maxA.X >= minB.X;
        bool intersectsY = minA.Y <= maxB.Y && maxA.Y >= minB.Y;
        bool intersectsZ = minA.Z <= maxB.Z && maxA.Z >= minB.Z;
        return intersectsX && intersectsY && intersectsZ;
    }

    public bool ContainsInclusive(Cube cube)
    {
        (Vector3 minA, Vector3 maxA) = GetMinMax();
        (Vector3 minB, Vector3 maxB) = cube.GetMinMax();

        return minB.X >= minA.X && maxB.X <= maxA.X &&
               minB.Y >= minA.Y && maxB.Y <= maxA.Y &&
               minB.Z >= minA.Z && maxB.Z <= maxA.Z;
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

    public bool IntersectWithVertices<TIndex>(TIndex[] indices, int indexCount, VertexDataAllocation vertices, out Vector3 collisionPoint, out Vector3 normal, out int triangleIndex)
        where TIndex : INumber<TIndex>
    {
        collisionPoint = Vector3.Zero;
        normal = Vector3.Zero;
        triangleIndex = -1;

        if (!vertices.Format.Built) return false;
        if (!vertices.Format.HasPosition) return false;

        var closestDistance = float.MaxValue;
        var intersectionFound = false;

        foreach (Triangle triangle in vertices.ForEachTriangle(indices, indexCount))
        {
            if (Intersects(triangle))
                return true;
        }
        

        //for (var i = 0; i < indexCount; i += 3)
        //{
        //    TIndex idx1 = indices[i];
        //    TIndex idx2 = indices[i + 1];
        //    TIndex idx3 = indices[i + 2];

        //    Triangle triangle = vertices.GetTriangleAtIndices(idx1, idx2, idx3);
        //    Vector3 triangleNormal = triangle.Normal;

        //    //if (!IntersectWithTriangle(triangle.A, triangle.B, triangle.C, triangleNormal, out float t)) continue;

        //    //if (t < closestDistance)
        //    //{
        //    //    closestDistance = t;
        //    //    normal = triangleNormal;
        //    //    triangleIndex = i;
        //    //    intersectionFound = true;
        //    //}
        //}

        //if (intersectionFound) collisionPoint = Start + Direction * closestDistance;

        return intersectionFound;
    }

    #region Cube-Triangle Intersection

    public bool Intersects(Triangle tri)
    {
        Vector3 cubeOrigin = Origin;
        Vector3 cubeHalfSize = HalfExtents;

        // Move triangle to box's local space
        Vector3 tv0 = tri.A - cubeOrigin;
        Vector3 tv1 = tri.B - cubeOrigin;
        Vector3 tv2 = tri.C - cubeOrigin;

        // Compute triangle edges
        Vector3 e0 = tv1 - tv0;
        Vector3 e1 = tv2 - tv1;
        Vector3 e2 = tv0 - tv2;

        float[] min = { 0, 0, 0 }, max = { 0, 0, 0 };
        Vector3[] verts = { tv0, tv1, tv2 };

        // 1) Test overlap in the box's x‐, y‐, and z‐axes
        for (int i = 0; i < 3; i++)
        {
            float v0i = verts[0][i];
            float v1i = verts[1][i];
            float v2i = verts[2][i];
            min[i] = Math.Min(v0i, Math.Min(v1i, v2i));
            max[i] = Math.Max(v0i, Math.Max(v1i, v2i));
            if (min[i] > cubeHalfSize[i] || max[i] < -cubeHalfSize[i])
                return false;
        }

        // 2) Test the triangle normal axis
        Vector3 normal = Vector3.Cross(e0, e1);
        if (!PlaneBoxOverlap(normal, tv0, cubeHalfSize))
            return false;

        // 3) Test the 9 axes given by cross products of edges and box axes
        Vector3[] edges = { e0, e1, e2 };
        for (int i = 0; i < 3; i++)                 // box axes
            for (int j = 0; j < 3; j++)             // tri edges
            {
                Vector3 axis = Vector3.Cross(edges[j], GetUnitAxis(i));
                if (axis.LengthSquared() < 1e-8f)   // skip near‐zero axes
                    continue;

                // Project triangle onto axis
                float minT = float.MaxValue, maxT = float.MinValue;
                for (int k = 0; k < 3; k++)
                {
                    float proj = Vector3.Dot(verts[k], axis);
                    minT = Math.Min(minT, proj);
                    maxT = Math.Max(maxT, proj);
                }

                // Project box onto axis (radius = sum of extents * |axis component|)
                float r =
                    cubeHalfSize[(i + 1) % 3] * Math.Abs(axis[(i + 2) % 3]) +
                    cubeHalfSize[(i + 2) % 3] * Math.Abs(axis[(i + 1) % 3]);

                if (minT > r || maxT < -r)
                    return false;
            }

        return true;
    }

    //  test if triangle plane overlaps the box
    private static bool PlaneBoxOverlap(Vector3 normal, Vector3 vert, Vector3 maxBox)
    {
        // Compute box extents in direction of plane normal
        Vector3 v = new Vector3(
            normal.X > 0 ? maxBox.X : -maxBox.X,
            normal.Y > 0 ? maxBox.Y : -maxBox.Y,
            normal.Z > 0 ? maxBox.Z : -maxBox.Z);

        float d1 = Vector3.Dot(normal, vert + v);
        float d2 = Vector3.Dot(normal, vert - v);

        // If both d1 and d2 have the same sign, no overlap
        return d1 * d2 <= 0;
    }

    private static Vector3 GetUnitAxis(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(1, 0, 0);
            case 1: return new Vector3(0, 1, 0);
            default: return new Vector3(0, 0, 1);
        }
    }

    #endregion

    #region Render Outline

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

    public void RenderOutline(Renderer c, Color? color = null, float thickness = 0.1f)
    {
        Span<Vector3> vertices = stackalloc Vector3[8];
        GetVertices(vertices);
        for (var i = 0; i < _outlineEdges.Length; i++)
        {
            int[] edge = _outlineEdges[i];
            c.RenderLine(vertices[edge[0]], vertices[edge[1]], color ?? Color.White, thickness);
        }
    }

    #endregion

    #region Unit Entity

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
            vertexData[0].Vertex = new Vector3(-0.5f, 0.5f, 0.5f);
            vertexData[1].Vertex = new Vector3(0.5f, 0.5f, 0.5f);
            vertexData[2].Vertex = new Vector3(0.5f, -0.5f, 0.5f);
            vertexData[3].Vertex = new Vector3(-0.5f, -0.5f, 0.5f);

            vertexData[4].Vertex = new Vector3(-0.5f, 0.5f, -0.5f);
            vertexData[5].Vertex = new Vector3(0.5f, 0.5f, -0.5f);
            vertexData[6].Vertex = new Vector3(0.5f, -0.5f, -0.5f);
            vertexData[7].Vertex = new Vector3(-0.5f, -0.5f, -0.5f);

            vertexData[0].UV = new Vector2(0, 0.5f);
            vertexData[1].UV = new Vector2(0.5f, 0.5f);
            vertexData[2].UV = new Vector2(0.5f, 0);
            vertexData[3].UV = new Vector2(0, 0);

            vertexData[4].UV = new Vector2(0, 0.5f);
            vertexData[5].UV = new Vector2(0.5f, 0.5f);
            vertexData[6].UV = new Vector2(0.5f, 0);
            vertexData[7].UV = new Vector2(0, 0);

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

    #endregion
}