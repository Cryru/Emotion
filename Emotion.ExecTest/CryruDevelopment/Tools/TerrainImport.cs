#nullable enable

using Emotion.Common;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.CryruDevelopment.Tools;

public class TerrainImport
{
    public static void GetConverted(string path, GameMap map)
    {
        MeshAsset? terrainChunk = Engine.AssetLoader.Get<MeshAsset>(path);
        MeshEntity? terrainEntity = terrainChunk.Entity;
        Mesh terrainMesh = terrainEntity.Meshes[0];

        terrainEntity.GetBounds(null, out _, out Cube cub);
        Vector3 min = cub.Origin - cub.HalfExtents;

        for (int i = 0; i < terrainMesh.Vertices.Length; i++)
        {
            ref Graphics.Data.VertexData vertex = ref terrainMesh.Vertices[i];
            vertex.Vertex = vertex.Vertex - min;
        }

        terrainEntity.ResetCachedBounds();
        terrainEntity.GetBounds(null, out _, out cub);

        Vector2 terrainSize = (cub.HalfExtents * 2f).ToVec2();
        Vector2 tileSize = new Vector2(2.083332f, 2.0830078f);

        TerrainMeshGrid terrain = new TerrainMeshGrid(tileSize, 19);
        terrain.InitEmptyChunksInArea(Vector2.Zero, terrainSize / tileSize);
        Engine.CoroutineManagerAsync.StartCoroutine(FillMapRoutine(terrain, terrainMesh));

        TextureAsset? tex = Engine.AssetLoader.Get<TextureAsset>("Test/cryru/map/maps/azeroth/tex_32_48.png");
        tex.Texture.Smooth = true;
        terrain.TerrainMeshMaterial.DiffuseTexture = tex.Texture;

        var rootFolder = AssetLoader.GetDirectoryName(path);
        var objectData = Engine.AssetLoader.Get<TextAsset>("Test/cryru/map/maps/azeroth/adt_32_48_ModelPlacementInformation.csv");
        string[] lines = objectData.Content.Split("\n");
        Engine.CoroutineManagerAsync.StartCoroutine(FillMapObjects(lines, rootFolder, min, terrainEntity, map));

        map.TerrainGrid = terrain;

        //map.AddObject(new MapObjectMesh(terrainEntity)
        //{
        //});
    }

    private static IEnumerator FillMapRoutine(TerrainMeshGrid terrain, Mesh terrainMesh)
    {
        Vector2 tiles = terrain.GetSize();
        for (int y = 0; y < tiles.Y; y++)
        {
            for (int x = 0; x < tiles.X; x++)
            {
                var tileCoord = new Vector2(x, y);
                var worldPos = terrain.GetWorldPosOfTile(tileCoord);

                float heightValue = GetHeightAtPosition(terrainMesh, worldPos);
                terrain.SetAt(tileCoord, heightValue);

                yield return null;
            }
        }
    }

    private static float GetHeightAtPosition(Mesh mesh, Vector2 position)
    {
        foreach (Triangle triangle in mesh.ForEachTriangle())
        {
            if (!triangle.IsPoint2DInTriangle(position, 0.1f)) continue;
            return InterpolateZ(triangle, position);
        }

        return 1;
    }

    private static float InterpolateZ(Triangle triag, Vector2 point)
    {
        Vector2 a = new Vector2(triag.A.X, triag.A.Y);
        Vector2 b = new Vector2(triag.B.X, triag.B.Y);
        Vector2 c = new Vector2(triag.C.X, triag.C.Y);

        // Compute barycentric coordinates
        float denominator = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
        float alpha = ((b.Y - c.Y) * (point.X - c.X) + (c.X - b.X) * (point.Y - c.Y)) / denominator;
        float beta = ((c.Y - a.Y) * (point.X - c.X) + (a.X - c.X) * (point.Y - c.Y)) / denominator;
        float gamma = 1.0f - alpha - beta;

        // Interpolate Z value
        return alpha * triag.A.Z + beta * triag.B.Z + gamma * triag.C.Z;
    }

    private static IEnumerator FillMapObjects(string[] lines, string rootFolder, Vector3 min, MeshEntity terrainEntity, GameMap map)
    {
        for (int i = 1; i < lines.Length; i++)
        {
            string[] props = lines[i].Split(";");
            if (props.Length == 1) continue;

            string assetPath = AssetLoader.GetNonRelativePath(rootFolder, AssetLoader.NameToEngineName(props[0]));
            MeshAsset? asset = Engine.AssetLoader.Get<MeshAsset>(assetPath);
            if (asset == null) continue;

            Vector3 position = new Vector3(float.Parse(props[1]), float.Parse(props[3]), float.Parse((props[2])));
            position = position - new Vector3(533.333333333f * 32, 533.333333333f * 32, 0);

            // 8 - scale factor

            float y = position.Y;
            position.X = -(position.X + min.X);
            position.Y = -(position.Y + min.Y);
            position.Z = position.Z - min.Z;

            Vector3 rot = new Vector3();
            {
                Vector4 rotation = new Vector4(
                    float.Parse(props[4]),
                    float.Parse(props[5]),
                    float.Parse(props[6]),
                    0//float.Parse(props[7])
                );

                rot = new Vector3(rotation.X, rotation.Z, -(rotation.Y - 270));
            }

            map.AddObject(new MapObjectMesh(asset.Entity)
            {
                Position = position,
                RotationDeg = rot
            });

            yield return null;
        }
    }
}
