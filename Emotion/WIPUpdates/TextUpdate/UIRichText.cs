#region Using

using System.Threading.Tasks;
using Emotion.Game.Text;
using Emotion.Graphics.Text;
using Emotion.IO;

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
            _text = value;
            InvalidateLayout();
        }
    }

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

    protected string _text;
    protected FontAsset _fontFile;
    protected DrawableFontAtlas _atlas;
    protected TextLayouterWrap _layouter;
    protected DrawableFontAtlas _layouterAtlas;
    protected Vector2 _scaledUnderlineOffset;
    protected float _scaledUnderlineThickness;

    protected TextLayoutEngine _layoutEngine = new TextLayoutEngine();

    public UIRichText()
    {
        FillX = false;
        FillY = false;
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

        // Reload the layouter if needed. Changing this means the text needs to be relayouted.
        if (_layouterAtlas != _atlas)
        {
            _layouter = new TextLayouterWrap(_atlas);
            _layouterAtlas = _atlas;
            InvalidateLayout();
        }
    }

    protected override Vector2 InternalMeasure(Vector2 space)
    {
        if (_fontFile == null || _layouter == null) return Vector2.Zero;

        // Text measure should depend on the text, and not its children.
        Assert(!StretchX);
        Assert(!StretchY);

        float scale = GetScale();
        space = Vector2.Clamp(space, MinSize * scale, MaxSize * scale).Ceiling();

        _scaledUnderlineOffset = UnderlineOffset * scale;
        _scaledUnderlineThickness = UnderlineThickness * scale;

        _layoutEngine.InitializeLayout(_text, TextHeightMode);
        _layoutEngine.SetWrap(space.X);
        _layoutEngine.SetDefaultAtlas(_layouterAtlas);
        _layoutEngine.Run();
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
        if (_text == null || _fontFile == null || _layouter == null) return true;

        c.RenderOutline(Position, Size, Color.Red);
        _layoutEngine.Render(c, Position, _calculatedColor);

        return true;
    }
}
