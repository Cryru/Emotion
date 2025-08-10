#nullable enable

using Emotion.Core;
using Emotion.Core.Systems.IO;
using Emotion.Game.World;
using Emotion.Game.World.ThreeDee;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Testing;
using System.Collections;
using System.Numerics;

namespace Tests.EngineTests;

public class Render3DTests : TestingScene
{
    private GameMap _gameMap = null!;

    public override IEnumerator LoadSceneRoutineAsync()
    {
        GameObject obj = GameObject.NewMeshObject("WoWModels/rabbit2/rabbit2_rabbitskin2_white.gltf");
        obj.Scale3D = new Vector3(100);

        _gameMap = new GameMap();
        _gameMap.AddObject(obj);
        yield return _gameMap.InitRoutine();

        yield return base.LoadSceneRoutineAsync();
    }

    protected override void TestDraw(Renderer c)
    {
        _gameMap.Render(c);
    }

    protected override void TestUpdate()
    {
        _gameMap.Update(16);
    }

    public IEnumerator ScreenshotPointFromAllSides(Vector3 point, string funcName)
    {
        var cam = Engine.Renderer.Camera;

        cam.Position = point + new Vector3(0, 0, 100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, 0, -100);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(-100, 0, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, 100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);

        cam.Position = point + new Vector3(0, -100, 0);
        cam.LookAtPoint(Vector3.Zero);

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(Render3DTests), funcName);
    }

    [Test]
    public IEnumerator WorldWith2DCamera()
    {
        Camera3D cam = new Camera3D(Vector3.Zero);
        Engine.Renderer.Camera = cam;
        yield return new TestWaiterRunLoops(1);

        yield return ScreenshotPointFromAllSides(Vector3.Zero, nameof(WorldWith2DCamera));
    }
}
