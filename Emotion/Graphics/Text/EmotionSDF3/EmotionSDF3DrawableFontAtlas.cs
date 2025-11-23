#nullable enable

#region Using

using System.Collections.Concurrent;
using Emotion.Core.Systems.IO;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility;
using Emotion.Graphics.Shading;
using Emotion.Graphics.Text.EmotionRenderer;
using Emotion.Graphics.Text.EmotionSDF;
using Emotion.Standard.Parsers.OpenType;
using OpenGL;

#endregion

namespace Emotion.Graphics.Text.EmotionSDF3;

public class EmotionSDF3DrawableFontAtlas : DrawableFontAtlas
{
    public const int SDF_REFERENCE_FONT_SIZE = 100;
    public const float SDF_HIGH_RES_SCALE = 1f;
    public static readonly int SdfSize = 64; // 64 spread value is from GenerateSDF.frag

    private static Vector2 _sdfGlyphSpacing = new Vector2(5);
    private static Vector2 _sdfAtlasSpacing = _sdfGlyphSpacing + new Vector2(2);

    private static ConcurrentDictionary<Font, EmotionSDFReference> _renderedFonts { get; } = new();

    private EmotionSDFReference _sdfReference;

    /// <inheritdoc />
    public override Texture Texture
    {
        get => _sdfReference.AtlasFramebuffer?.ColorAttachment ?? Texture.NoTexture;
    }

    // Useful resources:
    // https://medium.com/@calebfaith/implementing-msdf-font-in-opengl-ea09a9ab7e00
    // https://github.com/Chlumsky/msdfgen/issues/22
    // https://drewcassidy.me/2020/06/26/sdf-antialiasing/

    public EmotionSDF3DrawableFontAtlas(Font font, int fontSize, bool pixelFont = false) : base(font, fontSize, pixelFont)
    {
        if (!_renderedFonts.TryGetValue(Font, out EmotionSDFReference? atlas))
        {
            atlas = EmotionSDFReference.TryLoadReferenceFromFile(this, SDF_REFERENCE_FONT_SIZE, pixelFont, "EmotionSDF3", true);
            atlas ??= new EmotionSDFReference(Font, SDF_REFERENCE_FONT_SIZE, pixelFont);
            _renderedFonts.TryAdd(Font, atlas);
        }

        _sdfReference = atlas;
        atlas.FontsThatUseReference.Add(this);

        _renderScaleRatio = RenderScale / _sdfReference.ReferenceFont.RenderScale;
        if (_glyphPadding == Vector2.Zero)
        {
            float scaleDiff = _sdfReference.ReferenceFont.RenderScale / RenderScale;
            _glyphPadding = _sdfGlyphSpacing / scaleDiff;
        }

        _glyphPaddingSize = _sdfGlyphSpacing * RenderScale;
        _sdfParam = SdfSize * _sdfReference.ReferenceFont.RenderScale;
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
            var renderState = RenderState.Default.Clone();
            renderState.ViewMatrix = false;
            renderState.SFactorRgb = BlendingFactor.One;
            renderState.DFactorRgb = BlendingFactor.One;
            renderState.SFactorA = BlendingFactor.One;
            renderState.DFactorA = BlendingFactor.One;
            _glyphRenderState = renderState;
        }

        _noDiscardShader = Engine.AssetLoader.LEGACY_Get<ShaderAsset>("FontShaders/VertColorNoDiscard.xml");
        _windingShader = Engine.AssetLoader.LEGACY_Get<ShaderAsset>("FontShaders/Winding.xml");
        _sdfGeneratingShader = Engine.AssetLoader.LEGACY_Get<ShaderAsset>("FontShaders/GenerateSDF.xml");
        _sdfShader = Engine.AssetLoader.LEGACY_Get<ShaderAsset>("FontShaders/SDF.xml");
        if (_noDiscardShader == null || _windingShader == null || _sdfGeneratingShader == null || _sdfShader == null)
        {
            Engine.Log.Warning("Atlas rendering shader missing.", MessageSource.Renderer);
            _rendererInitError = true;
        }

        _rendererInit = true;
    }

    protected override Vector2 BinGetGlyphDimensions(DrawableGlyph g)
    {
        DrawableFontAtlas referenceAtlas = _sdfReference.ReferenceFont;
        Vector2 spacing = _sdfAtlasSpacing * 2;
        return new Vector2(g.Width, referenceAtlas.FontHeight) + spacing;
    }

    protected override Dictionary<char, DrawableGlyph> BinGetAllGlyphs()
    {
        return _sdfReference.ReferenceFont.Glyphs;
    }

    protected override void AddGlyphsToAtlas(List<DrawableGlyph> glyphsToAdd)
    {
        InitializeRenderer();
        if (_rendererInitError) return;

        DrawableFontAtlas referenceAtlas = _sdfReference.ReferenceFont;

        // Check which glyphs are missing in the reference atlas.
        var glyphsMissingReferences = new List<DrawableGlyph>();
        for (var i = 0; i < glyphsToAdd.Count; i++)
        {
            DrawableGlyph reqGlyph = glyphsToAdd[i];
            if (referenceAtlas.Glyphs.TryGetValue(reqGlyph.Character, out DrawableGlyph? refG))
            {
                // Already renderer, just set uv.
                reqGlyph.GlyphUV = refG.GlyphUV;
            }
            else
            {
                refG = new DrawableGlyph(reqGlyph.Character, reqGlyph.FontGlyph, referenceAtlas.RenderScale);
                glyphsMissingReferences.Add(refG);
                referenceAtlas.Glyphs.Add(reqGlyph.Character, refG);
            }
        }

        if (glyphsMissingReferences.Count == 0) return;

        Packing.PackingResumableState bin = _sdfReference.PackingState;
        bin = PackGlyphsInAtlas(glyphsMissingReferences, bin);
        _sdfReference.PackingState = bin;

        // Create list of missing glyphs in the atlas, from reference ones.
        // This needs to be recalculated because packing might decide to repack all.
        var glyphsMissing = new List<DrawableGlyph>();
        for (var i = 0; i < glyphsMissingReferences.Count; i++)
        {
            DrawableGlyph referenceGlyph = glyphsMissingReferences[i];

            // Pull out the glyph in the non-reference atlas as well.
            DrawableGlyph? atlasGlyph = Glyphs[referenceGlyph.Character];
            glyphsMissing.Add(atlasGlyph);
            Assert(atlasGlyph != null);
        }

        // Resync atlas texture.
        var clearBuffer = false;
        var bufferRecreated = false;
        if (_sdfReference.AtlasFramebuffer == null)
        {
            _sdfReference.AtlasFramebuffer = new FrameBuffer(bin.Size).WithColor(true, InternalFormat.Red, PixelFormat.Red);
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

        // Remove spacing from UVs and create rects for intermediate atlas bin.
        var intermediateAtlasUVs = new Rectangle[glyphsMissingReferences.Count];
        for (var i = 0; i < glyphsMissingReferences.Count; i++)
        {
            DrawableGlyph atlasGlyph = glyphsMissingReferences[i];
            intermediateAtlasUVs[i] = new Rectangle(0, 0, atlasGlyph.GlyphUV.Size);
            atlasGlyph.GlyphUV = atlasGlyph.GlyphUV.Deflate(_sdfAtlasSpacing.X, _sdfAtlasSpacing.Y);
        }

        Vector2? intermediateAtlasSize = Packing.FitRectangles(intermediateAtlasUVs);
        Assert(intermediateAtlasSize != null);
        for (var i = 0; i < intermediateAtlasUVs.Length; i++)
        {
            intermediateAtlasUVs[i] = intermediateAtlasUVs[i].Deflate(_sdfAtlasSpacing.X, _sdfAtlasSpacing.Y);
        }

        // Create high resolution glyphs.
        var sdfFullResGlyphs = new List<DrawableGlyph>(glyphsMissingReferences.Count);
        var sdfTempRects = new Rectangle[glyphsMissingReferences.Count];
        Vector2 sdfGlyphSpacing = _sdfGlyphSpacing * (1f / _sdfReference.ReferenceFont.RenderScale);
        for (var i = 0; i < glyphsMissingReferences.Count; i++)
        {
            DrawableGlyph refGlyph = glyphsMissingReferences[i];
            var sdfGlyph = new DrawableGlyph(refGlyph.Character, refGlyph.FontGlyph, SDF_HIGH_RES_SCALE);
            sdfFullResGlyphs.Add(sdfGlyph);

            float glyphWidth = sdfGlyph.Width + sdfGlyphSpacing.X * 2;
            float glyphHeight = Font.Height * SDF_HIGH_RES_SCALE + sdfGlyphSpacing.Y * 2;
            sdfTempRects[i] = new Rectangle(0, 0, glyphWidth, glyphHeight);
        }

        // todo: this can potentially create 16k textures which wont work on all GPUs/drivers.
        // we probably need to split it into multiple draws in those cases.
        Vector2 sdfBaseAtlas = Packing.FitRectangles(sdfTempRects);

        // Remove spacing.
        for (var i = 0; i < sdfTempRects.Length; i++)
        {
            Rectangle r = sdfTempRects[i];
            sdfTempRects[i] = r.Deflate(sdfGlyphSpacing.X, sdfGlyphSpacing.Y);
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

        // Emotion.Core.Platform.Debugger.RenderDoc.StartCapture();

        Renderer composer = Engine.Renderer;
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
        float bufferHeight = _sdfReference.AtlasFramebuffer.Size.Y;
        composer.PushModelMatrix(Matrix4x4.CreateScale(1, -1, 1) * Matrix4x4.CreateTranslation(0, bufferHeight, 0));
        composer.RenderTo(_sdfReference.AtlasFramebuffer);
        if (clearBuffer) composer.ClearFrameBuffer();
        for (var i = 0; i < glyphsMissingReferences.Count; i++)
        {
            DrawableGlyph refGlyph = glyphsMissingReferences[i];
            Rectangle placeWithinSdfAtlas = sdfTempRects[i];

            // Copy sdf spacing as there might be data there.
            placeWithinSdfAtlas.Position -= sdfGlyphSpacing;
            placeWithinSdfAtlas.Size += sdfGlyphSpacing * 2;
            placeWithinSdfAtlas.Position = placeWithinSdfAtlas.Position.Floor();
            placeWithinSdfAtlas.Size = placeWithinSdfAtlas.Size.Floor();

            composer.RenderSprite(
                new Vector3(refGlyph.GlyphUV.X - _sdfGlyphSpacing.X, refGlyph.GlyphUV.Y - _sdfGlyphSpacing.Y, 0),
                refGlyph.GlyphUV.Size + _sdfGlyphSpacing * 2,
                _intermediateBufferTwo.ColorAttachment,
                placeWithinSdfAtlas);
        }

        composer.PopModelMatrix();

        composer.SetShader();
        composer.RenderTo(null);
        composer.PopModelMatrix();
        composer.SetState(prevState);

        // Emotion.Core.Platform.Debugger.RenderDoc.EndCapture();

        // Invert UVs along the Y axis, this is to fix the texture inversion from above.
        for (var i = 0; i < glyphsMissingReferences.Count; i++)
        {
            DrawableGlyph glyph = glyphsMissingReferences[i];
            glyph.GlyphUV.Location = new Vector2(glyph.GlyphUV.X, bufferHeight - glyph.GlyphUV.Bottom);

            // Apply UV padding to fit the GlyphRenderPadding.
            DrawableGlyph atlasGlyph = glyphsMissing[i];
            atlasGlyph.GlyphUV = glyph.GlyphUV.Inflate(_sdfGlyphSpacing.X, _sdfGlyphSpacing.Y);
        }

        _sdfReference.CacheToFile(SDF_REFERENCE_FONT_SIZE, PixelFont, "EmotionSDF3");

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

    private Vector2 _glyphPadding;
    private Vector2 _glyphPaddingSize;
    private float _sdfParam;
    private float _renderScaleRatio;

    public override void SetupDrawing(Renderer c, string text, FontEffect effect = FontEffect.None, float effectAmount = 0f, Color? effectColor = null)
    {
        base.SetupDrawing(c, text);

        ShaderProgram? shader = _sdfShader?.Shader;
        if (shader == null) return;

        c.SetShader(shader);

        if (PixelFont && _sdfReference.AtlasFramebuffer != null)
            shader.SetUniformVector2("scaleFactor", _sdfReference.AtlasFramebuffer.Size);
        else
            shader.SetUniformVector2("scaleFactor", new Vector2(_sdfParam * 2f));

        if (effect == FontEffect.Outline)
        {
            float scaleFactor = 0.5f / _sdfParam;
            float outlineWidthInSdf = effectAmount * scaleFactor;

            // Maximum that can be shown.
            if (outlineWidthInSdf > 0.35f) outlineWidthInSdf = 0.35f;

            shader.SetUniformFloat("outlineWidthDist", outlineWidthInSdf);
            shader.SetUniformColor("outlineColor", effectColor.GetValueOrDefault());
        }
        else
        {
            shader.SetUniformFloat("outlineWidthDist", 0);
        }
    }

    public override void DrawGlyph(Renderer c, DrawableGlyph g, Vector3 pos, Color color)
    {
        c.RenderSprite(pos - _glyphPadding.ToVec3(), g.GlyphUV.Size * _renderScaleRatio + _glyphPaddingSize, color, _sdfReference.AtlasFramebuffer?.ColorAttachment, g.GlyphUV);
    }

    public override void FinishDrawing(Renderer c)
    {
        base.FinishDrawing(c);

        if (_sdfShader != null) c.SetShader();
    }
}