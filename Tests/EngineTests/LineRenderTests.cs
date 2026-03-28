#region Using

using System;
using System.Collections;
using System.Numerics;
using Emotion.Core;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Testing;
using OpenGL;

#endregion

namespace Tests.EngineTests;

public class LineRenderTests : ProxyRenderTestingScene
{
    [Test]
    public IEnumerator LinesThreeDee()
    {
        float[] cameraDistances = { 50f, 120f, 400f, 1000f };
        float[] thicknesses = { 0.5f, 1f, 2f, 4f };

        foreach (float distance in cameraDistances)
        {
            Camera3D cam = new Camera3D(new Vector3(0, 0, -distance));
            cam.LookAtPoint(Vector3.Zero);
            Engine.Renderer.Camera = cam;

            foreach (float thickness in thicknesses)
            {
                ToRender = (composer) =>
                {
                    composer.SetUseViewMatrix(true);

                    Cube cube = Cube.FromCenterAndSize(Vector3.Zero, new Vector3(60, 60, 60));
                    cube.RenderOutline(composer, Color.Cyan, thickness);
                };

                yield return new TestWaiterRunLoops(1);
                yield return VerifyScreenshot(nameof(LineRenderTests), nameof(LinesThreeDee));
            }
        }
    }

    [Test]
    public IEnumerator LinesTwoDee()
    {
        Camera2D cam = new Camera2D(new Vector3(0, 0, 0), 1f);
        Engine.Renderer.Camera = cam;

        float[] zoomLevels = { 0.6f, 1f, 2f, 3f };
        float[] thicknesses = { 1f, 2f, 5f };

        foreach (var zoom in zoomLevels)
        {
            cam.Zoom = zoom;
            cam.RecreateViewMatrix();

            foreach (float thickness in thicknesses)
            {
                ToRender = (composer) =>
                {
                    composer.SetUseViewMatrix(true);
                    composer.RenderRectOutline(new Vector3(20, 20, 0), new Vector2(210, 120), Color.Green, thickness);
                };
            }

            yield return new TestWaiterRunLoops(1);
            yield return VerifyScreenshot(nameof(LineRenderTests), nameof(LinesTwoDee));
        }
    }

    [Test]
    public IEnumerator LinesUI()
    {
        Camera2D cam = new Camera2D(new Vector3(0, 0, 0), 1f);
        Engine.Renderer.Camera = cam;

        float[] thicknesses = { 1f, 2f, 5f };
        float moveAmount = 0.1f;
        int sizeOneMoveTimes = 10;

        foreach (float thickness in thicknesses)
        {
            int moveTimes = thickness == 1f ? sizeOneMoveTimes : 1;
            float offset = 0;
            for (int i = 0; i < moveTimes; i++)
            {
                ToRender = (composer) =>
                {
                    composer.SetUseViewMatrix(false);

                    composer.RenderRectOutline(new Vector3(50, 53 + offset, 0), new Vector2(220, 120), Color.White, thickness);
                    composer.RenderRectOutline(new Vector3(70, 73 + offset, 0), new Vector2(180, 80), Color.PrettyPurple, thickness);

                    composer.StartLineRender();
                    composer.AddLineRender(new Vector3(60, 63 + offset, 0), new Vector3(280, 170, 0), Color.PrettyGreen, thickness);
                    composer.AddLineRender(new Vector3(280, 63 + offset, 0), new Vector3(60, 170, 0), Color.Cyan, thickness);
                    composer.EndLineRender();
                };

                yield return new TestWaiterRunLoops(1);
                yield return VerifyScreenshot(nameof(LineRenderTests), nameof(LinesUI));

                offset += moveAmount;
            }
        }
    }
}