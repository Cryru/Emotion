#nullable enable

using Emotion;
using Emotion.Common;
using Emotion.ExecTest.CryruDevelopment.Tools;
using Emotion.Game.PremadeControllers.WorldOfWarcraft;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Utility;
using Emotion.Utility.Noise;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;
using System;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.CryruDevelopment;

public class Example3D : SceneWithMap
{
    private MapObjectMesh _person;
    private WoWMovementController _movementController;

    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        //var cam = new Camera3D(new Vector3(210, 500, 400));
        //cam.LookAtPoint(new Vector3(500, 500, 0));
        //Engine.Renderer.Camera = cam;

        Map = new GameMap();

        //TerrainMeshGrid terrain = new TerrainMeshGrid(new Vector2(5), 32);
        //terrain.InitEmptyChunksInArea(new Vector2(-50), new Vector2(100));

        //var simplex = new SimplexNoise(new Random(1337));
        //var simplex2 = new SimplexNoise(new Random(777));
        //for (int y = 0; y < terrain.ChunkSize.Y * 10; y++)
        //{
        //    for (int x = 0; x < terrain.ChunkSize.X * 10; x++)
        //    {
        //        Vector2 pos = new Vector2(x, y);

        //        float sample1 = simplex.Sample2D(pos / new Vector2(10)) * 20;
        //        float sample2 = simplex2.Sample2D(pos) * 5;

        //        terrain.ExpandingSetAt(pos, (sample1 + sample2) / 2f);
        //    }
        //}

        //Map.TerrainGrid = terrain;

        yield return TerrainImport.GetConverted("Test/cryru/map/maps/azeroth/adt_32_48.obj", Map);

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

        var testObj = new MapObjectMesh("Test/malehdtextured/humanmale_hd.gltf");
        testObj.Position = new Vector3(385, 130, 28);
        Map.AddObject(testObj);
        _person = testObj;

        _movementController = new WoWMovementController();
        _movementController.Attach();
        _movementController.SetCharacter(_person,
            "Stand (ID 0 variation 0)",
            "Run (ID 5 variation 0)",
            "Walkbackwards (ID 13 variation 0)",
            "bone_Waist"
        );

        yield break;
    }

    public void AddObject(string file, Vector3 pos)
    {
        var testObj = new MapObjectMesh(file);
        testObj.Size3D = new Vector3(1);
        testObj.Position = pos;
        testObj.SetAnimation("Stand [4]");
        testObj.SetAnimation("Stand (ID 0 variation 0)");
        Map.AddObject(testObj);
    }

    public override void UpdateScene(float dt)
    {
        base.UpdateScene(dt);
        _movementController?.Update(dt);
    }

    public override void RenderScene(RenderComposer c)
    {
        base.RenderScene(c);
    }
}
