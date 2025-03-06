#nullable enable

using Emotion.Common;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.Utility;
using Emotion.WIPUpdates.Grids;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;
using System;
using System.Collections;
using System.IO;
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
        Vector2 tileSize = new Vector2(4.16665649f);

        TerrainMeshGrid terrain = new TerrainMeshGrid(tileSize, (terrainSize / tileSize).Ceiling().X);
        terrain.InitEmptyChunksInArea(Vector2.Zero, terrainSize / tileSize);
        Engine.CoroutineManagerAsync.StartCoroutine(FillMapRoutine(terrain, tileSize, terrainMesh));

        TextureAsset? tex = Engine.AssetLoader.Get<TextureAsset>("Test/cryru/map/maps/azeroth/tex_32_48.png");
        tex.Texture.Smooth = true;
        terrain.TerrainMeshMaterial.DiffuseTexture = tex.Texture;

        var rootFolder = AssetLoader.GetDirectoryName(path);
        var objectData = Engine.AssetLoader.Get<TextAsset>("Test/cryru/map/maps/azeroth/adt_32_48_ModelPlacementInformation.csv");
        string[] lines = objectData.Content.Split("\n");
        Engine.CoroutineManagerAsync.StartCoroutine(FillMapObjects(lines, rootFolder, min, terrainEntity, map));

        map.TerrainGrid = terrain;

        map.AddObject(new MapObjectMesh(terrainEntity)
        {
        });
    }

    private static IEnumerator FillMapRoutine(TerrainMeshGrid terrain, Vector2 resolution, Mesh terrainMesh)
    {
        var verts = terrainMesh.Vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertexPos = verts[i].Vertex;
            Vector2 vPos2 = vertexPos.ToVec2();

            Vector2 emotionTilePos = (vPos2 / resolution).Floor();
            terrain.ExpandingSetAt(emotionTilePos, vertexPos.Z);

            //if (i == 145) break;

            yield return null;
        }
        yield break;

        // Interpolate holes
        Vector2[] neighbours = [
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),

            new Vector2(1, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
        ];

        var terrainSize = terrain.GetSize();
        for (int y = 0; y < terrainSize.Y; y++)
        {
            for (int x = 0; x < terrainSize.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                float value = terrain.GetAt(tileCoord);
                if (value != 0) continue;

                float sum = 0;
                int values = 0;

                int kernelSize = 1;
                for (int dy = -kernelSize; dy <= kernelSize; dy++)
                {
                    for (int dx = -kernelSize; dx <= kernelSize; dx++)
                    {
                        Vector2 diff = new Vector2(dx, dy);
                        Vector2 tileToSample = tileCoord + diff;
                        float otherVal = terrain.GetAt(tileToSample);
                        if (otherVal != 0)
                        {
                            sum += otherVal;
                            values++;
                        }
                    }
                }
                if (values == 0) continue;

                float avg = sum / values;
                terrain.SetAt(tileCoord, avg);
            }
        }
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
