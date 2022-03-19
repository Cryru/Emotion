#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Game.Tiled;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Test;
using Emotion.Test.Helpers;
using Emotion.Utility;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    /// <summary>
    /// Tests concerning the rendering and not the composer itself.
    /// </summary>
    [Test]
    public class RenderTests
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

                // Generate the locations at which an invalidation will occur.
                var invalidationIdx = new List<int>();
                for (var i = 0; i < 10; i++)
                {
                    invalidationIdx.Add(Helpers.GenerateRandomNumber(100, count - 100));
                }

                while (elements < count)
                {
                    if (invalidationIdx.IndexOf(elements) != -1)
                    {
                        Engine.Log.Info($"Invalidation of batch at element - {elements}", TestRunnerLogger.TestRunnerSrc);
                        composer.FlushRenderStream();
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

        /// <summary>
        /// Tests batching of different kinds of primitives in the render stream.
        /// Quads, Sequential Triangles, Triangle Fan
        /// RenderSprite, RenderVertices, RenderCircle
        /// </summary>
        [Test]
        public void RenderStreamMultiPrimitiveBatching()
        {
            var drawVariants = new List<Action<RenderComposer, Vector3>>
            {
                // Quads
                (c, loc) =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        c.RenderSprite(loc + new Vector3(i * 25, 0, 0), new Vector2(20, 20), Color.Red);
                    }
                },
                // Sequential triangles
                (c, loc) =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        c.RenderCircle(loc + new Vector3(i * 50, 0, 0), 20, Color.Green);
                    }
                },
                // Triangle fan
                (c, loc) =>
                {
                    Vector2 vec2Loc = loc.ToVec2();
                    var poly = new List<Vector2>
                    {
                        vec2Loc + new Vector2(19.4f, 5.4f),
                        vec2Loc + new Vector2(70.9f, 5.4f),
                        vec2Loc + new Vector2(45.1f, 66)
                    };

                    for (var i = 0; i < 10; i++)
                    {
                        c.RenderVertices(poly, Color.Blue);

                        // Offset vertices for the next draw.
                        for (var j = 0; j < poly.Count; j++)
                        {
                            poly[j] += new Vector2(55, 0);
                        }
                    }
                }
            };

            // Draw combinations of variants multiple times.
            var variantsTest = new[]
            {
                new[] {0, 1, 2},
                new[] {2, 1, 0},
                new[] {0, 2, 1},
                new[] {2, 0, 1}
            };
            const int drawsPerVariant = 10;

            Runner.ExecuteAsLoop(_ =>
            {
                for (var v = 0; v < variantsTest.Length; v++)
                {
                    int[] variant = variantsTest[v];

                    for (var c = 0; c < drawsPerVariant; c++)
                    {
                        RenderComposer composer = Engine.Renderer.StartFrame();
                        composer.SetUseViewMatrix(false);
                        var brush = new Vector3();
                        for (var i = 0; i < variant.Length; i++)
                        {
                            drawVariants[variant[i]](composer, brush);
                            brush.Y += 100;
                        }

                        composer.SetUseViewMatrix(true);
                        Engine.Renderer.EndFrame();

                        // Verify only on the last run of the variant.
                        if (c == drawsPerVariant - 1) Runner.VerifyScreenshot(ResultDb.RenderComposerMultiBatchVariant + v);
                    }
                }
            }).WaitOne();
        }

        /// <summary>
        /// Tests the basic text drawing functionality of the composer.
        /// </summary>
        [Test]
        public void RenderText()
        {
            var asset = Engine.AssetLoader.Get<FontAsset>("Fonts/1980XX.ttf");

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderString(new Vector3(10, 10, 0), Color.Red, "The quick brown fox jumps over the lazy dog.\n123456789!@#$%^&*(0", asset.GetAtlas(20));
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.ComposerRenderText);
            }).WaitOne();
        }

        /// <summary>
        /// Tests the rendering of RichText. This also tests batch caching.
        /// </summary>
        [Test]
        public void RenderRichText()
        {
            var asset = Engine.AssetLoader.Get<FontAsset>("Fonts/1980XX.ttf");
            var testRich = new RichText(new Vector3(20, 20, 0), new Vector2(100, 100), asset.GetAtlas(20));
            testRich.SetText("The quick brown fox jumps over the <color=255-0-0>lazy</> dog.\n123456789!@#$%^&*(0");

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.Render(testRich);
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.RenderRichText);
            }).WaitOne();
        }

        /// <summary>
        /// Tests the ordering of items drawn by the composer by drawing at different Z coordinates and how the result overlaps.
        /// </summary>
        [Test]
        public void RenderDepthTest()
        {
            var asset = Engine.AssetLoader.Get<TextureAsset>("Images/logoAlpha.png");
            var fontAsset = Engine.AssetLoader.Get<FontAsset>("Fonts/1980XX.ttf");
            const int maxY = 5 * 49;

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

                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 50, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 100, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 150, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 200, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 250, 100, 0), new Vector2(100, 100), Color.White, asset.Texture);
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X - 300, 100, 1), new Vector2(100, 100), Color.White, asset.Texture);

                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 0, 1), Color.Red, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 10, 2), Color.Green, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderString(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 20, 1), Color.Blue, "This is test text", fontAsset.GetAtlas(20));
                composer.RenderSprite(new Vector3(Engine.Renderer.CurrentTarget.Size.X / 2 - 100, 0, 0), new Vector2(200, 100), Color.Black);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.ComposerRender);
            }).WaitOne();
        }

        [Test]
        public void TilemapTest()
        {
            var tileMap = new TileMap<TransformRenderable>("Tilemap/DeepForest.tmx");

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.Render(tileMap);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.TilemapRender);
            }).WaitOne();

            tileMap.Reset("");
        }

        /// <summary>
        /// Tests reading the FrameBuffer's depth texture, and using it to copy the depth over to the draw buffer.
        /// </summary>
        [Test]
        public void TestDepthFromOtherFrameBuffer()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                FrameBuffer testBuffer = new FrameBuffer(Engine.Renderer.DrawBuffer.Size).WithColor().WithDepth(true);
                var shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/DepthTest.xml");

                RenderComposer composer = Engine.Renderer.StartFrame();
                composer.RenderTo(testBuffer);
                composer.ClearFrameBuffer();
                composer.RenderSprite(new Vector3(0, 0, 10), new Vector2(100, 100), Color.Green);
                composer.RenderTo(null);

                composer.SetUseViewMatrix(false);
                composer.SetShader(shader.Shader);
                shader.Shader.SetUniformInt("depthTexture", 1);
                Texture.EnsureBound(testBuffer.DepthTexture.Pointer, 1);
                composer.RenderSprite(new Vector3(0, 0, 0), testBuffer.Texture.Size, Color.White, testBuffer.Texture);
                composer.SetShader();
                composer.SetUseViewMatrix(true);

                composer.RenderSprite(new Vector3(20, 20, 15), new Vector2(100, 100), Color.Blue);
                composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(100, 100), Color.Red);

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.TestDepthFromOtherFrameBuffer);

                testBuffer.Dispose();
            }).WaitOne();
        }

        /// <summary>
        /// Tests whether the UVs are mapped properly and flipping functionality.
        /// </summary>
        [Test]
        public void TestUVMapping()
        {
            var asset = Engine.AssetLoader.Get<TextureAsset>("Images/logoAsymmetric.png");

            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.RenderSprite(new Vector3(10, 10, 0), new Vector2(100, 100), Color.White, asset.Texture, null, true);
                composer.RenderSprite(new Vector3(100, 10, 0), new Vector2(100, 100), Color.White, asset.Texture, null, false, true);
                composer.RenderSprite(new Vector3(200, 10, 0), new Vector2(100, 100), Color.White, asset.Texture, null, true, true);

                composer.FlushRenderStream();
                asset.Texture.Tile = false;

                composer.RenderSprite(new Vector3(10, 100, 0), new Vector2(100, 100), Color.White, asset.Texture, null, true);
                composer.RenderSprite(new Vector3(100, 100, 0), new Vector2(100, 100), Color.White, asset.Texture, null, false, true);
                composer.RenderSprite(new Vector3(200, 100, 0), new Vector2(100, 100), Color.White, asset.Texture, null, true, true);

                composer.RenderSprite(new Vector3(10, 200, 0), new Vector2(100, 100), Color.White, asset.Texture, new Rectangle(0, 0, 50, 50), true);
                composer.RenderSprite(new Vector3(100, 200, 0), new Vector2(100, 100), Color.White, asset.Texture, new Rectangle(0, 0, 50, 50), false, true);
                composer.RenderSprite(new Vector3(200, 200, 0), new Vector2(100, 100), Color.White, asset.Texture, new Rectangle(0, 0, 50, 50), true, true);

                composer.FlushRenderStream();
                asset.Texture.Tile = true;
                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.TestUVMapping);
            }).WaitOne();
        }

        /// <summary>
        /// Framebuffer resizing
        /// </summary>
        [Test]
        public void FramebufferResizing()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                FrameBuffer testBuffer = new FrameBuffer(new Vector2(1000, 1000)).WithColor();

                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.RenderToAndClear(testBuffer);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(1000, 1000), Color.Red);
                composer.RenderTo(null);

                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(100, 100), Color.White, testBuffer.Texture);
                testBuffer.Resize(new Vector2(500, 500), true);

                composer.RenderToAndClear(testBuffer);
                composer.RenderSprite(new Vector3(0, 0, 0), new Vector2(500, 500), Color.Green);
                composer.RenderTo(null);

                composer.RenderFrameBuffer(testBuffer, new Vector2(100, 100), new Vector3(100, 0, 0));

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.FramebufferResizing);

                testBuffer.Dispose();
            }).WaitOne();
        }

        /// <summary>
        /// Test drawing of lines.
        /// </summary>
        [Test]
        public void LineDrawing()
        {
            Runner.ExecuteAsLoop(_ =>
            {
                RenderComposer composer = Engine.Renderer.StartFrame();

                composer.PushModelMatrix(Matrix4x4.CreateTranslation(200, 200, 0));

                // Diagonal Lines
                composer.RenderLine(new Vector3(10, 10, 0), new Vector3(50, 50, 0),Color.White);
                composer.RenderLine(new Vector3(50, 50, 0), new Vector3(100, 100, 0), Color.White, 2);
                composer.RenderLine(new Vector3(100, 100, 0), new Vector3(150, 150, 0), Color.White, 3);

                // Horizontal lines
                // 100 pixels long
                composer.RenderLine(new Vector3(100, 10, 0), new Vector3(200, 10, 0), Color.White);
                composer.RenderLine(new Vector3(100, 20, 0), new Vector3(200, 20, 0), Color.White, 2);
                composer.RenderLine(new Vector3(100, 30, 0), new Vector3(200, 30, 0), Color.White,3);
                composer.RenderLine(new Vector3(100, 40, 0), new Vector3(200, 40, 0), Color.White,4);

                // Vertical
                // 100 pixels high
                composer.RenderLine(new Vector3(10, 100, 0), new Vector3(10, 200, 0), Color.White);
                composer.RenderLine(new Vector3(20, 100, 0), new Vector3(20, 200, 0), Color.White,2);
                composer.RenderLine(new Vector3(30, 100, 0), new Vector3(30, 200, 0), Color.White,3);
                composer.RenderLine(new Vector3(40, 100, 0), new Vector3(40, 200, 0), Color.White,4);

                // Test lines in all 2d directions. Z would only be visible with a 3d. camera.
                composer.RenderArrow(new Vector3(0, 0, 0), new Vector3(10, 0, 0), Color.Red);
                composer.RenderArrow(new Vector3(0, 0, 0), new Vector3(0, 10, 0), Color.Green);
                composer.RenderArrow(new Vector3(0, 0, 0), new Vector3(0, 0, 10), Color.Blue);

                // Lines must be at least 1 pixel thick.
                composer.RenderArrow(new Vector3(10, 0, 0), new Vector3(100, 0, 0), Color.Red, 0.1f);
                composer.RenderArrow(new Vector3(0, 10, 0), new Vector3(0, 100, 0), Color.Green, 0.1f);
                composer.RenderArrow(new Vector3(0, 0, 10), new Vector3(0, 0, 100), Color.Blue, 0.1f);

                composer.PopModelMatrix();
                Engine.Renderer.EndFrame();

                Runner.VerifyScreenshot(ResultDb.LineDrawing);
            }).WaitOne();
        }
    }
}