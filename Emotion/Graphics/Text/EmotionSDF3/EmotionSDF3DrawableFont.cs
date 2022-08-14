#region Using

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text.EmotionRenderer;
using Emotion.Graphics.Text.EmotionSDF;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionSDF3
{
    public class EmotionSDF3DrawableFont : DrawableFont
    {
        public const int SDF_REFERENCE_FONT_SIZE = 100;
        public const float SDF_HIGH_RES_SCALE = 1f;
        public static readonly int SdfSize = 64; // 64 spread value is from GenerateSDF.frag

        private static ConcurrentDictionary<Font, EmotionSDFReference> _renderedFonts { get; } = new();

        private EmotionSDFReference _sdfReference;

        public EmotionSDF3DrawableFont(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
        {
            if (!_renderedFonts.TryGetValue(Font, out EmotionSDFReference? atlas))
            {
                atlas = new EmotionSDFReference(Font, SDF_REFERENCE_FONT_SIZE, pixelFont);
                _renderedFonts.TryAdd(Font, atlas);
            }

            _sdfReference = atlas;
        }

        private static bool _rendererInit;
        private static bool _rendererInitError;
        private static RenderState? _glyphRenderState;
        private static ShaderAsset? _noDiscardShader;
        private static ShaderAsset? _windingShader;
        private static ShaderAsset? _sdfGeneratingShader;
        private static ShaderAsset? _sdfShader;
        private static FrameBuffer? _intermediateBuffer;
        private static FrameBuffer? _intermediateBufferTwo;

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
            _windingShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/Winding.xml");
            _sdfGeneratingShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/GenerateSDF.xml");
            _sdfShader = Engine.AssetLoader.Get<ShaderAsset>("FontShaders/SDF.xml");
            if (_noDiscardShader == null || _windingShader == null || _sdfGeneratingShader == null || _sdfShader == null)
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

            var spacing = new Vector2(5);
            Vector2 spacingAtlas = spacing + new Vector2(2);

            // Ensure various font properties.
            _renderScaleRatio = RenderScale / _sdfReference.ReferenceFont.RenderScale;
            if (_glyphPadding == Vector2.Zero)
            {
                float scaleDiff = _sdfReference.ReferenceFont.RenderScale / RenderScale;
                _glyphPadding = spacing / scaleDiff;
            }

            _sdfParam = SdfSize * _sdfReference.ReferenceFont.RenderScale;

            // Check which glyphs are missing in the reference atlas.
            var refGlyphsToRender = new List<DrawableGlyph>();
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph reqGlyph = glyphsToAdd[i];
                if (_sdfReference.ReferenceFont.Glyphs.TryGetValue(reqGlyph.Character, out DrawableGlyph? refG))
                {
                    // Already renderer, just set uv.
                    reqGlyph.GlyphUV = refG.GlyphUV;
                }
                else
                {
                    refG = new DrawableGlyph(reqGlyph.Character, reqGlyph.FontGlyph, _sdfReference.ReferenceFont.RenderScale);
                    refGlyphsToRender.Add(refG);
                    _sdfReference.ReferenceFont.Glyphs.Add(reqGlyph.Character, refG);
                }
            }

            if (refGlyphsToRender.Count == 0) return;

            Rectangle[] intermediateAtlasUVs;
            var justCreated = false;
            if (_sdfReference.AtlasFramebuffer == null)
            {
                justCreated = true;

                // Initialize atlas by binning first.
                var binningRects = new Rectangle[refGlyphsToRender.Count];
                intermediateAtlasUVs = new Rectangle[refGlyphsToRender.Count];
                for (var i = 0; i < refGlyphsToRender.Count; i++)
                {
                    DrawableGlyph atlasGlyph = refGlyphsToRender[i];
                    Vector2 glyphRenderSize = new Vector2(atlasGlyph.Width, _sdfReference.ReferenceFont.FontSize) + spacingAtlas * 2;
                    binningRects[i] = new Rectangle(0, 0, glyphRenderSize);
                    intermediateAtlasUVs[i] = new Rectangle(0, 0, glyphRenderSize);
                }

                // Apply to atlas glyphs.
                Vector2 atlasSize = Binning.FitRectangles(binningRects, false, _sdfReference.BinningState);
                for (var i = 0; i < binningRects.Length; i++)
                {
                    DrawableGlyph atlasGlyph = refGlyphsToRender[i];
                    Rectangle binPosition = binningRects[i];
                    atlasGlyph.GlyphUV = binPosition.Deflate(spacingAtlas.X, spacingAtlas.Y);
                }

                _sdfReference.AtlasFramebuffer = new FrameBuffer(atlasSize).WithColor(true, InternalFormat.Red, PixelFormat.Red);
                _sdfReference.AtlasFramebuffer.ColorAttachment.Smooth = true;
            }
            else
            {
                intermediateAtlasUVs = new Rectangle[0];
            }

            // Create high resolution glyphs.
            var sdfFullResGlyphs = new List<DrawableGlyph>(refGlyphsToRender.Count);
            var sdfTempRects = new Rectangle[refGlyphsToRender.Count];
            Vector2 sdfGlyphSpacing = spacing * (1f / _sdfReference.ReferenceFont.RenderScale);
            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                DrawableGlyph refGlyph = refGlyphsToRender[i];
                var sdfGlyph = new DrawableGlyph(refGlyph.Character, refGlyph.FontGlyph, SDF_HIGH_RES_SCALE);
                sdfFullResGlyphs.Add(sdfGlyph);
                sdfTempRects[i] = new Rectangle(0, 0, new Vector2(sdfGlyph.Width, Font.UnitsPerEm * SDF_HIGH_RES_SCALE) + sdfGlyphSpacing * 2);
            }

            Vector2 sdfBaseAtlas = Binning.FitRectangles(sdfTempRects);

            // Remove spacing.
            for (var i = 0; i < sdfTempRects.Length; i++)
            {
                Rectangle r = sdfTempRects[i];
                sdfTempRects[i] = new Rectangle(r.Position + sdfGlyphSpacing, r.Size - sdfGlyphSpacing * 2);
            }

            // Generate ping-pong buffers.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(sdfBaseAtlas).WithColor(true, InternalFormat.Red, PixelFormat.Red);
            else
                _intermediateBuffer.Resize(sdfBaseAtlas, true);
            _intermediateBuffer.ColorAttachment.Smooth = false;

            if (_intermediateBufferTwo == null)
                _intermediateBufferTwo = new FrameBuffer(sdfBaseAtlas).WithColor(true, InternalFormat.Red, PixelFormat.Red);
            else
                _intermediateBufferTwo.Resize(sdfBaseAtlas, true);
            _intermediateBufferTwo.ColorAttachment.Smooth = false;

            // Emotion.Platform.Debugger.RenderDoc.StartCapture();
            RenderComposer composer = Engine.Renderer;
            RenderState prevState = composer.CurrentState.Clone();
            composer.PushModelMatrix(Matrix4x4.Identity, false);

            // Render high res glyphs.
            composer.SetState(_glyphRenderState);
            composer.RenderToAndClear(_intermediateBuffer);
            composer.SetShader(_noDiscardShader!.Shader);
            uint clr = new Color(1, 0, 0, 0).ToUint();
            Vector3 offset = Vector3.Zero;
            EmotionGlyphRendererBackend.RenderGlyphs(Font, composer, sdfFullResGlyphs, offset, clr, SDF_HIGH_RES_SCALE, sdfTempRects);
            composer.RenderTargetPop();

            // Unwind them.
            composer.SetAlphaBlend(false);
            composer.RenderToAndClear(_intermediateBufferTwo);
            composer.SetShader(_windingShader!.Shader);
            _windingShader.Shader.SetUniformVector2("drawSize", _intermediateBufferTwo.AllocatedSize);
            composer.RenderFrameBuffer(_intermediateBuffer);
            composer.SetShader();
            composer.RenderTargetPop();

            // Generate SDF
            _intermediateBuffer.ColorAttachment.Smooth = true;
            _intermediateBufferTwo.ColorAttachment.Smooth = true;
            composer.SetState(RenderState.Default);
            composer.SetUseViewMatrix(false);
            composer.SetShader(_sdfGeneratingShader!.Shader);

            // Generate SDF while downsizing individual glyphs.
            // If we do it in bulk the GPU might hang long enough for the OS to kill us.
            float bufferHeight = _sdfReference.AtlasFramebuffer.AllocatedSize.Y;
            composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
            composer.RenderTo(_sdfReference.AtlasFramebuffer);
            if (justCreated) composer.ClearFrameBuffer();
            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                DrawableGlyph refGlyph = refGlyphsToRender[i];
                Rectangle placeWithinSdfAtlas = sdfTempRects[i];

                // Copy sdf spacing as there might be data there.
                placeWithinSdfAtlas.Position -= sdfGlyphSpacing;
                placeWithinSdfAtlas.Size += sdfGlyphSpacing * 2;
                placeWithinSdfAtlas.Position = placeWithinSdfAtlas.Position.Floor();
                placeWithinSdfAtlas.Size = placeWithinSdfAtlas.Size.Floor();

                composer.RenderSprite(
                    new Vector3(refGlyph.GlyphUV.X - spacing.X, refGlyph.GlyphUV.Y - spacing.Y, 0),
                    refGlyph.GlyphUV.Size + spacing * 2,
                    _intermediateBufferTwo.ColorAttachment,
                    placeWithinSdfAtlas);
            }

            composer.PopModelMatrix();

            composer.SetShader();
            composer.RenderTo(null);
            composer.PopModelMatrix();
            composer.SetState(prevState);
            // Emotion.Platform.Debugger.RenderDoc.EndCapture();

            for (var i = 0; i < refGlyphsToRender.Count; i++)
            {
                DrawableGlyph glyph = refGlyphsToRender[i];
                glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);
            }

            // Apply all UVs from the ref atlas to the req atlas.
            foreach (KeyValuePair<char, DrawableGlyph> atlasGlyph in Glyphs)
            {
                for (var i = 0; i < refGlyphsToRender.Count; i++)
                {
                    DrawableGlyph refGlyph = refGlyphsToRender[i];
                    if (refGlyph.Character == atlasGlyph.Key)
                    {
                        // Apply UV padding to fit the GlyphRenderPadding.
                        atlasGlyph.Value.GlyphUV = refGlyph.GlyphUV.Inflate(spacing.X, spacing.Y);
                        break;
                    }
                }
            }
        }

        private Vector2 _glyphPadding;
        private float _sdfParam;
        private float _renderScaleRatio;

        public override void SetupDrawing(RenderComposer c, string text)
        {
            base.SetupDrawing(c, text);

            if (_sdfShader != null)
            {
                c.SetShader(_sdfShader.Shader);
                _sdfShader.Shader.SetUniformFloat("scaleFactor", _sdfParam * 2f);
            }
        }

        public override void DrawGlyph(RenderComposer c, DrawableGlyph g, Vector3 pos, Color color)
        {
            c.RenderSprite(pos - _glyphPadding.ToVec3(), g.GlyphUV.Size * _renderScaleRatio, color, _sdfReference.AtlasFramebuffer?.ColorAttachment, g.GlyphUV);
        }

        public override void FinishDrawing(RenderComposer c)
        {
            base.FinishDrawing(c);

            if (_sdfShader != null) c.SetShader();
        }
    }
}