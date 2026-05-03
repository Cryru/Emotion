#nullable enable


namespace Emotion.Editor.EditorUI.Components;

public class NewUIScrollArea : UIBaseWindow
{
    public NewUIScrollArea()
    {
        Layout.SizingX = UISizing.Grow();
        Layout.SizingY = UISizing.Grow();
        Layout.OverflowX = UIOverflow.Scroll;
        Layout.OverflowY = UIOverflow.Scroll;
    }

    public Vector2 AbsolutePositionToScreenPosition(Vector2 pos)
    {
        pos += ScrollOffset;
        pos += CalculatedMetrics.Position.ToVec2();
        return pos;
    }

    // Backward-compat
    public void AddChildInside(UIBaseWindow win)
    {
        AddChild(win);
    }
}

public class EditorPanel : NewUIScrollArea
{
    public EditorPanel()
    {
        Layout.Padding = new UISpacing(5, 5, 5, 5);
        Visuals.Border = 3;
        Visuals.BorderColor = EditorColorPalette.ButtonColor;
    }
}
