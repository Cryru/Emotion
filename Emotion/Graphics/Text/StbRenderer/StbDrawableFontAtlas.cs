#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.OpenType;
using Emotion.Utility;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.StbRenderer
{
    public class StbDrawableFontAtlas : DrawableFontAtlas
    {
        private const int GLYPH_SPACING = 1;

        private static RenderState? _glyphRenderState;
        private FrameBuffer? _intermediateBuffer;
        private FrameBuffer? _atlasBuffer;
        private Packing.PackingResumableState? _bin;

        /// <inheritdoc />
        public override Texture Texture
        {
            get => _atlasBuffer?.ColorAttachment ?? Texture.NoTexture;
        }

        public StbDrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {
        }

        protected override Vector2 BinGetGlyphDimensions(DrawableGlyph g)
        {
            const float spacing = GLYPH_SPACING * 2;
            return new Vector2(MathF.Ceiling(g.Width) + spacing, FontHeight + spacing);
        }

        protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
            if (_glyphRenderState == null)
            {
                _glyphRenderState = RenderState.Default.Clone();
                _glyphRenderState.ViewMatrix = false;
                _glyphRenderState.SFactorRgb = BlendingFactor.One;
                _glyphRenderState.DFactorRgb = BlendingFactor.One;
                _glyphRenderState.SFactorA = BlendingFactor.One;
                _glyphRenderState.DFactorA = BlendingFactor.One;
            }

            _bin ??= new Packing.PackingResumableState(Vector2.Zero);
            _bin = PackGlyphsInAtlas(glyphsToAdd, _bin);

            // Sync atlas.
            var clearBuffer = false;
            if (_atlasBuffer == null)
            {
                _atlasBuffer = new FrameBuffer(_bin.Size).WithColor();
                //_atlasBuffer.ColorAttachment.Smooth = true;
                clearBuffer = true;
            }
            else if (_bin.Size != _atlasBuffer.Size)
            {
                Vector2 newSize = Vector2.Max(_bin.Size, _atlasBuffer.Size); // Dont size down.
                _atlasBuffer.Resize(newSize, true);
                clearBuffer = true;
            }

            // Remove spacing from UVs and create rects for intermediate atlas bin.
            var intermediateAtlasUVs = new Rectangle[glyphsToAdd.Count];
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];
                intermediateAtlasUVs[i] = new Rectangle(0, 0, atlasGlyph.GlyphUV.Size);
                atlasGlyph.GlyphUV = atlasGlyph.GlyphUV.Deflate(GLYPH_SPACING, GLYPH_SPACING);
            }

            Vector2 intermediateAtlasSize = Packing.FitRectangles(intermediateAtlasUVs)!;
            for (var i = 0; i < intermediateAtlasUVs.Length; i++)
            {
                intermediateAtlasUVs[i] = intermediateAtlasUVs[i].Deflate(GLYPH_SPACING, GLYPH_SPACING);
            }

            // Create intermediate buffer.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(intermediateAtlasSize).WithColor();
            else
                _intermediateBuffer.Resize(intermediateAtlasSize, true);

            // Bin them for the stb rasterization atlas.
            var stbAtlasData = new byte[(int) intermediateAtlasSize.X * (int) intermediateAtlasSize.Y];
            var stride = (int) intermediateAtlasSize.X;
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];

                Vector2 size = BinGetGlyphDimensions(atlasGlyph);
                size -= new Vector2(GLYPH_SPACING * 2);

                var canvas = new StbGlyphCanvas((int) size.X + 1, (int) size.Y + 1);
                StbGlyphRendererBackend.RenderGlyph(Font, canvas, atlasGlyph, RenderScale);

                // Remove canvas padding.
                canvas.Width--;
                canvas.Height--;

                // Copy pixels and record the location of the glyph.
                Vector2 uvLoc = intermediateAtlasUVs[i].Position;
                var uvX = (int) uvLoc.X;
                var uvY = (int) uvLoc.Y;

                for (var row = 0; row < canvas.Height; row++)
                {
                    for (var col = 0; col < canvas.Width; col++)
                    {
                        int x = uvX + col;
                        int y = uvY + row;
                        stbAtlasData[y * stride + x] = canvas.Data[row * canvas.Stride + col];
                    }
                }
            }

            // Upload to GPU.
            byte[] rgbaData = ImageUtil.AToRgba(stbAtlasData);
            var tempGlyphTexture = new Texture();
            tempGlyphTexture.Upload(intermediateAtlasSize, rgbaData, PixelFormat.Rgba, InternalFormat.Rgba, PixelType.UnsignedByte);

            // Render to big atlas.
            RenderComposer composer = Engine.Renderer;
            RenderState previousRenderState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);
            composer.SetState(_glyphRenderState);
            composer.RenderTo(_atlasBuffer);
            if (clearBuffer) composer.ClearFrameBuffer();

            float bufferHeight = _atlasBuffer.Size.Y;
            composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];
                Rectangle placeWithinIntermediateAtlas = intermediateAtlasUVs[i];
                Debug.Assert(atlasGlyph.GlyphUV.Size == placeWithinIntermediateAtlas.Size);
                composer.RenderSprite(atlasGlyph.GlyphUV.Position.ToVec3(), atlasGlyph.GlyphUV.Size, tempGlyphTexture, placeWithinIntermediateAtlas);
            }

            composer.PopModelMatrix();

            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph glyph = glyphsToAdd[i];
                glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);
            }

            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(previousRenderState);
            tempGlyphTexture.Dispose();
        }

        public override void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
            c.RenderSprite(pos, g.GlyphUV.Size, color, _atlasBuffer?.ColorAttachment, g.GlyphUV);
        }
    }
}