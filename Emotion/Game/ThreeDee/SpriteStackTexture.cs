#region Using

using Emotion.Game.Animation2D;
using Emotion.Graphics.Data;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Game.ThreeDee
{
    public class SpriteStackTexture : TextureAsset
    {
        private byte[] _textureData;
        private PixelFormat _textureDataFormat;

        public MeshEntity GetSpriteStackEntity(Vector2 frameSize)
        {
            Vector2 textureSize = Texture.Size;
            var columns = (int) MathF.Floor(textureSize.X / frameSize.X);
            var rows = (int) MathF.Floor(textureSize.Y / frameSize.Y);

            var frames = new Mesh[columns * rows];

            // Download the pixel data from each frame.
            var frameIdx = 0;
            for (var y = 0; y < columns; y++)
            {
                for (var x = 0; x < rows; x++)
                {
                    // Count the filled pixels
                    int filledPixels = GenerateStackFrame(frameIdx, columns * rows, frameSize, textureSize, null, null, null);

                    // Allocate data for the mesh and run againt to fill.
                    // todo: what if data is outside index range
                    var vertices = new VertexData[filledPixels * 8];
                    var meshData = new VertexDataMesh3DExtra[vertices.Length];
                    var indices = new ushort[filledPixels * 6 * 6]; // 6 sides, one quad is 6 indices
                    GenerateStackFrame(frameIdx, columns * rows, frameSize, textureSize, vertices, meshData, indices);

                    var frameMesh = new Mesh($"SpriteStack Frame {frameIdx} ({Name})", vertices, meshData, indices);
                    frames[frameIdx] = frameMesh;
                    frameIdx++;
                }
            }

            var spriteStackEntity = new MeshEntity
            {
                Name = Name,
                Meshes = frames
            };
            return spriteStackEntity;
        }


        public int GenerateStackFrame(int frameIdx, int totalFrames, Vector2 frameSize, Vector2 textureSize, VertexData[] vertices, VertexDataMesh3DExtra[] meshData, ushort[] indices)
        {
            Rectangle rect = Animation2DHelpers.GetGridFrameBounds(textureSize, frameSize, Vector2.Zero, frameIdx);

            var frameWidth = (int) frameSize.X;
            var frameHeight = (int) frameSize.Y;

            var meshCenter = new Vector2(frameWidth - 1, -frameHeight + 1);

            // Count pixels that will be turned into voxels
            var filledPixelCount = 0;
            for (var imageY = (int) rect.Y; imageY < (int) rect.Y + rect.Height; imageY++)
            {
                for (var imageX = (int) rect.X; imageX < (int) rect.X + rect.Width; imageX++)
                {
                    var pixelDataIndex = (int) (imageY * textureSize.X + imageX);
                    pixelDataIndex *= 4; // Assuming 4 component color.

                    // Count the pixels with alpha > 0
                    var colorData = new Span<byte>(_textureData, pixelDataIndex, 4);
                    var c = new Color(colorData[0], colorData[1], colorData[2], colorData[3], _textureDataFormat);
                    if (c.A == 0) continue;

                    // Count pixels that aren't occluded.
                    var anyTransp = false;
                    foreach (Vector2 dir in Maths.CardinalDirections2D)
                    {
                        Vector2 otherPixel = new Vector2(imageX, imageY) + dir;
                        if (otherPixel.X < 0 || otherPixel.Y < 0 || otherPixel.X > frameWidth || otherPixel.Y > frameHeight)
                        {
                            anyTransp = true;
                            break;
                        }

                        var otherDataIndex = (int) (otherPixel.Y * frameWidth + otherPixel.X);
                        var otherColorData = new Span<byte>(_textureData, otherDataIndex, 4);
                        var otherColor = new Color(otherColorData[0], otherColorData[1], otherColorData[2], otherColorData[3], _textureDataFormat);
                        if (otherColor.A < 255)
                        {
                            anyTransp = true;
                            break;
                        }
                    }

                    // Check for occlusions between frames.
                    Vector2 pixelRelativeOffset = new Vector2(imageX, imageY) - new Vector2(rect.X, rect.Y);
                    if (frameIdx == 0 || frameIdx == totalFrames)
                    {
                        anyTransp = true;
                    }
                    else
                    {
                        Rectangle prevFrameRect = Animation2DHelpers.GetGridFrameBounds(textureSize, frameSize, Vector2.Zero, frameIdx - 1);
                        Rectangle nextFrameRect = Animation2DHelpers.GetGridFrameBounds(textureSize, frameSize, Vector2.Zero, frameIdx + 1);

                        Vector2 prevRectPixel = prevFrameRect.Position + pixelRelativeOffset;
                        Vector2 nextRectPixel = nextFrameRect.Position + pixelRelativeOffset;

                        var prevRectPixelIdx = (int) (prevRectPixel.Y * frameWidth + prevRectPixel.X);
                        var nextRectPixelIdx = (int) (nextRectPixel.Y * frameWidth + nextRectPixel.X);

                        var prevRectPixelData = new Span<byte>(_textureData, prevRectPixelIdx, 4);
                        var prevRectPixelColor = new Color(prevRectPixelData[0], prevRectPixelData[1], prevRectPixelData[2], prevRectPixelData[3], _textureDataFormat);

                        var nextRectPixelData = new Span<byte>(_textureData, nextRectPixelIdx, 4);
                        var nextRectPixelColor = new Color(nextRectPixelData[0], nextRectPixelData[1], nextRectPixelData[2], nextRectPixelData[3], _textureDataFormat);

                        if (prevRectPixelColor.A < 255 || nextRectPixelColor.A < 255) anyTransp = true;
                    }

                    if (!anyTransp) continue;

                    // Add vertex and index data if any.
                    if (vertices != null && indices != null)
                    {
                        Span<VertexData> thisPixel = new Span<VertexData>(vertices).Slice(filledPixelCount * 8, 8);
                        Span<ushort> thisPixelIndices = new Span<ushort>(indices).Slice(filledPixelCount * 6 * 6, 6 * 6);

                        // Darken the lower layers.
                        //int halfway = totalFrames / 2;
                        //if (frameIdx < halfway)
                        //{
                        //	int layerFromTop = halfway - frameIdx;
                        //	int darkening = Maths.Clamp(3 * layerFromTop, 0, 255);
                        //	int r = Maths.Clamp(c.R - darkening, 0, 255);
                        //	int g = Maths.Clamp(c.G - darkening, 0, 255);
                        //	int b = Maths.Clamp(c.B - darkening, 0, 255);

                        //	c.R = (byte) r;
                        //	c.G = (byte) g;
                        //	c.B = (byte) b;
                        //}

                        uint colorUint = c.ToUint();
                        for (var ic = 0; ic < thisPixel.Length; ic++)
                        {
                            thisPixel[ic].Color = colorUint;
                            thisPixel[ic].UV = Vector2.Zero;
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

                        meshData[0].Normal = new Vector3(0, 0, 1);
                        meshData[1].Normal = new Vector3(0, 0, 1);
                        meshData[2].Normal = new Vector3(0, 0, 1);
                        meshData[3].Normal = new Vector3(0, 0, 1);
                        meshData[4].Normal = new Vector3(0, 0, -1);
                        meshData[5].Normal = new Vector3(0, 0, -1);
                        meshData[6].Normal = new Vector3(0, 0, -1);
                        meshData[7].Normal = new Vector3(0, 0, -1);

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
                        int indexOffset = filledPixelCount * 8;
                        for (var k = 0; k < thisPixelIndices.Length; k++)
                        {
                            thisPixelIndices[k] = (ushort) (thisPixelIndices[k] + indexOffset);
                        }

                        float x = pixelRelativeOffset.X;
                        float y = -pixelRelativeOffset.Y;

                        var pixelPositionMatrix = Matrix4x4.CreateTranslation(x * 2 - meshCenter.X, y * 2 - meshCenter.Y, frameIdx * 2 + 1);
                        for (var iv = 0; iv < thisPixel.Length; iv++)
                        {
                            thisPixel[iv].Vertex = Vector3.Transform(thisPixel[iv].Vertex, pixelPositionMatrix);
                        }
                    }

                    filledPixelCount++;
                }
            }

            return filledPixelCount;
        }

        protected override void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat format)
        {
            _textureData = pixels;
            _textureDataFormat = format;
            base.UploadTexture(size, pixels, flipped, format);
        }
    }
}