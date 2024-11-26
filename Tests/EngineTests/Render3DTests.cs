using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Testing;
using Emotion.UI;
using Emotion.Utility;
using Emotion.WIPUpdates.One;
using Emotion.WIPUpdates.One.Camera;
using Emotion.WIPUpdates.One.Work;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tests.EngineTests;

[Test]
public class Render3DTests : TestingScene
{
    private MeshEntity _testEntity;
    private GameMap _gameMap;

    public override IEnumerator LoadSceneRoutineAsync()
    {
        AssetHandle<MeshAsset> meshEntityRef = Engine.AssetLoader.ONE_Get<MeshAsset>("WoWModels/rabbit2/rabbit2_rabbitskin2_white.gltf");
        yield return meshEntityRef;

        _testEntity = meshEntityRef.Asset.Entity;

        MapObjectMesh obj = new MapObjectMesh(_testEntity);
        obj.Size3D = new Vector3(100);

        _gameMap = new GameMap();
        _gameMap.AddObject(obj);
        yield return _gameMap.LoadRoutine();

        yield return base.LoadSceneRoutineAsync();
    }

    protected override void TestDraw(RenderComposer c)
    {
        _gameMap.Render(c);
    }

    protected override void TestUpdate()
    {
        _gameMap.Update(16);
    }

    public IEnumerator ScreenshotPointFromAllSides(Vector3 point)
    {
        var cam = Engine.Renderer.Camera;

        cam.Position = point + new Vector3(0, 0, 100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();

        cam.Position = point + new Vector3(0, 0, -100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();

        cam.Position = point + new Vector3(100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();

        cam.Position = point + new Vector3(-100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();

        cam.Position = point + new Vector3(0, 100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();

        cam.Position = point + new Vector3(0, -100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        yield return VerifyScreenshot();
    }

    [DebugTest]
    [Test]
    public IEnumerator WorldWith2DCamera()
    {
        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(1);

        yield return ScreenshotPointFromAllSides(Vector3.Zero);
    }
}
