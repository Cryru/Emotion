#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Command;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Test;
using Emotion.Test.Helpers;
using Emotion.Utility;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    [Test]
    public class ComposerUnitTests
    {
        /// <summary>
        /// Tests whether the composer will switch deal with drawing more than the maximum number of sprites that can be batched.
        /// </summary>
        [Test]
        public void RenderComposerSpriteLimit()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                const int count = ushort.MaxValue * 2;
                const int size = 1;

                var y = 0;
                var x = 0;
                var elements = 0;

                while (elements < count)
                {
                    var c = new Color(elements, 255 - elements, elements < ushort.MaxValue ? 255 : 0);

                    composer.RenderSprite(new Vector3(x * size, y * size, 0), new Vector2(size, size), c);
                    x++;
                    elements++;

                    if (x * size < Engine.Renderer.CurrentTarget.Size.X) continue;
                    y++;
                    x = 0;
                }

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.RenderComposerSpriteLimitTest);
            }).WaitOne();
        }

        /// <summary>
        /// Tests whether the composer handles batching properly when the current batch is invalidated.
        /// </summary>
        [Test]
        public void RenderComposerRandomInvalidation()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                const int count = ushort.MaxValue * 2;
                const int size = 1;

                var y = 0;
                var x = 0;
                var elements = 0;

                // Generate the locations at which an invaldiation will occur.
                List<int> invalidationIdx = new List<int>();
                for (int i = 0; i < 10; i++)
                {
                    invalidationIdx.Add(Helpers.GenerateRandomNumber(100, count - 100));
                }

                while (elements < count)
                {
                    if(invalidationIdx.IndexOf(elements) != -1)
                    {
                        Runner.Log.Info($"Invalidation of batch at element - {elements}", CustomMSource.TestRunner);
                        composer.InvalidateStateBatches();
                    }

                    var c = new Color(elements, 255 - elements, elements < ushort.MaxValue ? 255 : 0);

                    composer.RenderSprite(new Vector3(x * size, y * size, 0), new Vector2(size, size), c);
                    x++;
                    elements++;

                    if (x * size < Engine.Renderer.CurrentTarget.Size.X) continue;
                    y++;
                    x = 0;
                }

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.RenderComposerSpriteLimitTest);
            }).WaitOne();
        }

        public class TestCustomBatch : QuadBatch
        {
            public override void Process(RenderComposer c)
            {
                for (int i = 0; i < BatchedTexturables.Count; i++)
                {
                    BatchedTexturables[i].Color = (new Color(BatchedTexturables[i].Color) * Color.Magenta).ToUint();
                }
                base.Process(c);
            }
        }

        /// <summary>
        /// Tests whether setting a custom batch works.
        /// </summary>
        [Test]
        public void RenderComposerCustomBatch()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                const int count = ushort.MaxValue * 2;
                const int size = 1;

                var y = 0;
                var x = 0;
                var elements = 0;

                const int changeBatchAt = 10000;

                while (elements < count)
                {
                    var c = new Color(elements, 255 - elements, elements < ushort.MaxValue ? 255 : 0);

                    if (elements == changeBatchAt) composer.SetSpriteBatchType<TestCustomBatch>();

                    composer.RenderSprite(new Vector3(x * size, y * size, 0), new Vector2(size, size), c);
                    x++;
                    elements++;

                    if (x * size < Engine.Renderer.CurrentTarget.Size.X) continue;
                    y++;
                    x = 0;
                }

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.RenderComposerCustomBatch);
            }).WaitOne();
        }

        /// <summary>
        /// Tests the rendering of sub composers under the main composer.
        /// </summary>
        [Test]
        public void RenderSubComposer()
        {
            var asset = Engine.AssetLoader.Get<TextureAsset>("Images/logoAlpha.png");
            var fontAsset = Engine.AssetLoader.Get<FontAsset>("Fonts/1980XX.ttf");
            const int maxY = 5 * 49;

            var subComposer = new RenderComposer();

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                // Set a background so invalid alpha can be seen
                composer.RenderSprite(new Vector3(0, 0, -1), Engine.Renderer.CurrentTarget.Size, Color.CornflowerBlue);

                for (var i = 0; i < 50; i++)
                {
                    composer.RenderSprite(new Vector3(5 * i, 5 * i, i), new Vector2(100, 100), Color.White, asset.Texture);
                }

                for (var i = 0; i < 50; i++)
                {
                    composer.RenderSprite(new Vector3(5 * i, maxY - 5 * i, i), new Vector2(100, 100), Color.White, asset.Texture);
                }

                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X, 0, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 50, 0, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 100, 0, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 150, 0, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 200, 0, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 250, 0, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 300, 0, 0), new Vector2(100, 100), Color.White, asset.Texture);

                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 50, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 100, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 150, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 200, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 250, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                subComposer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 300, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);

                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 0, 1), Color.Red, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 10, 2), Color.Green, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 20, 1), Color.Blue, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 0, 0), new Vector2(200, 100), Color.Black);

                composer.AddSubComposer(subComposer);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.ComposerRender);

                Assert.True(subComposer.Processed);
            }).WaitOne();
        }
    }
}