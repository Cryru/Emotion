#region Using

using Emotion.Game.Systems.UI;

#endregion

namespace Emotion.Editor.EditorUI;

#nullable enable

public class EditorScrollBar : UIScrollbar
{
    public EditorScrollBar()
    {
        DefaultSelectorColor = EditorColorPalette.ButtonColor;
        SelectorMouseInColor = EditorColorPalette.ActiveButtonColor;
        WindowColor = Color.Black * 0.5f;
        AnchorAndParentAnchor = UIAnchor.TopRight;
        MinSizeX = 15;
        MaxSizeX = 15;
    }

    protected override bool RenderInternal(Renderer c)
    {
        base.RenderInternal(c);

        var grabLineColor = Color.White * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 10 * scale;
        var lineHeight = 3 * scale;

        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(0, lineHeight * 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(0, lineHeight * 4f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);

        return true;
    }
}

public class EditorScrollBarHorizontal : UIScrollbar
{
    public EditorScrollBarHorizontal()
    {
        DefaultSelectorColor = EditorColorPalette.ButtonColor;
        SelectorMouseInColor = EditorColorPalette.ActiveButtonColor;
        WindowColor = Color.Black * 0.5f;
        AnchorAndParentAnchor = UIAnchor.BottomLeft;
        MinSizeY = 15;
        MaxSizeY = 15;
        Horizontal = true;
    }

    protected override bool RenderInternal(Renderer c)
    {
        base.RenderInternal(c);

        var grabLineColor = Color.White * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 3 * scale;
        var lineHeight = 10 * scale;

        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 2f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 4f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);

        return true;
    }
}