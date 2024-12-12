#nullable enable

using Emotion;
using Emotion.Scenography;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Work;
using System.Collections;
using System.Numerics;

namespace Emotion.ExecTest.ExamplesOne;

public class Example3D : SceneWithMap
{
    protected override IEnumerator InternalLoadSceneRoutineAsync()
    {
        Map = new GameMap();

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
