#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Game.Animation2D
{
    public class SpriteArrayFrameSource : ISpriteAnimationFrameSource
    {
        [SerializeNonPublicGetSet]
        public Rectangle[]? Frames { get; protected set; }

        // Serialization
        protected SpriteArrayFrameSource()
        {
        }

        public SpriteArrayFrameSource(Texture t)
        {
            GLThread.ExecuteGLThreadAsync(() => { Frames = AutoDetectFrames(t); });
        }

        private static unsafe Rectangle[] AutoDetectFrames(Texture tex)
        {
            var pixels = new byte[(int) (tex.Size.X * tex.Size.Y * 4)];

            fixed (void* p = &pixels[0])
            {
                Texture.EnsureBound(tex.Pointer);
                Gl.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat.Rgba, PixelType.UnsignedByte, new IntPtr(p));
            }

            // Convert to A1 from BGRA8 with an alpha threshold.
            for (int i = 0, w = 0; i < pixels.Length; i += 4, w++)
            {
                if (pixels[i + 3] > 10)
                    pixels[w] = 1;
                else
                    pixels[w] = 0;
            }

            Array.Resize(ref pixels, pixels.Length / 4);

            var boxes = new List<Rectangle>();

            // First pass - identify box start positions.
            for (var y = 0; y < tex.Size.Y; y++)
            {
                for (var x = 0; x < tex.Size.X; x++)
                {
                    byte current = pixels[(int) (y * tex.Size.X + x)];

                    // Check if the current one is filled.
                    if (current != 1) continue;
                    // Start a box.
                    var start = new Vector2(x, y);
                    var size = new Vector2();
                    var width = 0;
                    // Find the next non full. This is the width.
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        for (int xx = x; xx < tex.Size.X; xx++)
                        {
                            byte curLook = pixels[(int) (yy * tex.Size.X + xx)];
                            if (curLook == 0)
                            {
                                if (width > size.X) size.X = width;
                                goto step2;
                            }

                            width++;
                        }

                        if (width > size.X) size.X = width;
                        width = 0;
                    }

                    step2:
                    // Now go down from the start until we find a non-full.
                    var heightLeft = 0;
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * tex.Size.X + x)];
                        if (curLook == 0) break;
                        heightLeft++;
                    }

                    // Now go down from the end until we find a non-full.
                    var heightRight = 0;
                    for (int yy = y; yy < tex.Size.Y; yy++)
                    {
                        byte curLook = pixels[(int) (yy * tex.Size.X + (x + size.X - 1))];
                        if (curLook == 0) break;
                        heightRight++;
                    }

                    size.Y = MathF.Max(heightLeft, heightRight);
                    boxes.Add(new Rectangle(start, size));
                    x += (int) size.X;
                }
            }

            // Combine all boxes.
            var runAgain = true;
            while (runAgain)
            {
                runAgain = false;
                for (int i = boxes.Count - 1; i >= 0; i--)
                {
                    for (var j = 0; j < boxes.Count; j++)
                    {
                        if (i == j) continue;

                        if (!boxes[i].IntersectsInclusive(boxes[j])) continue;
                        boxes[i] = Rectangle.Union(boxes[i], boxes[j]);
                        boxes.RemoveAt(j);
                        runAgain = true;
                        break;
                    }
                }
            }

            return boxes
                .Where(x => x.Width > 1 && x.Height > 1)
                .OrderBy(x => Math.Round((x.Y + x.Height / 2) / 100f))
                .ThenBy(x => Math.Round((x.X + x.Width / 2) / 100f)).ToArray();
        }

        public int GetFrameCount()
        {
            if (Frames == null) return 0;
            return Frames.Length;
        }

        public Rectangle GetFrameUV(int i)
        {
            if (Frames == null) return Rectangle.Empty;
            return Frames[i];
        }
    }
}