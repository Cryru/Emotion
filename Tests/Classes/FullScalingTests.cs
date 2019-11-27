#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Primitives;
using Emotion.Test;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    /// <summary>
    /// Tests concerning the scaling of the rendered content, when in full scale mode.
    /// </summary>
    [Test("FullScale", true)]
    public class FullScalingTests
    {
        [Test]
        public void TestFullScale()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                Engine.Renderer.Camera = new PixelArtCamera(Vector3.Zero);
                Engine.Host.Window.Size = new Vector2(960, 540);
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.SetUseViewMatrix(true);
                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize * -1, 0), Engine.Configuration.RenderSize, Color.Black);
                composer.RenderSprite(new Vector3(0, 0, 0), Engine.Configuration.RenderSize, Color.CornflowerBlue);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

                Engine.Renderer.EndFrame();

                // Force the screen buffer to be screenshot.
                Engine.Renderer.ScreenBuffer.Bind();
                Runner.VerifyScreenshot(ResultDb.TestFullScale);
                
            }).WaitOne();
        }

        [Test]
        public void TestFullScaleInteger()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                Engine.Renderer.Camera = new PixelArtCamera(Vector3.Zero);
                Engine.Host.Window.Size = new Vector2(1280, 720);
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize * -1, 0), Engine.Configuration.RenderSize, Color.Black);
                composer.RenderSprite(new Vector3(0, 0, 0), Engine.Configuration.RenderSize, Color.CornflowerBlue);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

                Engine.Renderer.EndFrame();

                // Force the screen buffer to be screenshot.
                Engine.Renderer.ScreenBuffer.Bind();
                Runner.VerifyScreenshot(ResultDb.TestFullScaleInteger);

            }).WaitOne();
        }
    }
}