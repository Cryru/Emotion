#region Using

using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

#nullable enable

public class EditorScrollBar : UIScrollbar
{
    public EditorScrollBar()
    {
        DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
        SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
        WindowColor = Color.Black * 0.5f;
        Anchor = UIAnchor.TopRight;
        ParentAnchor = UIAnchor.TopRight;
        MinSize = new Vector2(5, 0);
        MaxSize = new Vector2(5, 9999);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        base.RenderInternal(c);

        var grabLineColor = Color.Black * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 3 * scale;
        var lineHeight = 1 * scale;

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
        DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
        SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
        WindowColor = Color.Black * 0.5f;
        Anchor = UIAnchor.BottomLeft;
        ParentAnchor = UIAnchor.BottomLeft;
        MinSize = new Vector2(0, 5);
        MaxSize = new Vector2(9999, 5);
        Horizontal = true;
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        base.RenderInternal(c);

        var grabLineColor = Color.Black * 0.35f;

        var center = _selectorRect.Center;
        var scale = GetScale();
        var lineWidth = 1 * scale;
        var lineHeight = 3 * scale;

        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 2f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);
        c.RenderSprite((center - new Vector2(lineWidth / 2f, lineHeight / 2f) + new Vector2(lineWidth * 4f, 0)).ToVec3(Z), new Vector2(lineWidth, lineHeight), grabLineColor);

        return true;
    }
}