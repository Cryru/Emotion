using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Graphics.Data;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;

namespace Emotion.Game.SpriteStack
{
    public class SpriteStackTexture : TextureAsset
    {
        private byte[] _bgraPixels;

        public SpriteStackModel GetSpriteStackModel(Vector2 frameSize)
        {
            Vector2 size = Texture.Size;
            var frameWidth = (int) frameSize.X;
            var frameHeight = (int) frameSize.Y;
            var frameCount = (int) (size.X / frameWidth);

            var frames = new SpriteStackFrame[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var frame = new SpriteStackFrame(frameWidth, frameHeight);
                frames[i] = frame;
            }

            // Get the non 0 alpha pixels from the image.
            Span<Color> pixels = MemoryMarshal.Cast<byte, Color>(new Span<byte>(_bgraPixels));
            for (var i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                if (c.A == 0) continue;
                int x = i % (int) size.X;
                int y = i / (int) size.X;
                int frame = x / frameWidth;

                int frameStart = frame * frameWidth;
                int frameX = x - frameStart;

                frames[frame].SetPixel(frameX + y * frameWidth, new Color(c.B, c.G, c.R, c.A));
            }

            // Convert the pixels to voxel cubes.
            Vector3 center = new Vector3(frameWidth / 2, frameHeight / 2, frameCount / 2) * 2;
            for (var i = 0; i < frameCount; i++)
            {
                SpriteStackFrame frame = frames[i];
                frame.Vertices = new VertexData[frame.FilledPixels * 36];
                frame.VertexToPixelIdx = new int[frame.FilledPixels];
                var pixelCount = 0;

                for (var j = 0; j < frame.Pixels.Length; j++)
                {
                    Color color = frame.Pixels[j];
                    if (color.A == 0) continue;

                    frame.VertexToPixelIdx[pixelCount] = j;
                    Span<VertexData> thisPixel = new Span<VertexData>(frame.Vertices).Slice(pixelCount * 36, 36);

                    // Darken the lower layers.
                    //int halfway = (frames.Length - 1) / 2;
                    //if (i < halfway)
                    //{
                    //    int layerFromTop = halfway - i;
                    //    int darkening = Maths.Clamp(3 * layerFromTop, 0, 255);
                    //    int r = Maths.Clamp(color.R - darkening, 0, 255);
                    //    int g = Maths.Clamp(color.G - darkening, 0, 255);
                    //    int b = Maths.Clamp(color.B - darkening, 0, 255);

                    //    color.R = (byte)r;
                    //    color.G = (byte)g;
                    //    color.B = (byte)b;
                    //}
          
                    uint c = color.ToUint();
                    for (var ic = 0; ic < thisPixel.Length; ic++)
                    {
                        thisPixel[ic].Color = c;
                        thisPixel[ic].Tid = -1;
                    }

                    // Cube - 36 vertices, 12 triangles, 6 sides
                    // This can be optimized with indices of course, but this is just a proof of concept for now.
                    thisPixel[0].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[1].Vertex = new Vector3(-1.0f, -1.0f, 1.0f);
                    thisPixel[2].Vertex = new Vector3(-1.0f, 1.0f, 1.0f);
                    thisPixel[3].Vertex = new Vector3(1.0f, 1.0f, -1.0f);
                    thisPixel[4].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[5].Vertex = new Vector3(-1.0f, 1.0f, -1.0f);
                    thisPixel[6].Vertex = new Vector3(1.0f, -1.0f, 1.0f);
                    thisPixel[7].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[8].Vertex = new Vector3(1.0f, -1.0f, -1.0f);
                    thisPixel[9].Vertex = new Vector3(1.0f, 1.0f, -1.0f);
                    thisPixel[10].Vertex = new Vector3(1.0f, -1.0f, -1.0f);
                    thisPixel[11].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[12].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[13].Vertex = new Vector3(-1.0f, 1.0f, 1.0f);
                    thisPixel[14].Vertex = new Vector3(-1.0f, 1.0f, -1.0f);
                    thisPixel[15].Vertex = new Vector3(1.0f, -1.0f, 1.0f);
                    thisPixel[16].Vertex = new Vector3(-1.0f, -1.0f, 1.0f);
                    thisPixel[17].Vertex = new Vector3(-1.0f, -1.0f, -1.0f);
                    thisPixel[18].Vertex = new Vector3(-1.0f, 1.0f, 1.0f);
                    thisPixel[19].Vertex = new Vector3(-1.0f, -1.0f, 1.0f);
                    thisPixel[20].Vertex = new Vector3(1.0f, -1.0f, 1.0f);
                    thisPixel[21].Vertex = new Vector3(1.0f, 1.0f, 1.0f);
                    thisPixel[22].Vertex = new Vector3(1.0f, -1.0f, -1.0f);
                    thisPixel[23].Vertex = new Vector3(1.0f, 1.0f, -1.0f);
                    thisPixel[24].Vertex = new Vector3(1.0f, -1.0f, -1.0f);
                    thisPixel[25].Vertex = new Vector3(1.0f, 1.0f, 1.0f);
                    thisPixel[26].Vertex = new Vector3(1.0f, -1.0f, 1.0f);
                    thisPixel[27].Vertex = new Vector3(1.0f, 1.0f, 1.0f);
                    thisPixel[28].Vertex = new Vector3(1.0f, 1.0f, -1.0f);
                    thisPixel[29].Vertex = new Vector3(-1.0f, 1.0f, -1.0f);
                    thisPixel[30].Vertex = new Vector3(1.0f, 1.0f, 1.0f);
                    thisPixel[31].Vertex = new Vector3(-1.0f, 1.0f, -1.0f);
                    thisPixel[32].Vertex = new Vector3(-1.0f, 1.0f, 1.0f);
                    thisPixel[33].Vertex = new Vector3(1.0f, 1.0f, 1.0f);
                    thisPixel[34].Vertex = new Vector3(-1.0f, 1.0f, 1.0f);
                    thisPixel[35].Vertex = new Vector3(1.0f, -1.0f, 1.0f);

                    int x = j % frameWidth;
                    int y = j / frameWidth;

                    var pixelPositionMatrix = Matrix4x4.CreateTranslation(x * 2 - center.X, y * 2 - center.Y, i * 2 - center.Z);
                    for (var iv = 0; iv < thisPixel.Length; iv++)
                    {
                        thisPixel[iv].Vertex = Vector3.Transform(thisPixel[iv].Vertex, pixelPositionMatrix);
                    }

                    pixelCount++;
                }
            }

            return new SpriteStackModel(frames, frameWidth, frameHeight);
        }

        protected override void UploadTexture(Vector2 size, byte[] bgraPixels, bool flipped)
        {
            _bgraPixels = bgraPixels;
            base.UploadTexture(size, bgraPixels, flipped);
        }
    }
}
