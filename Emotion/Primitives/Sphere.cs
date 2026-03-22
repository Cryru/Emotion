#nullable enable

#region Using

using Emotion.Game.World.ThreeDee;
using Emotion.Graphics.Data;

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
        Vector3 closestPoint = Vector3.Clamp(Origin, cube.Origin - cube.HalfExtents, cube.Origin + cube.HalfExtents);
        float distanceSquared = Vector3.DistanceSquared(Origin, closestPoint);
        return distanceSquared <= Radius * Radius;
    }

    public static MeshEntity GetEntity()
    {
        // todo: unit sphere

        int resolution = 100; // Resolution of the sphere (higher means more detailed)
        int vertexCount = (resolution + 1) * (resolution + 1);
        int indexCount = 6 * resolution * resolution;

        VertexDataAllocation alloc = VertexDataAllocation.Allocate(VertexData_Pos_UV_Normal.Format, vertexCount);
        float step = MathF.PI * 2 / resolution;

        // Generating vertices and UVs
        for (int lat = 0; lat <= resolution; lat++)
        {
            for (int lon = 0; lon <= resolution; lon++)
            {
                int index = lat * (resolution + 1) + lon;
                float latAngle = MathF.PI / 2 - lat * step;
                float lonAngle = lon * step;

                float x = MathF.Cos(latAngle) * MathF.Cos(lonAngle);
                float y = MathF.Sin(latAngle);
                float z = MathF.Cos(latAngle) * MathF.Sin(lonAngle);
                alloc.SetVertexPositionAtIndex(index, new Vector3(x, y, z));
                alloc.SetUVAtIndex(0, index, new Vector2((float) lon / resolution, (float) lat / resolution));
                alloc.SetNormalAtIndex(index, Vector3.Normalize(new Vector3(x, y, z)));
            }
        }

        ushort[] indices = new ushort[indexCount];
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

        return new MeshEntity([new Mesh(alloc, indices, MeshMaterial.DefaultMaterialTwoSided, "Sphere")], "Sphere");
    }
}