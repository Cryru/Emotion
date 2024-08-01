#region Using

using System.Threading.Tasks;
using Emotion.Game.Text;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Text;
using Emotion.Graphics.Text.EmotionSDF;
using Emotion.IO;
using Emotion.WIPUpdates.TextUpdate;

#endregion

#nullable enable

namespace Emotion.UI;

public class UIRichText : UIBaseWindow
{
    /// <summary>
    /// The name of the font asset.
    /// </summary>
    public string FontFile
    {
        get => _fontFileName;
        set
        {
            _fontFileName = value;
            InvalidateLoaded();
        }
    }

    private string _fontFileName = FontAsset.DefaultBuiltInFontName;

    /// <summary>
    /// The unscaled font size of the text.
    /// </summary>
    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            InvalidateLoaded();
        }
    }

    private int _fontSize = 10;

    /// <summary>
    /// Whether to smoothen the drawing of the font by using bilinear filtering.
    /// </summary>
    public bool Smooth = true;

    /// <summary>
    /// Whether the font is a pixel font and we want it to scale integerly.
    /// </summary>
    public bool FontSizePixelPerfect;

    /// <summary>
    /// The text to display.
    /// </summary>
    public virtual string? Text
    {
        get => _text;
        set
        {
            value ??= "";
            if (_text == value) return;
            StringIsTranslated = false;
            _text = value;
            InvalidateLayout();
        }
    }

    public virtual string? TranslatedText
    {
        get => _text;
        set
        {
            value ??= "";
            if (_text == value) return;
            StringIsTranslated = true;
            _text = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Whether the string was set via TranslatedText
    /// </summary>
    public bool StringIsTranslated { get; private set; }

    /// <summary>
    /// The context of the string - this is used to resolve tags.
    /// </summary>
    public StringContext StringContext;

    /// <summary>
    /// Text shadow to draw, if any.
    /// </summary>
    public Color? TextShadow { get; set; }

    /// <summary>
    /// Used to modify the way the height of glyphs are measured and rendered.
    /// Used in producing better looking text when it is expected to span a single line, or positioned above/below/centered on
    /// something.
    /// </summary>
    public GlyphHeightMeasurement TextHeightMode = GlyphHeightMeasurement.FullHeight;

    /// <summary>
    /// The offset of the shadow from the text.
    /// </summary>
    public Vector2 ShadowOffset = new Vector2(2, 2);

    /// <summary>
    /// Whether to underline.
    /// </summary>
    public bool Underline;

    /// <summary>
    /// The offset of the underline.
    /// </summary>
    public Vector2 UnderlineOffset = Vector2.Zero;

    /// <summary>
    /// The thickness of the underline.
    /// </summary>
    public float UnderlineThickness = 0.5f;

    public bool WrapText = true;

    public float OutlineSize;

    public Color OutlineColor;

    protected string _text = string.Empty;
    protected FontAsset? _fontFile;
    protected DrawableFontAtlas? _atlas;
    protected Vector2 _scaledUnderlineOffset;
    protected float _scaledUnderlineThickness;

    protected TextLayoutEngine _layoutEngine = new TextLayoutEngine();

    public UIRichText()
    {
        FillX = false;
        FillY = false;
        StringContext = new StringContext(InvalidateLayout);
    }

    protected override async Task LoadContent()
    {
        // Load font if not loaded.
        if (_fontFile == null || _fontFile.Name != FontFile || _fontFile.Disposed) _fontFile = await Engine.AssetLoader.GetAsync<FontAsset>(FontFile);
        if (_fontFile == null) return;

        if (FontSize == 0)
        {
            _atlas = null;
            return;
        }

        // Load atlas as well. This one will change based on UI scale.
        // Todo: Split scaled atlas from drawing so that metrics don't need the full thing.
        float scale = GetScale();
        _atlas = _fontFile.GetAtlas((int)MathF.Ceiling(FontSize * scale), FontSizePixelPerfect);
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        if (_fontFile == null || _atlas == null) return Vector2.Zero;

        // Text measure should depend on the text, and not its children.
        Assert(!StretchX);
        Assert(!StretchY);

        float scale = GetScale();
        space = Vector2.Clamp(space, MinSize * scale, MaxSize * scale).Ceiling();

        _scaledUnderlineOffset = UnderlineOffset * scale;
        _scaledUnderlineThickness = UnderlineThickness * scale;

        string text = StringContext.ResolveString(_text, StringIsTranslated);
        if (_layoutEngine.NeedsToReRun(text, space.X))
        {
            _layoutEngine.InitializeLayout(text, TextHeightMode);
            if (WrapText)
                _layoutEngine.SetWrap(space.X);
            else
                _layoutEngine.SetWrap(null);
            _layoutEngine.SetDefaultAtlas(_atlas);
            _layoutEngine.Run();

            if (_atlas is EmotionSDFDrawableFontAtlas emSdf)
                _cachedRenderOffset = emSdf.GetDrawOffset().Ceiling();

            _cachedTextRender ??= new VirtualTextureForRichText(this);
            _cachedTextRender.SetVirtualSize(new Vector2(_layoutEngine.TextSize.X, _layoutEngine.TextRenderHeight) + _cachedRenderOffset.ToVec2() * 2f);
            _cachedTextRender.UpVersion();
        }

        return _layoutEngine.TextSize;
    }

#if !NEW_UI
    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return InternalMeasure(space);
    }
#endif

    protected override bool RenderInternal(RenderComposer c)
    {
        if (string.IsNullOrEmpty(_text) || _fontFile == null) return true;

        bool batched = _cachedTextRender != null && c.RenderStream.AttemptToBatchVirtualTexture(_cachedTextRender);
        if (batched)
            c.RenderSprite(Position - _cachedRenderOffset + _layoutEngine.LayoutRenderOffset, _cachedTextRender!.Size, _cachedTextRender);
        else
            _layoutEngine.Render(c, Position, _calculatedColor, OutlineSize > 0 ? FontEffect.Outline : FontEffect.None, OutlineSize * GetScale(), OutlineColor);

        return true;
    }

    #region Atlas Batching

    private class VirtualTextureForRichText : VirtualTextureAtlasTexture
    {
        private UIRichText _textElement;


        public VirtualTextureForRichText(UIRichText textElement)
        {
            _textElement = textElement;
        }

        public override void VirtualTextureRenderToBatch(RenderComposer c, Vector2 offset)
        {
            c.SetAlphaBlend(true);
            _textElement.RenderTextForBatch(c, offset);
        }
    }

    private VirtualTextureForRichText? _cachedTextRender;
    private Vector3 _cachedRenderOffset = new Vector3(1f, 1f, 0);

    protected void RenderTextForBatch(RenderComposer c, Vector2 offset)
    {
        _layoutEngine.RenderNoOffset(c, offset.ToVec3() + _cachedRenderOffset, _calculatedColor, OutlineSize > 0 ? FontEffect.Outline : FontEffect.None, OutlineSize * GetScale(), OutlineColor);
    }

    #endregion
}
