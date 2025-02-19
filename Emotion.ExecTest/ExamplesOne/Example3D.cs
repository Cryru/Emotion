#nullable enable

using Emotion;
using Emotion.Common;
using Emotion.Graphics.Camera;
using Emotion.Scenography;
using Emotion.Utility;
using Emotion.Utility.Noise;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using Emotion.WIPUpdates.ThreeDee;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.ExamplesOne;

public class Example3D : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        var cam = new Camera3D(new Vector3(800, 800, 2000));
        cam.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam;

        Map = new GameMap();

        TerrainMeshGrid terrain = new TerrainMeshGrid(new Vector2(50), 9);

        var simplex = new SimplexNoise();
        var simplex2 = new SimplexNoise();
        for (int y = 0; y < terrain.ChunkSize.Y * 21; y++)
        {
            for (int x = 0; x < terrain.ChunkSize.X * 21; x++)
            {
                Vector2 pos = new Vector2(x, y);

                float sample1 = simplex.Sample2D(pos / new Vector2(10)) * 150;
                float sample2 = simplex2.Sample2D(pos) * 50;

                terrain.ExpandingSetAt(pos, (sample1 + sample2) / 2f);
            }
        }
        Map.TerrainGrid = terrain;

        var testObj = new MapObjectMesh("Test/creatures/rabbit2/rabbit2_rabbitskin2_white.gltf");
        //var testObj = new MapObjectMesh("Test/creatures/peacockmount/peacockmount_body_blue.gltf");
        testObj.Z = 50;
        testObj.Size3D = new Vector3(300, 300, 300);
        testObj.SetAnimation("Stand (ID 0 variation 2)");
        //testObj.RotationDeg = new Vector3(0, 0, 90);
        Map.AddObject(testObj);

        yield break;
    }
}
