#nullable enable


namespace Emotion.Editor.EditorUI.Components;

public class NewUIScrollArea : UIBaseWindow
{
    public bool AutoHideScrollX { get; set; } = true;
    public bool AutoHideScrollY { get; set; } = false;

    private Matrix4x4 _scrollTranslationMatrix = Matrix4x4.Identity;

    public NewUIScrollArea()
    {
        Layout.SizingX = UISizing.Grow();
        Layout.SizingY = UISizing.Grow();
        Layout.ChildrenCanExpand = false;
    }

    protected override void InternalOnLayoutComplete()
    {
        base.InternalOnLayoutComplete();
    }

    protected override void InternalBeforeRenderChildren(Renderer r)
    {
        base.InternalBeforeRenderChildren(r);
    }

    protected override void InternalAfterRenderChildren(Renderer r)
    {
        base.InternalAfterRenderChildren(r);
    }

    protected override void InternalCustomLayout()
    {
        base.InternalCustomLayout();
    }

    #region API

    public Vector2 AbsolutePositionToScreenPosition(Vector2 pos)
    {
        pos = Vector2.Transform(pos, _scrollTranslationMatrix);
        pos += CalculatedMetrics.Position.ToVec2();
        return pos;
    }

    #endregion

    #region To Delete

    public void AddChildInside(UIBaseWindow win)
    {
        AddChild(win);
    }

    #endregion
}

public class EditorScrollArea : NewUIScrollArea
{
    public EditorScrollArea()
    {
        Layout.Padding = new UISpacing(5, 5, 5, 5);
        Visuals.Border = 3;
        Visuals.BorderColor = EditorColorPalette.ButtonColor;
    }
}
