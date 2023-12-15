#region Using

using Emotion.Graphics.Objects;
using Emotion.IO;
using Emotion.Standard.OpenType;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionRenderer
{
    public class EmotionRendererDrawableFontAtlas : DrawableFontAtlas
    {
        private const int GLYPH_SPACING = 2;

        private FrameBuffer? _atlasBuffer;
        private Packing.PackingResumableState? _pack;

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
                var renderState = RenderState.Default.Clone();
                renderState.ViewMatrix = false;
                renderState.SFactorRgb = BlendingFactor.One;
                renderState.DFactorRgb = BlendingFactor.One;
                renderState.SFactorA = BlendingFactor.One;
                renderState.DFactorA = BlendingFactor.One;
                _glyphRenderState = renderState;
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

        protected override Vector2 BinGetGlyphDimensions(DrawableGlyph g)
        {
            float spacing = GLYPH_SPACING * 2;
            return new Vector2(MathF.Ceiling(g.Width) + spacing, FontHeight + spacing);
        }

        protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
        {
            InitializeRenderer();
            if (_rendererInitError) return;

            _pack ??= new Packing.PackingResumableState(Vector2.Zero);
            _pack = PackGlyphsInAtlas(glyphsToAdd, _pack);

            // Sync atlas.
            var clearBuffer = false;
            if (_atlasBuffer == null)
            {
                _atlasBuffer = new FrameBuffer(_pack.Size).WithColor();
                _atlasBuffer.ColorAttachment.Smooth = true;
                clearBuffer = true;
            }
            else if (_pack.Size != _atlasBuffer.Size)
            {
                Vector2 newSize = Vector2.Max(_pack.Size, _atlasBuffer.Size); // Dont size down.
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

            Vector2 intermediateAtlasSize = Packing.FitRectangles(intermediateAtlasUVs);
            for (var i = 0; i < intermediateAtlasUVs.Length; i++)
            {
                intermediateAtlasUVs[i] = intermediateAtlasUVs[i].Deflate(GLYPH_SPACING, GLYPH_SPACING);
            }

            // Create intermediate buffer.
            if (_intermediateBuffer == null)
                _intermediateBuffer = new FrameBuffer(intermediateAtlasSize).WithColor();
            else
                _intermediateBuffer.Resize(intermediateAtlasSize, true);

            // Prepare for rendering.
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
            if (clearBuffer) composer.ClearFrameBuffer();
            composer.SetAlphaBlend(false);
            composer.SetShader(_windingAaShader!.Shader);
            _windingAaShader.Shader.SetUniformVector2("drawSize", _intermediateBuffer.AllocatedSize);

            float bufferHeight = _atlasBuffer.Size.Y;
            composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
            for (var i = 0; i < glyphsToAdd.Count; i++)
            {
                DrawableGlyph atlasGlyph = glyphsToAdd[i];
                Rectangle placeWithinIntermediateAtlas = intermediateAtlasUVs[i];
                Assert(atlasGlyph.GlyphUV.Size == placeWithinIntermediateAtlas.Size);
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