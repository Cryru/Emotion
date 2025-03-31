#nullable enable

#region Using

using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Primitives;

public struct Sphere
{
    public Vector3 Origin;
    public float Radius;

    public Sphere(Vector3 origin, float radius)
    {
        Origin = origin;
        Radius = radius;
    }

    public Sphere Transform(Matrix4x4 mat)
    {
        Vector3 transformedOrigin = Vector3.Transform(Origin, mat);

        float scaleX = new Vector3(mat.M11, mat.M21, mat.M31).Length();
        float scaleY = new Vector3(mat.M12, mat.M22, mat.M32).Length();
        float scaleZ = new Vector3(mat.M13, mat.M23, mat.M33).Length();
        float maxScale = Math.Max(Math.Max(scaleX, scaleY), scaleZ);
        float transformedRadius = Radius * maxScale;
        return new Sphere(transformedOrigin, transformedRadius);
    }
    public bool Intersects(Sphere other)
    {
        float distanceSquared = (other.Origin - Origin).LengthSquared();
        float sumOfRadii = Radius + other.Radius;
        return distanceSquared <= sumOfRadii * sumOfRadii;
    }

    public bool Intersects(Cube cube)
    {
        // Find the closest point on the cube to the sphere center
        Vector3 closestPoint = Vector3.Clamp(Origin, cube.Origin - cube.HalfExtents, cube.Origin + cube.HalfExtents);

        // Calculate the distance between the closest point and the sphere center
        float distanceSquared = Vector3.DistanceSquared(Origin, closestPoint);

        // Check if the distance is less than the squared radius of the sphere
        return distanceSquared <= Radius * Radius;
    }

    public MeshEntity GetEntity(int segments = 20, int rings = 10)
    {
        int numVertices = (segments + 1) * (rings + 1);
        int numIndices = segments * rings * 6;

        // Generate indices
        var indices = new ushort[numIndices];
        var i = 0;
        for (var r = 0; r < rings; r++)
        {
            for (var s = 0; s < segments; s++)
            {
                int v1 = r * (segments + 1) + s;
                int v2 = v1 + segments + 1;
                int v3 = v2 + 1;
                int v4 = v1 + 1;

                indices[i++] = (ushort)v1;
                indices[i++] = (ushort)v2;
                indices[i++] = (ushort)v3;

                indices[i++] = (ushort)v3;
                indices[i++] = (ushort)v4;
                indices[i++] = (ushort)v1;
            }
        }

        var sphereMesh = new Mesh("SphereMesh", indices);
        sphereMesh.VertexFormat = VertexData_Pos_UV_Normal.Descriptor;
        sphereMesh.AllocateVertices(numVertices);
        var vertices = sphereMesh.VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal>();

        float phiStep = MathF.PI / rings;
        float thetaStep = 2.0f * MathF.PI / segments;

        // Generate vertices
        for (var r = 0; r <= rings; r++)
        {
            float phi = r * phiStep;

            for (var s = 0; s <= segments; s++)
            {
                float theta = s * thetaStep;

                float x = Radius * MathF.Sin(phi) * MathF.Cos(theta);
                float y = Radius * MathF.Cos(phi);
                float z = Radius * MathF.Sin(phi) * MathF.Sin(theta);

                int index = r * (segments + 1) + s;

                vertices[index].Position = new Vector3(x, y, z);
                vertices[index].UV = new Vector2(s / segments, r / rings);
                vertices[index].Normal = Vector3.Zero;
            }
        }

        return MeshEntity.CreateFromMesh(sphereMesh);
    }

    public static MeshEntity GetEntity()
    {
        const int RESOLUTION = 100; // Resolution of the sphere (higher means more detailed)
        int vertexCount = (RESOLUTION + 1) * (RESOLUTION + 1);
        int indexCount = 6 * RESOLUTION * RESOLUTION;

        // Generate indices, the sphere is made up of quads.
        ushort[] indices = new ushort[indexCount];
        int triIndex = 0;
        for (int lat = 0; lat < RESOLUTION; lat++)
        {
            for (int lon = 0; lon < RESOLUTION; lon++)
            {
                int current = lat * (RESOLUTION + 1) + lon;
                int next = current + RESOLUTION + 1;

                indices[triIndex++] = (ushort)current;
                indices[triIndex++] = (ushort)next;
                indices[triIndex++] = (ushort)(current + 1);

                indices[triIndex++] = (ushort)(current + 1);
                indices[triIndex++] = (ushort)next;
                indices[triIndex++] = (ushort)(next + 1);
            }
        }

        // Generate vertices
        Mesh sphereMesh = new Mesh("Sphere", indices);
        sphereMesh.VertexFormat = VertexData_Pos_UV_Normal.Descriptor;
        sphereMesh.AllocateVertices(vertexCount);
        var vertices = sphereMesh.VertexMemory.GetAsSpan<VertexData_Pos_UV_Normal>();

        float step = MathF.PI * 2 / RESOLUTION;
        for (int lat = 0; lat <= RESOLUTION; lat++)
        {
            for (int lon = 0; lon <= RESOLUTION; lon++)
            {
                int index = lat * (RESOLUTION + 1) + lon;
                float latAngle = MathF.PI / 2 - lat * step;
                float lonAngle = lon * step;

                float x = MathF.Cos(latAngle) * MathF.Cos(lonAngle);
                float y = MathF.Sin(latAngle);
                float z = MathF.Cos(latAngle) * MathF.Sin(lonAngle);

                vertices[index].Position = new Vector3(x, y, z);
                vertices[index].UV = new Vector2((float)lon / RESOLUTION, (float)lat / RESOLUTION);
                vertices[index].Normal = Vector3.Normalize(new Vector3(x, y, z)); // todo
            }
        }

        return MeshEntity.CreateFromMesh(sphereMesh);
    }
}