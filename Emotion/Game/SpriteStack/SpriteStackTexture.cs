#region Using

using System;
using System.Numerics;
using Emotion.Graphics.Data;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Game.SpriteStack
{
    public class SpriteStackTexture : TextureAsset
    {
        private byte[] _textureData;
        private PixelFormat _textureDataFormat;

        public SpriteStackModel GetSpriteStackModel(Vector2 frameSize)
        {
            Vector2 size = Texture.Size;
            var frameWidth = (int)frameSize.X;
            var frameHeight = (int)frameSize.Y;
            var frameCount = (int)(size.X / frameWidth);

            var frames = new SpriteStackFrame[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var frame = new SpriteStackFrame(frameWidth, frameHeight);
                frames[i] = frame;
            }

            // Get the non 0 alpha pixels from the image.
            for (var i = 0; i < _textureData.Length; i += 4)
            {
                var c = new Color(_textureData[i], _textureData[i + 1], _textureData[i + 2], _textureData[i + 3], _textureDataFormat);
                if (c.A == 0) continue;

                int pixelIdx = i / 4;
                int x = pixelIdx % (int)size.X;
                int y = pixelIdx / (int)size.X;
                int frame = x / frameWidth;

                int frameStart = frame * frameWidth;
                int frameX = x - frameStart;

                frames[frame].SetPixel(frameX + y * frameWidth, c);
            }

            // Convert the pixels to voxel cubes.
            Vector3 center = new Vector3(frameWidth / 2, frameHeight / 2, frameCount / 2) * 2;
            for (var fIdx = 0; fIdx < frameCount; fIdx++)
            {
                SpriteStackFrame frame = frames[fIdx];
                frame.Vertices = new VertexData[frame.FilledPixels * 8];
                frame.Indices = new ushort[frame.FilledPixels * 6 * 6]; // 6 sides, one quad is 6 indices
                var pixelCount = 0;

                for (var pIdx = 0; pIdx < frame.Pixels.Length; pIdx++)
                {
                    Color color = frame.Pixels[pIdx];
                    if (color.A == 0) continue;

                    // Check if the pixel is not occluded by another.
                    var pixelTwoDeeCoord = new Vector2(pIdx % frameWidth, pIdx / frameWidth);
                    var anyTransp = false;
                    foreach (Vector2 dir in Maths.CardinalDirections2D)
                    {
                        Vector2 otherPixel = pixelTwoDeeCoord + dir;
                        if (otherPixel.X < 0 || otherPixel.Y < 0 || otherPixel.X > frameWidth || otherPixel.Y > frameHeight)
                        {
                            anyTransp = true;
                            break;
                        }

                        var oneDeeCoord = (int)(otherPixel.Y * frameWidth + otherPixel.X);
                        Color otherC = frame.Pixels[oneDeeCoord];
                        if (otherC.A < 255)
                        {
                            anyTransp = true;
                            break;
                        }
                    }

                    // Check between frames (layers).
                    if (!anyTransp)
                    {
                        if (fIdx == 0 || fIdx == frameCount - 1)
                        {
                            anyTransp = true;
                        }
                        else
                        {
                            SpriteStackFrame prevFrame = frames[fIdx - 1];
                            SpriteStackFrame nextFrame = frames[fIdx + 1];
                            Color cBelow = prevFrame.Pixels[pIdx];
                            Color cAbove = nextFrame.Pixels[pIdx];
                            if (cBelow.A < 255 || cAbove.A < 255) anyTransp = true;
                        }

                        if (!anyTransp) continue;
                    }

                    color = color.SetAlpha(135);

                    Span<VertexData> thisPixel = new Span<VertexData>(frame.Vertices).Slice(pixelCount * 8, 8);
                    Span<ushort> thisPixelIndices = new Span<ushort>(frame.Indices).Slice(pixelCount * 6 * 6, 6 * 6);

                    // Darken the lower layers.
                    //int halfway = (frames.Length - 1) / 2;
                    //if (i < halfway)
                    //{
                    //    int layerFromTop = halfway - i;
                    //    int darkening = Emotion.Utility.Maths.Clamp(3 * layerFromTop, 0, 255);
                    //    int r = Emotion.Utility.Maths.Clamp(color.R - darkening, 0, 255);
                    //    int g = Emotion.Utility.Maths.Clamp(color.G - darkening, 0, 255);
                    //    int b = Emotion.Utility.Maths.Clamp(color.B - darkening, 0, 255);

                    //    color.R = (byte)r;
                    //    color.G = (byte)g;
                    //    color.B = (byte)b;
                    //}

                    uint c = color.ToUint();
                    for (var ic = 0; ic < thisPixel.Length; ic++)
                    {
                        thisPixel[ic].Color = c;
                    }

                    // Cube - 36 vertices, 12 triangles, 6 sides
                    // Cube - 36 indices, 8 vertices, 6 quads
                    thisPixel[0].Vertex = new Vector3(-1, -1, 1);
                    thisPixel[1].Vertex = new Vector3(1, -1, 1);
                    thisPixel[2].Vertex = new Vector3(1, 1, 1);
                    thisPixel[3].Vertex = new Vector3(-1, 1, 1);

                    thisPixel[4].Vertex = new Vector3(-1, -1, -1);
                    thisPixel[5].Vertex = new Vector3(1, -1, -1);
                    thisPixel[6].Vertex = new Vector3(1, 1, -1);
                    thisPixel[7].Vertex = new Vector3(-1, 1, -1);

                    // Front
                    thisPixelIndices[00] = 0;
                    thisPixelIndices[01] = 1;
                    thisPixelIndices[02] = 2;
                    thisPixelIndices[03] = 2;
                    thisPixelIndices[04] = 3;
                    thisPixelIndices[05] = 0;

                    // Right
                    thisPixelIndices[06] = 1;
                    thisPixelIndices[07] = 5;
                    thisPixelIndices[08] = 6;
                    thisPixelIndices[09] = 6;
                    thisPixelIndices[10] = 2;
                    thisPixelIndices[11] = 1;

                    // Back
                    thisPixelIndices[12] = 7;
                    thisPixelIndices[13] = 6;
                    thisPixelIndices[14] = 5;
                    thisPixelIndices[15] = 5;
                    thisPixelIndices[16] = 4;
                    thisPixelIndices[17] = 7;

                    // Left
                    thisPixelIndices[18] = 4;
                    thisPixelIndices[19] = 0;
                    thisPixelIndices[20] = 3;
                    thisPixelIndices[21] = 3;
                    thisPixelIndices[22] = 7;
                    thisPixelIndices[23] = 4;

                    // Bottom
                    thisPixelIndices[24] = 4;
                    thisPixelIndices[25] = 5;
                    thisPixelIndices[26] = 1;
                    thisPixelIndices[27] = 1;
                    thisPixelIndices[28] = 0;
                    thisPixelIndices[29] = 4;

                    // Top
                    thisPixelIndices[30] = 3;
                    thisPixelIndices[31] = 2;
                    thisPixelIndices[32] = 6;
                    thisPixelIndices[33] = 6;
                    thisPixelIndices[34] = 7;
                    thisPixelIndices[35] = 3;

                    // Add frame vertex index offset as the whole frame will be drawn together.
                    int indexOffset = pixelCount * 8;
                    for (var k = 0; k < thisPixelIndices.Length; k++)
                    {
                        thisPixelIndices[k] = (ushort)(thisPixelIndices[k] + indexOffset);
                    }

                    int x = pIdx % frameWidth;
                    int y = pIdx / frameWidth;

                    var pixelPositionMatrix = Matrix4x4.CreateTranslation(x * 2 - center.X, y * 2 - center.Y, fIdx * 2 - center.Z);
                    for (var iv = 0; iv < thisPixel.Length; iv++)
                    {
                        thisPixel[iv].Vertex = Vector3.Transform(thisPixel[iv].Vertex, pixelPositionMatrix);
                    }

                    pixelCount++;
                }
            }

            return new SpriteStackModel(frames, frameWidth, frameHeight);
        }

        protected override void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat format)
        {
            _textureData = pixels;
            _textureDataFormat = format;
            base.UploadTexture(size, pixels, flipped, format);
        }
    }
}