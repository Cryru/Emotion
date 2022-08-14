#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Platform.Debugger;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionSDF
{
    public sealed class EmotionSDFDrawableFontAtlas : DrawableFontAtlas
    {
        public static int GlyphSize = 256;
        public static readonly int SdfSize = 32;

        private static ConcurrentDictionary<Font, EmotionSDFReference> _renderedFonts { get; } = new();

        private EmotionSDFReference _sdfReference;
        private float _renderScaleRatio;

        /// <inheritdoc />
        public override Texture Texture
        {
            get => _sdfReference.AtlasFramebuffer?.ColorAttachment ?? Texture.NoTexture;
        }

        public EmotionSDFDrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {
            if (!_renderedFonts.TryGetValue(Font, out EmotionSDFReference? atlas))
            {
                atlas = new EmotionSDFReference(Font, GlyphSize, pixelFont);
                _renderedFonts.TryAdd(Font, atlas);
            }

            _sdfReference = atlas;
        }

        protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
            DrawableFontAtlas referenceAtlas = _sdfReference.ReferenceFont;

            _renderScaleRatio = RenderScale / _sdfReference.ReferenceFont.RenderScale;

            // Check which glyphs are missing from the atlas.
            var glyphsMissing = new List<DrawableGlyph>();
            var glyphsMissingReferences = new List<DrawableGlyph>();
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph g = glyphsToAdd[i];

                if (referenceAtlas.Glyphs.TryGetValue(g.Character, out DrawableGlyph? refG))
                {
                    // If found, just assign uv.
                    g.GlyphUV = refG.GlyphUV;
                }
                else
                {
                    var referenceGlyph = new DrawableGlyph(g.Character, g.FontGlyph, referenceAtlas.RenderScale);
                    referenceAtlas.Glyphs.Add(g.Character, referenceGlyph);
                    glyphsMissing.Add(g);
                    glyphsMissingReferences.Add(referenceGlyph);
                }
            }

            if (glyphsMissing.Count == 0) return;

            // Bin glyphs.
            var newBuffer = false;
            if (_sdfReference.AtlasFramebuffer == null)
            {
                var binningRects = new Rectangle[glyphsMissingReferences.Count];
                for (var i = 0; i < glyphsMissingReferences.Count; i++)
                {
                    DrawableGlyph gRef = glyphsMissingReferences[i];

                    float glyphWidth = gRef.Width + SdfSize * 2;
                    float glyphHeight = referenceAtlas.FontSize + SdfSize * 2; // Set to constant size to better align to baseline.

                    binningRects[i] = new Rectangle(0, 0, glyphWidth, glyphHeight);
                }

                Vector2 atlasSize = Binning.FitRectangles(binningRects, false, _sdfReference.BinningState);
                _sdfReference.AtlasFramebuffer = new FrameBuffer(atlasSize).WithColor(true, InternalFormat.Red, PixelFormat.Red).WithDepthStencil();
                _sdfReference.AtlasFramebuffer.ColorAttachment.Smooth = true;
                newBuffer = true;

                // Assign binned uvs
                for (var i = 0; i < glyphsMissingReferences.Count; i++)
                {
                    DrawableGlyph gRef = glyphsMissingReferences[i];
                    gRef.GlyphUV = binningRects[i];
                }
            }

            // RenderDoc.StartCapture();

            // Render glyphs.
            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            RenderState renderState = RenderState.Default.Clone();
            renderState.AlphaBlending = false;
            renderState.ViewMatrix = false;

            float bufferHeight = _sdfReference.AtlasFramebuffer.Size.Y;
            composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
            composer.SetState(renderState);
            composer.RenderTo(_sdfReference.AtlasFramebuffer);
            if (newBuffer) composer.ClearFrameBuffer();
            GenerateSdfBackend(composer, Font, glyphsMissingReferences, SdfSize, referenceAtlas.RenderScale);
            composer.PopModelMatrix();

            // Invert UVs along the Y axis, this is to fix the texture inversion from above.
            for (var i = 0; i < glyphsMissingReferences.Count; i++)
            {
                DrawableGlyph glyph = glyphsMissingReferences[i];
                glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);

                DrawableGlyph g = glyphsMissing[i];
                g.GlyphUV = glyph.GlyphUV;
            }

            composer.RenderTo(null);
            composer.SetState(prevState);
            composer.PopModelMatrix();

            // RenderDoc.EndCapture();
        }

        private static RenderStreamBatch<SdfVertex>? _sdfVertexStream;
        private static ShaderAsset? _lineShader;
        private static ShaderAsset? _fillShader;
        private static ShaderAsset? _sdfShader;

        public static void GenerateSdfBackend(RenderComposer renderer, Font font, List<DrawableGlyph> glyphs, float sdfDist, float scale)
        {
            // Initialize objects.
            _sdfVertexStream ??= new RenderStreamBatch<SdfVertex>(0, 1, false);
            _lineShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GlyphRenderLine.xml");
            _fillShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GlyphRenderFill.xml");
            if (_sdfShader == null)
            {
                _sdfShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");
            }

            if (_lineShader == null || _fillShader == null || _sdfShader == null)
            {
                Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.Renderer);
                return;
            }

            // Rasterize glyph commands.
            var painter = new GlyphPainter();
            for (var i = 0; i < glyphs.Count; i++)
            {
                DrawableGlyph glyph = glyphs[i];

                float baseline = -font.Descender * scale;
                float left = glyph.XBearing;
                Vector2 glyphPos = new Vector2(glyph.GlyphUV.X, glyph.GlyphUV.Y + baseline) + new Vector2(sdfDist - left, sdfDist);
                painter.RasterizeGlyph(glyph.FontGlyph, glyphPos, scale, sdfDist);
            }

            // Draw lines
            renderer.SetShader(_lineShader.Shader);
            Span<SdfVertex> memory = _sdfVertexStream.GetStreamMemory((uint) painter.LinePainter.Vertices.Count, BatchMode.SequentialTriangles);
            Span<SdfVertex> lineDataSpan = CollectionsMarshal.AsSpan(painter.LinePainter.Vertices);
            lineDataSpan.CopyTo(memory);
            _sdfVertexStream.FlushRender();
            renderer.SetShader();

            // Draw fill
            renderer.SetDepthTest(false);
            renderer.SetStencilTest(true);
            renderer.StencilWindingStart();
            renderer.ToggleRenderColor(false);

            renderer.SetShader(_fillShader.Shader);
            memory = _sdfVertexStream.GetStreamMemory((uint) painter.FillPainter.Vertices.Count, BatchMode.SequentialTriangles);
            Span<SdfVertex> fillDataSpan = CollectionsMarshal.AsSpan(painter.FillPainter.Vertices);
            fillDataSpan.CopyTo(memory);
            _sdfVertexStream.FlushRender();
            renderer.SetShader();

            // Draw full screen quad, inverting stencil where it is 1
            renderer.StencilWindingEnd();
            renderer.ToggleRenderColor(true);
            renderer.SetAlphaBlend(true);
            renderer.SetAlphaBlendType(BlendingFactor.OneMinusDstColor, BlendingFactor.Zero, BlendingFactor.One, BlendingFactor.Zero);
            Gl.StencilFunc(StencilFunction.Notequal, 0, 0xff);
            Gl.StencilOp(StencilOp.Zero, StencilOp.Zero, StencilOp.Zero);

            renderer.RenderSprite(Vector3.Zero, renderer.CurrentTarget.Size, Color.White);
        }

        public override void SetupDrawing(RenderComposer c, string text, FontEffect effect = FontEffect.None, float effectAmount = 0f, Color? effectColor = null)
        {
            base.SetupDrawing(c, text);

            ShaderProgram? shader = _sdfShader?.Shader;
            if (shader == null) return;

            c.SetShader(shader);
            shader.SetUniformFloat("scaleFactor", SdfSize * 2f);

            if (effect == FontEffect.Outline)
            {
                float scaleFactor = 0.5f / SdfSize;
                float outlineWidthInSdf = effectAmount * scaleFactor;
                shader.SetUniformFloat("outlineWidthDist", outlineWidthInSdf);
                shader.SetUniformColor("outlineColor", effectColor.GetValueOrDefault());
            }
            else
            {
                shader.SetUniformFloat("outlineWidthDist", 0);
            }
        }

        public override void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
            c.RenderSprite(pos, g.GlyphUV.Size * _renderScaleRatio, color, _sdfReference.AtlasFramebuffer?.ColorAttachment, g.GlyphUV);
        }

        public override void FinishDrawing(RenderComposer c)
        {
            base.FinishDrawing(c);
            c.SetShader();
        }
    }
}