#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
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
        private Vector3 _drawOffset;

        /// <inheritdoc />
        public override Texture Texture
        {
            get => _sdfReference.AtlasFramebuffer?.ColorAttachment ?? Texture.NoTexture;
        }

        public EmotionSDFDrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {           
            _sdfShader ??= Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");

            if (!_renderedFonts.TryGetValue(Font, out EmotionSDFReference? atlas))
            {
                atlas ??= new EmotionSDFReference(Font, GlyphSize, pixelFont);
                _renderedFonts.TryAdd(Font, atlas);
            }

            _sdfReference = atlas;
            atlas.FontsThatUseReference.Add(this);

            _renderScaleRatio = RenderScale / _sdfReference.ReferenceFont.RenderScale;
            _drawOffset = (new Vector2(SdfSize) * _renderScaleRatio).ToVec3();
        }

        protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
            DrawableFontAtlas referenceAtlas = _sdfReference.ReferenceFont;

            // Check which glyphs are missing from the atlas.
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
                    glyphsMissingReferences.Add(referenceGlyph);
                }
            }

            if (glyphsMissingReferences.Count == 0) return;

            // Bin glyphs.
            Binning.BinningResumableState bin = _sdfReference.BinningState;
            bin = BinGlyphsInAtlas(glyphsMissingReferences, bin);
            _sdfReference.BinningState = bin;

            // Create list of missing glyphs in the atlas, from reference ones.
            // This needs to be recalculated because binning might decide to bin all.
            var glyphsMissing = new List<DrawableGlyph>();
            for (var i = 0; i < glyphsMissingReferences.Count; i++)
            {
                DrawableGlyph referenceGlyph = glyphsMissingReferences[i];

                // Pull out the glyph in the non-reference atlas as well.
                DrawableGlyph? atlasGlyph = Glyphs[referenceGlyph.Character];
                glyphsMissing.Add(atlasGlyph);
                Debug.Assert(atlasGlyph != null);
            }

            // Resync atlas texture.
            var bufferRecreated = false;
            var clearBuffer = false;
            if (_sdfReference.AtlasFramebuffer == null)
            {
                _sdfReference.AtlasFramebuffer = new FrameBuffer(bin.Size).WithColor(true, InternalFormat.Red, PixelFormat.Red).WithDepthStencil();
                _sdfReference.AtlasFramebuffer.ColorAttachment.Smooth = true;
                clearBuffer = true;
            }
            else if (bin.Size != _sdfReference.AtlasFramebuffer.Size)
            {
                Vector2 newSize = Vector2.Max(bin.Size, _sdfReference.AtlasFramebuffer.Size); // Dont size down.
                _sdfReference.AtlasFramebuffer.Resize(newSize, true);
                clearBuffer = true;
                bufferRecreated = true;
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
            if (clearBuffer) composer.ClearFrameBuffer();
            GenerateSdfBackend(composer, Font, glyphsMissingReferences, SdfSize, referenceAtlas.RenderScale);
            composer.PopModelMatrix();

            composer.RenderTo(null);
            composer.SetState(prevState);
            composer.PopModelMatrix();

            // RenderDoc.EndCapture();

            // Invert UVs along the Y axis, this is to fix the texture inversion from above.
            for (var i = 0; i < glyphsMissingReferences.Count; i++)
            {
                DrawableGlyph glyph = glyphsMissingReferences[i];
                glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);

                DrawableGlyph g = glyphsMissing[i];
                g.GlyphUV = glyph.GlyphUV;
            }

            // If the reference atlas buffer was recreated we need to reassign the UVs in all atlases
            // that use the reference atlas.
            if (bufferRecreated)
            {
                List<DrawableFontAtlas> allAtlases = _sdfReference.FontsThatUseReference;
                for (var i = 0; i < allAtlases.Count; i++)
                {
                    DrawableFontAtlas atlas = allAtlases[i];
                    if (atlas == this) continue;

                    Dictionary<char, DrawableGlyph>? glyphs = atlas.Glyphs;
                    foreach ((char glyphChar, DrawableGlyph? atlasGlyph) in glyphs)
                    {
                        if (!referenceAtlas.Glyphs.TryGetValue(glyphChar, out DrawableGlyph? referenceGlyph)) continue;
                        atlasGlyph.GlyphUV = referenceGlyph.GlyphUV;
                    }
                }
            }
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

            if (_lineShader == null || _fillShader == null)
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

        protected override Dictionary<char, DrawableGlyph> BinGetAllGlyphs()
        {
            return _sdfReference.ReferenceFont.Glyphs;
        }

        protected override Vector2 BinGetGlyphDimensions(DrawableGlyph g)
        {
            // Set to constant size to better align to baseline.
            DrawableFontAtlas referenceAtlas = _sdfReference.ReferenceFont;
            float sdfSpread = SdfSize * 2;
            return new Vector2(g.Width + sdfSpread, referenceAtlas.FontHeight + sdfSpread);
        }

        public override void SetupDrawing(RenderComposer c, string text, FontEffect effect = FontEffect.None, float effectAmount = 0f, Color? effectColor = null)
        {
            base.SetupDrawing(c, text, effect, effectAmount, effectColor);

            ShaderProgram? shader = _sdfShader?.Shader;
            if (shader == null) return;

            c.SetShader(shader);

            if (PixelFont && _sdfReference.AtlasFramebuffer != null)
                shader.SetUniformVector2("scaleFactor", _sdfReference.AtlasFramebuffer.Size);
            else
                shader.SetUniformVector2("scaleFactor", new Vector2(SdfSize * 2f));

            if (effect == FontEffect.Outline)
            {
                float scaleFactor = 0.5f / (SdfSize * _renderScaleRatio);
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
            c.RenderSprite(pos - _drawOffset, g.GlyphUV.Size * _renderScaleRatio, color, _sdfReference.AtlasFramebuffer?.ColorAttachment, g.GlyphUV);
        }

        public override void FinishDrawing(RenderComposer c)
        {
            base.FinishDrawing(c);
            c.SetShader();
        }
    }
}