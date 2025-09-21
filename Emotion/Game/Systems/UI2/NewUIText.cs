#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI.Text.TextUpdate;
using Emotion.Graphics.Text;

namespace Emotion.Game.Systems.UI2;

public class NewUIText : UIBaseWindow
{
    public AssetOrObjectReference<FontAsset, FontAsset> Font = FontAsset.DefaultBuiltInFontName;
    private AssetOrObjectReference<FontAsset, FontAsset>? _loadedFont = null;

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

    /// <summary>
    /// The text to display.
    /// </summary>
    public string? Text
    {
        get => _text;
        set
        {
            _text = value;
            InvalidateLayout();
        }
    }

    private string? _text = string.Empty;

    public Color TextColor = Color.White;

    protected DrawableFontAtlas? _atlas;
    protected TextLayoutEngine _layoutEngine = new TextLayoutEngine();

    public NewUIText()
    {
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
        yield return Font.PerformLoading(this, ProxyInvalidateLayout);
        _loadedFont = Font;
        InvalidateLayout();
    }

    private static void ProxyInvalidateLayout(object? owner)
    {
        if (owner is UIBaseWindow win)
            win.InvalidateLayout();
    }

    #endregion

    protected override Vector2 InternalGetWindowMinSize()
    {
        if (string.IsNullOrEmpty(Text))
        {
            return Vector2.Zero;
        }

        FontAsset font = Font.GetObject() ?? FontAsset.GetDefaultBuiltIn();
        _atlas = font.GetAtlas(FontSize);

        if (_layoutEngine.NeedsToReRun(Text, null, _atlas))
        {
            _layoutEngine.SetWrap(null);
            _layoutEngine.SetDefaultAtlas(_atlas);
            _layoutEngine.InitializeLayout(Text);             //_layoutEngine.InitializeLayout(text, TextHeightMode);
            _layoutEngine.Run();
        }

        return _layoutEngine.TextSize;
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);
        if (string.IsNullOrEmpty(Text)) return;

        //_layoutEngine.Render(r, pos, _calculatedColor, OutlineSize > 0 ? FontEffect.Outline : FontEffect.None, OutlineSize * GetScale(), OutlineColor);
        Vector3 pos = CalculatedMetrics.Position.ToVec3();
        _layoutEngine.Render(r, pos, TextColor);
    }
}
