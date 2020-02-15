#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
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
                        Engine.Log.Info($"Invalidation of batch at element - {elements}", CustomMSource.TestRunner);
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

        public class TestCustomBatch : VertexDataSpriteBatch
        {
            public override unsafe void Render(RenderComposer c)
            {
                var data = new Span<VertexData>((void*) _batchedVertices, _mappedTo);

                for (var i = 0; i < data.Length; i++)
                {
                    data[i].Color = (new Color(data[i].Color) * Color.Magenta).ToUint();
                }

                base.Render(c);
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

                    if (elements == changeBatchAt) composer.SetSpriteBatch(new TestCustomBatch());

                    composer.RenderSprite(new Vector3(x * size, y * size, 0), new Vector2(size, size), c);
                    x++;
                    elements++;

                    if (x * size < Engine.Renderer.CurrentTarget.Size.X) continue;
                    y++;
                    x = 0;
                }

                composer.SetDefaultSpriteBatch();

                Engine.Renderer.EndFrame();
                Runner.VerifyScreenshot(ResultDb.RenderComposerCustomBatch);
            }).WaitOne();
        }
    }
}