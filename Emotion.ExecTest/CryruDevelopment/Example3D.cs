#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Game.World.Terrain;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics;
using Emotion.Standard.Noise;
using Emotion.Game.World;
using System.Collections;
using System.Numerics;
using Emotion.Game.World.Components;
using Emotion.Game.Systems.UI;
using Emotion.Primitives;
using Emotion.Game.Systems.UI2;

namespace Emotion.ExecTest.CryruDevelopment;

public class Example3D : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        //var cam = new Camera3D(new Vector3(210, 500, 400));
        //cam.LookAtPoint(new Vector3(500, 500, 0));
        //Engine.Renderer.Camera = cam;

        var map = new GameMap();

        TerrainMeshGrid terrain = new TerrainMeshGrid(new Vector2(5), 32);
        terrain.CreateEmptyChunksInArea(new Vector2(-50), new Vector2(100));

        var simplex = new SimplexNoise(1337);
        var simplex2 = new SimplexNoise(777);
        for (int y = 0; y < terrain.ChunkSize.Y * 10; y++)
        {
            for (int x = 0; x < terrain.ChunkSize.X * 10; x++)
            {
                Vector2 pos = new Vector2(x, y);

                float sample1 = simplex.Sample2D(pos / new Vector2(10)) * 20;
                float sample2 = simplex2.Sample2D(pos) * 5;

                terrain.ExpandingSetAt(pos, (sample1 + sample2) / 2f);
            }
        }

        map.AddGrid(terrain);

        yield return SetCurrentMap(map);

        //AddObject("Test/humanmalewarriorlight/humanmalewarriorlight_skin01.gltf", new Vector3(100, 100, 0));
        //AddObject("Test/male/humanmale.gltf", new Vector3(300, 100, 0));
        //AddObject("Test/malehd/humanmale_hd.gltf", new Vector3(500, 100, 0));
        //AddObject("Test/pika/pika_4.gltf", new Vector3(100, 300, 0));
        //AddObject("Test/creatures/rabbit2/rabbit2_rabbitskin2_white.gltf", new Vector3(300, 300, 0));
        //AddObject("Test/pkmnlady/zisu.gltf", new Vector3(100, 500, 0));
        //AddObject("Test/creatures/peacockmount/peacockmount_body_blue.gltf", new Vector3(500, 500, 0));

        //AddObject("Test/holiday-kit/bench.fbx", new Vector3(100, 100, 0));
        //AddObject("Test/linkOG/link.dae", new Vector3(300, 100, 0));
        //AddObject("Test/old ref/bear.fbx", new Vector3(500, 100, 0));
        //AddObject("Test/astroboy/astroBoy_walk_Maya.dae", new Vector3(100, 300, 0));

        yield break;
    }

    public void AddObject(string file, Vector3 pos)
    {
        var testObj = new GameObject();
        testObj.Scale3D = new Vector3(1);
        testObj.Position3D = pos;

        MeshComponent meshComponent = testObj.AddComponent(new MeshComponent(file));
        meshComponent.SetAnimation("Stand [4]");
        meshComponent.SetAnimation("Stand (ID 0 variation 0)");

        Map.AddObject(testObj);
    }
}
