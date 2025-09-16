#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.IO;
using Emotion.Game.Systems.UI.Text;
using Emotion.Graphics.Text;

#endregion

namespace Emotion.Game.Systems.UI;

public class UIText : UIBaseWindow
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
    public virtual string Text
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

    public float OutlineSize;

    public Color OutlineColor;

    protected string _text;
    protected FontAsset _fontFile;
    protected DrawableFontAtlas _atlas;
    protected TextLayouterWrap _layouter;
    protected DrawableFontAtlas _layouterAtlas;
    protected Vector2 _scaledUnderlineOffset;
    protected float _scaledUnderlineThickness;

    public UIText()
    {
        GrowX = false;
        GrowY = false;
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
        float scale = Engine.UI.GetScale();// GetScale();
        _atlas = _fontFile.GetAtlas((int) MathF.Ceiling(FontSize * scale), FontSizePixelPerfect);

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

        _layouter.Restart();
        if (string.IsNullOrEmpty(_text))
        {
            if (TextHeightMode == GlyphHeightMeasurement.FullHeight)
            {
                return new Vector2(0, _atlas.FontHeight);
            }
            else
            {
                _layouter.SetupBox("", space, TextHeightMode);
                return new Vector2(_layouter.NeededWidth, _layouter.NeededHeight);
            }
        }

        _layouter.SetupBox(_text, space, TextHeightMode);
        return new Vector2(_layouter.NeededWidth, _layouter.NeededHeight);
    }

#if !NEW_UI
    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return InternalMeasure(space);
    }
#endif

    protected override bool RenderInternal(Renderer c)
    {
        if (_text == null || _fontFile == null || _layouter == null) return true;

        if (TextShadow != null)
        {
            _layouter.RestartPen();
            c.RenderString(Position + ShadowOffset.ToVec3(), TextShadow.Value * _calculatedColor.A, _text, _atlas, _layouter);
        }

        if (Underline)
        {
            float y = Y + Height + _scaledUnderlineOffset.Y;
            c.RenderLine(new Vector3(X, y, Z), new Vector3(X + Width, y, Z), _calculatedColor, _scaledUnderlineThickness);
        }

        _layouter.RestartPen();
        if (OutlineSize > 0)
            c.RenderString(Position, _calculatedColor, _text, _atlas, _layouter, FontEffect.Outline, OutlineSize * GetScale(), OutlineColor);
        else
            c.RenderString(Position, _calculatedColor, _text, _atlas, _layouter);

        return true;
    }
}