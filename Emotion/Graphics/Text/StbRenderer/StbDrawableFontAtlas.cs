#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static RenderState? _glyphRenderState;
        private FrameBuffer? _intermediateBuffer;
        private FrameBuffer? _atlasBuffer;
        private Binning.BinningResumableState? _bin;

        /// <inheritdoc />
        public override Texture Texture
        {
            get => _atlasBuffer?.ColorAttachment ?? Texture.NoTexture;
        }

        public StbDrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {
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

            const int glyphSpacing = 2;

            Rectangle[] intermediateAtlasUVs;
            var justCreated = false;
            if (_atlasBuffer == null)
            {
                justCreated = true;

                // Initialize atlas by binning first.
                var binningRects = new Rectangle[glyphsToAdd.Count];
                intermediateAtlasUVs = new Rectangle[glyphsToAdd.Count];
                for (var i = 0; i < glyphsToAdd.Count; i++)
                {
                    DrawableGlyph atlasGlyph = glyphsToAdd[i];
                    float glyphWidth = MathF.Ceiling(atlasGlyph.Width) + glyphSpacing * 2;
                    float glyphHeight = MathF.Ceiling(FontSize - Descent) + glyphSpacing * 2;
                    binningRects[i] = new Rectangle(0, 0, glyphWidth, glyphHeight);
                    intermediateAtlasUVs[i] = new Rectangle(0, 0, glyphWidth, glyphHeight);
                }

                // Apply to atlas glyphs.
                var resumableState = new Binning.BinningResumableState(Vector2.Zero);
                Vector2 atlasSize = Binning.FitRectangles(binningRects, false, resumableState);
                for (var i = 0; i < binningRects.Length; i++)
                {
                    DrawableGlyph atlasGlyph = glyphsToAdd[i];
                    Rectangle binPosition = binningRects[i];
                    atlasGlyph.GlyphUV = binPosition.Deflate(glyphSpacing, glyphSpacing);
                }

                _atlasBuffer = new FrameBuffer(atlasSize).WithColor();
                _bin = resumableState;
            }
            else
            {
                intermediateAtlasUVs = new Rectangle[0];
            }

            Vector2? intermediateAtlasSize = Binning.FitRectangles(intermediateAtlasUVs);
            Debug.Assert(intermediateAtlasSize != null);
            Vector2 intermediateSize = intermediateAtlasSize.Value;
            for (var i = 0; i < intermediateAtlasUVs.Length; i++)
            {
                intermediateAtlasUVs[i].Size -= new Vector2(glyphSpacing * 2);
            }

            // Prepare for rendering.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(intermediateAtlasSize.Value).WithColor();
            else
                _intermediateBuffer.Resize(intermediateAtlasSize.Value, true);

            // Bin them for the stb rasterization atlas.
            var stbAtlasData = new byte[(int) intermediateSize.X * (int) intermediateSize.Y];
            var stride = (int) intermediateSize.X;
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];

                float glyphWidth = MathF.Ceiling(atlasGlyph.Width);
                float glyphHeight = MathF.Ceiling(FontSize - Descent);
                var canvas = new StbGlyphCanvas((int) glyphWidth + 1, (int) glyphHeight + 1);
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
            tempGlyphTexture.Upload(intermediateSize, rgbaData, PixelFormat.Rgba, InternalFormat.Rgba, PixelType.UnsignedByte);

            // Render to big atlas.
            RenderComposer composer = Engine.Renderer;
            RenderState previousRenderState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);
            composer.SetState(_glyphRenderState);
            composer.RenderTo(_atlasBuffer);
            if (justCreated) composer.ClearFrameBuffer();

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

            base.AddGlyphsToAtlas(glyphsToAdd);
        }

        public override void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
            c.RenderSprite(pos, g.GlyphUV.Size, color, _atlasBuffer?.ColorAttachment, g.GlyphUV);
        }
    }
}