#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionRenderer
{
    public class EmotionRendererDrawableFontAtlas : DrawableFontAtlas
    {
        private FrameBuffer? _atlasBuffer;
        private Binning.BinningResumableState? _bin;

        /// <inheritdoc />
        public override Texture Texture
        {
            get => _atlasBuffer?.ColorAttachment ?? Texture.NoTexture;
        }

        public EmotionRendererDrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {
        }

        private static bool _rendererInit;
        private static bool _rendererInitError;
        private static ShaderAsset? _noDiscardShader;
        private static ShaderAsset? _windingAaShader;

        private static RenderState? _glyphRenderState;
        private static FrameBuffer? _intermediateBuffer;

        private static void InitializeRenderer()
        {
            if (_rendererInit) return;

            if (_glyphRenderState == null)
            {
                _glyphRenderState = RenderState.Default.Clone();
                _glyphRenderState.ViewMatrix = false;
                _glyphRenderState.SFactorRgb = BlendingFactor.One;
                _glyphRenderState.DFactorRgb = BlendingFactor.One;
                _glyphRenderState.SFactorA = BlendingFactor.One;
                _glyphRenderState.DFactorA = BlendingFactor.One;
            }

            _noDiscardShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
            _windingAaShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/WindingAA.xml");
            if (_noDiscardShader == null || _windingAaShader == null)
            {
                Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.Renderer);
                _rendererInitError = true;
            }

            _rendererInit = true;
        }

        protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
            InitializeRenderer();
            if (_rendererInitError) return;

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
                    float glyphHeight = FontSize - Descent + glyphSpacing * 2;
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
            for (var i = 0; i < intermediateAtlasUVs.Length; i++)
            {
                intermediateAtlasUVs[i].Size -= new Vector2(glyphSpacing * 2);
            }

            // Prepare for rendering.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(intermediateAtlasSize.Value).WithColor();
            else
                _intermediateBuffer.Resize(intermediateAtlasSize.Value, true);

            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Render glyphs to the intermediate buffer.
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader!.Shader);
            var colors = new[]
            {
                new Color(1, 0, 0, 0),
                new Color(0, 1, 0, 0),
                new Color(0, 0, 1, 0),
                new Color(0, 0, 0, 1),
            };
            var offsets = new[]
            {
                new Vector3(-0.30f, 0, 0),
                new Vector3(0.30f, 0, 0),
                new Vector3(0.0f, -0.30f, 0),
                new Vector3(0.0f, 0.30f, 0),
            };
            for (var p = 0; p < 4; p++)
            {
                uint clr = colors[p].ToUint();
                Vector3 offset = offsets[p];
                EmotionGlyphRendererBackend.RenderGlyphs(Font, composer, glyphsToAdd, offset, clr, RenderScale, intermediateAtlasUVs);
            }

            composer.RenderTargetPop();

            // Copy to atlas buffer, while unwinding.
            composer.RenderTo(_atlasBuffer);
            if (justCreated) composer.ClearFrameBuffer();
            composer.SetAlphaBlend(false);
            composer.SetShader(_windingAaShader!.Shader);
            _windingAaShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);

            float bufferHeight = _atlasBuffer.AllocatedSize.Y;
            composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];
                Rectangle placeWithinIntermediateAtlas = intermediateAtlasUVs[i];
                Debug.Assert(atlasGlyph.GlyphUV.Size == placeWithinIntermediateAtlas.Size);
                composer.RenderSprite(atlasGlyph.GlyphUV.PositionZ(0), atlasGlyph.GlyphUV.Size, _intermediateBuffer.ColorAttachment, placeWithinIntermediateAtlas);
            }

            composer.PopModelMatrix();

            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph glyph = glyphsToAdd[i];
                glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);
            }

            composer.SetShader();
            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(prevState);
        }

        public override void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
            c.RenderSprite(pos, g.GlyphUV.Size, color, _atlasBuffer?.ColorAttachment, g.GlyphUV);
        }
    }
}