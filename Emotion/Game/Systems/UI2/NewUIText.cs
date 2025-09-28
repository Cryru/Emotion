#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI.Text;
using Emotion.Game.Systems.UI.Text.TextUpdate;
using Emotion.Graphics.Text;

namespace Emotion.Game.Systems.UI2;

public class NewUIText : UIBaseWindow
{
    public AssetOrObjectReference<FontAsset, FontAsset> Font = FontAsset.DefaultBuiltInFontName;
    private AssetOrObjectReference<FontAsset, FontAsset>? _loadedFont = null;

    public Color TextColor = Color.White;

    /// <summary>
    /// The unscaled font size of the text.
    /// </summary>
    public int FontSize
    {
        get => _fontSize;
        set
        {
            _fontSize = value;
            InvalidateAssets();
        }
    }

    private int _fontSize = 10;

    #region Text Set

    public virtual string Text
    {
        get => _text;
        set
        {
            value ??= string.Empty;
            if (_text == value) return;
            StringIsTranslated = false;
            _text = value;
            InvalidateLayout();
        }
    }

    protected string _text = string.Empty;

    public virtual string TranslatedText
    {
        get => _text;
        set
        {
            value ??= string.Empty;
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

    #endregion

    #region Shadow

    /// <summary>
    /// Text shadow to draw, if any.
    /// </summary>
    public Color? TextShadow { get; set; }

    /// <summary>
    /// The offset of the shadow from the text.
    /// </summary>
    public Vector2 ShadowOffset = new Vector2(2, 2);

    #endregion

    #region Underline

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

    #endregion

    #region Effects

    public float OutlineSize;

    public Color OutlineColor;

    #endregion

    /// <summary>
    /// Used to modify the way the height of glyphs are measured and rendered.
    /// Used in producing better looking text when it is expected to span a single line, or positioned above/below/centered on
    /// something.
    /// </summary>
    public GlyphHeightMeasurement TextHeightMode = GlyphHeightMeasurement.FullHeight;

    public bool WrapText
    {
        get => _wrapText;
        set
        {
            if (_wrapText == value) return;
            _wrapText = value;
            InvalidateLayout();
        }
    }

    private bool _wrapText = true;

    public bool AllowRenderBatch = true;

    protected DrawableFontAtlas? _atlas;
    protected TextLayoutEngine _layoutEngine = new TextLayoutEngine();

    public NewUIText()
    {
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
    }

    protected override void OnClose()
    {
        base.OnClose();

        // Dispose of the loaded asset
        if (_loadedFont != null)
        {
            _loadedFont.Cleanup();
            _loadedFont = null;
        }
    }

    #region Loading

    protected override Coroutine? InternalLoad()
    {
        if (Font.ReadyToUse() || !Font.IsValid())
            return null;

        // Dispose of the old one.
        _loadedFont?.Cleanup();
        _loadedFont = null;
        return Engine.CoroutineManager.StartCoroutine(LoadAsset());
    }

    private IEnumerator LoadAsset()
    {
        yield return 5000;
        yield return Font.PerformLoading(this, ProxyInvalidateLayout);
        _loadedFont = Font;
        InvalidateLayout();
    }

    #endregion

    protected override IntVector2 InternalGetWindowMinSize()
    {
        if (string.IsNullOrEmpty(Text))
        {
            return IntVector2.Zero;
        }

        FontAsset font = Font.GetObject() ?? FontAsset.GetDefaultBuiltIn();

        float atlasSize = (int)MathF.Ceiling(FontSize * CalculatedMetrics.ScaleF);
        _atlas = font.GetAtlas(atlasSize);

        if (_layoutEngine.NeedsToReRun(Text, null, _atlas))
        {
            _layoutEngine.SetWrap(null);
            _layoutEngine.SetDefaultAtlas(_atlas);
            _layoutEngine.InitializeLayout(Text, TextHeightMode);
            _layoutEngine.Run();
        }

        return IntVector2.FromVec2Ceiling(_layoutEngine.TextSize); // Is this TextSize IntVector2?
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);
        if (string.IsNullOrEmpty(Text)) return;

        //_layoutEngine.Render(r, pos, _calculatedColor, OutlineSize > 0 ? FontEffect.Outline : FontEffect.None, OutlineSize * GetScale(), OutlineColor);
        Vector3 pos = CalculatedMetrics.Position.ToVec3();
        _layoutEngine.Render(r, pos, TextColor, OutlineSize > 0 ? FontEffect.Outline : FontEffect.None, OutlineSize * GetScale(), OutlineColor);
    }
}
