#region Using

using System.Collections;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Testing;

#endregion

namespace Tests.EngineTests;

/// <summary>
/// Tests concerning the scaling of the rendered content, when in full scale mode.
/// </summary>
public class ResizeTests : ProxyRenderTestingScene
{
    private static Vector2 _backgroundSize = new Vector2(50);

    [Test]
    public IEnumerator TestFullScale()
    {
        CameraBase oldCamera = Engine.Renderer.Camera;
        Vector2 oldHostSize = Engine.Host.Size;
        Vector2 oldRenderSize = Engine.Configuration.RenderSize;

        Engine.Configuration.RenderSize = new Vector2(320, 180);
        Engine.Renderer.Camera = new PixelArtCamera(Vector3.Zero);
        Engine.Host.Size = new Vector2(600, 600);

        ToRender = (composer) =>
        {
            composer.SetUseViewMatrix(true);
            composer.RenderSprite(new Vector3(_backgroundSize * -1, 0), _backgroundSize, Color.Black);
            composer.RenderSprite(new Vector3(0, 0, 0), _backgroundSize, Color.CornflowerBlue);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            composer.RenderSprite(new Vector3(_backgroundSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ResizeTests), nameof(TestFullScale));

        Engine.Renderer.Camera = oldCamera;
        Engine.Host.Size = oldHostSize;
        Engine.Configuration.RenderSize = oldRenderSize;
    }

    [Test]
    public IEnumerator TestFullScaleInteger()
    {
        CameraBase oldCamera = Engine.Renderer.Camera;
        Vector2 oldHostSize = Engine.Host.Size;
        Vector2 oldRenderSize = Engine.Configuration.RenderSize;

        Engine.Renderer.Camera = new PixelArtCamera(Vector3.Zero);
        Engine.Host.Size = new Vector2(640, 360);

        ToRender = (composer) =>
        {
            composer.RenderSprite(new Vector3(_backgroundSize * -1, 0), _backgroundSize, Color.Black);
            composer.RenderSprite(new Vector3(0, 0, 0), _backgroundSize, Color.CornflowerBlue);
            composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
            composer.RenderSprite(new Vector3(_backgroundSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);
        };

        yield return new TestWaiterRunLoops(1);
        VerifyScreenshot(nameof(ResizeTests), nameof(TestFullScaleInteger));

        Engine.Renderer.Camera = oldCamera;
        Engine.Host.Size = oldHostSize;
        Engine.Configuration.RenderSize = oldRenderSize;
    }
}