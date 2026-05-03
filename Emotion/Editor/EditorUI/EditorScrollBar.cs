#nullable enable

namespace Emotion.Editor.EditorUI;

public class EditorScrollBar : UIScrollbar
{
    public EditorScrollBar() : base(false)
    {
        DefaultSelectorColor = EditorColorPalette.ButtonColor;
        SelectorMouseInColor = EditorColorPalette.ActiveButtonColor;
        
        Layout.SizingX = UISizing.Fixed(15);
        Visuals.BackgroundColor = Color.Black * 0.5f;
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);

        var grabLineColor = Color.White * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 10 * scale;
        var lineHeight = 3 * scale;

        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(0, lineHeight * 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(0, lineHeight * 4f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
    }
}

public class EditorScrollBarHorizontal : UIScrollbar
{
    public EditorScrollBarHorizontal() : base(true)
    {
        DefaultSelectorColor = EditorColorPalette.ButtonColor;
        SelectorMouseInColor = EditorColorPalette.ActiveButtonColor;
        Horizontal = true;

        Layout.SizingY = UISizing.Fixed(15);
        Visuals.BackgroundColor = Color.Black * 0.5f;
    }

    protected override void InternalRender(Renderer r)
    {
        base.InternalRender(r);

        var grabLineColor = Color.White * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 3 * scale;
        var lineHeight = 10 * scale;

        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 2f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        r.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 4f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
    }
}