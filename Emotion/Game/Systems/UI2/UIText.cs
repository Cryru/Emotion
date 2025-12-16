#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Graphics.Text;
using Emotion.Standard.Parsers.OpenType;

namespace Emotion.Game.Systems.UI2;

public class UIText : UIBaseWindow
{
    public AssetObjectReference<FontAsset, Font> Font
    {
        get => _assetOwner.GetCurrentReference();
        set
        {
            _assetOwner.Set(value);
            InvalidateLoaded();
        }
    }
    private AssetOwner<FontAsset, Font> _assetOwner = new();

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

    private int _fontSize = 20;

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

    public TextEffect? Effect;

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

    protected TextLayouter _layouter;

    public UIText()
    {
        _layouter = CreateTextLayouter();

        _assetOwner.Set(FontAsset.GetDefaultBuiltIn(), true);
        _assetOwner.SetOnChangeCallback(ProxyInvalidateLayout, this);
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
    }

    protected virtual TextLayouter CreateTextLayouter()
    {
        return new TextLayouter();
    }

    protected override void OnClose()
    {
        base.OnClose();
        _assetOwner.Done();
    }

    protected override Coroutine? InternalLoad()
    {
        return _assetOwner.GetCurrentLoading();
    }

    protected override IntVector2 InternalGetWindowMinSize()
    {
        ReRunLayout();
        return _layouter.Calculated_TotalSize;
    }

    protected void ReRunLayout()
    {
        Font? font = _assetOwner.GetCurrentObject();
        int textSizeScaled = (int)MathF.Ceiling(FontSize * CalculatedMetrics.ScaleF);
        _layouter.RunLayout(Text, textSizeScaled, font, null, TextHeightMode);
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);
        if (string.IsNullOrEmpty(Text)) return;

        Vector3 pos = CalculatedMetrics.Position.ToVec3();
        _layouter.RenderLastLayout(r, pos, TextColor, Effect);
    }
}
