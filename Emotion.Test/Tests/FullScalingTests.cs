#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Test.Results;

#endregion

namespace Emotion.Test.Tests
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
                Engine.Host.Window.Size = new Vector2(960, 540);
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize * -1, 0), Engine.Configuration.RenderSize, Color.Black);
                composer.RenderSprite(new Vector3(0, 0, 0), Engine.Configuration.RenderSize, Color.CornflowerBlue);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

                Engine.Renderer.EndFrame();

                // Force the screen buffer to be screenshot.
                composer.Reset();
                composer.RenderTo(Engine.Renderer.ScreenBuffer);
                composer.Process();
                composer.Execute();
                Engine.Renderer.ScreenBuffer.Bind();
                Runner.VerifyScreenshot(ResultDb.TestFullScale);
            }).WaitOne();
        }

        [Test]
        public void TestFullScaleInteger()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                Engine.Host.Window.Size = new Vector2(1280, 720);
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize * -1, 0), Engine.Configuration.RenderSize, Color.Black);
                composer.RenderSprite(new Vector3(0, 0, 0), Engine.Configuration.RenderSize, Color.CornflowerBlue);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(10, 10), Color.Red);
                composer.RenderSprite(new Vector3(Engine.Configuration.RenderSize - new Vector2(10, 10), 0), new Vector2(10, 10), Color.Red);

                Engine.Renderer.EndFrame();

                // Force the screen buffer to be screenshot.
                composer.Reset();
                composer.RenderTo(Engine.Renderer.ScreenBuffer);
                composer.Process();
                composer.Execute();
                Engine.Renderer.ScreenBuffer.Bind();
                Runner.VerifyScreenshot(ResultDb.TestFullScaleInteger);
            }).WaitOne();
        }
    }
}