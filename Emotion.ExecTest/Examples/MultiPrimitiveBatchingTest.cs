#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Scenography;
using Emotion.Utility;

#endregion

namespace Emotion.ExecTest.Examples
{
    /// <summary>
    /// More of a test/proof of concept than example. These are how different primitives types are batched together within
    /// Emotion.
    /// </summary>
    public class MultiPrimitiveBatchingTest : IScene
    {
        private List<Action<RenderComposer, Vector3>> _variants = new List<Action<RenderComposer, Vector3>>
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
                var poly = new List<Vector3>
                {
                    loc + new Vector3(19.4f, 5.4f, 0),
                    loc + new Vector3(70.9f, 5.4f, 0),
                    loc + new Vector3(45.1f, 66, 0)
                };

                for (var i = 0; i < 10; i++)
                {
                    c.RenderVertices(poly, Color.Blue);

                    // Offset vertices for the next draw.
                    for (var j = 0; j < poly.Count; j++)
                    {
                        poly[j] += new Vector3(55, 0, 0);
                    }
                }
            }
        };

        private const int DRAWS = 4;
        private const int FRAMES_HOLD = 60;
        private static int[] _currentVariants = new int[DRAWS];

        private int _frameCounter = FRAMES_HOLD;

        public void Load()
        {
        }

        public void Update()
        {
        }

        public void Draw(RenderComposer composer)
        {
            _frameCounter++;
            if (_frameCounter > FRAMES_HOLD)
            {
                // Generate new variants every FRAMES_HOLD frames.
                _frameCounter = 0;
                for (var i = 0; i < DRAWS; i++)
                {
                    _currentVariants[i] = Helpers.GenerateRandomNumber(0, _variants.Count - 1);
                }
            }

            composer.SetUseViewMatrix(false);
            var brush = new Vector3();
            for (var i = 0; i < DRAWS; i++)
            {
                _variants[_currentVariants[i]](composer, brush);
                brush.Y += 100;
            }

            composer.SetUseViewMatrix(true);
        }

        public void Unload()
        {
        }
    }
}