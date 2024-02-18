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

    public static MeshEntity GetEntity()
    {
        int resolution = 100; // Resolution of the sphere (higher means more detailed)
        int vertexCount = (resolution + 1) * (resolution + 1);
        int indexCount = 6 * resolution * resolution;

        VertexData[] vertexData = new VertexData[vertexCount];
        VertexDataMesh3DExtra[] extraData = new VertexDataMesh3DExtra[vertexCount];
        ushort[] indices = new ushort[indexCount];

        for (var ic = 0; ic < vertexData.Length; ic++)
        {
            vertexData[ic].Color = Color.WhiteUint;
        }

        float step = MathF.PI * 2 / resolution;

        // Generating vertices and UVs
        for (int lat = 0; lat <= resolution; lat++)
        {
            for (int lon = 0; lon <= resolution; lon++)
            {
                int index = lat * (resolution + 1) + lon;
                float latAngle = MathF.PI / 2 - lat * step; // Latitude
                float lonAngle = lon * step; // Longitude

                // Sphere vertex position
                float x = MathF.Cos(latAngle) * MathF.Cos(lonAngle);
                float y = MathF.Sin(latAngle);
                float z = MathF.Cos(latAngle) * MathF.Sin(lonAngle);
                vertexData[index].Vertex = new Vector3(x, y, z);

                // UV coordinates
                vertexData[index].UV = new Vector2((float) lon / resolution, (float) lat / resolution);

                // Normal is just the normalized position
                extraData[index].Normal = Vector3.Normalize(new Vector3(x, y, z));
            }
        }

        // Generating indices for triangles
        int triIndex = 0;
        for (int lat = 0; lat < resolution; lat++)
        {
            for (int lon = 0; lon < resolution; lon++)
            {
                int current = lat * (resolution + 1) + lon;
                int next = current + resolution + 1;

                indices[triIndex++] = (ushort) current;
                indices[triIndex++] = (ushort) next;
                indices[triIndex++] = (ushort) (current + 1);

                indices[triIndex++] = (ushort) (current + 1);
                indices[triIndex++] = (ushort) next;
                indices[triIndex++] = (ushort) (next + 1);
            }
        }

        // Create the sphere entity
        return new MeshEntity
        {
            Name = "Sphere",
            BackFaceCulling = true,
            Scale = 1f,
            Meshes = new[]
            {
                new Mesh(vertexData, extraData, indices)
            }
        };
    }
}