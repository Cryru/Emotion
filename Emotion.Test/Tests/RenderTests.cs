#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.Test.Results;

#endregion

namespace Emotion.Test.Tests
{
    /// <summary>
    /// Tests concerning the rendering and not the composer itself.
    /// </summary>
    [Test]
    public class RenderTests
    {
        [Test]
        public void TilemapTest()
        {
            var tileMap = new TileMap(Vector3.Zero, Vector2.Zero, "Tilemap/DeepForest.tmx", "Tilemap/");

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                tileMap.Render(composer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.TilemapRender);
            }).WaitOne();

            tileMap.Reset("", "");
        }
    }
}