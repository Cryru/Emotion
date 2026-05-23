#nullable enable

namespace Emotion.Editor.EditorUI.Components;

public class EditorPanel : UIBaseWindow
{
    public EditorPanel()
    {
        Layout.Padding = new UISpacing(5, 5, 5, 5);
        //Visuals.Border = 3;
        //Visuals.BorderColor = EditorColorPalette.ButtonColor;
        Layout.OverflowX = UIOverflow.Scroll;
        Layout.OverflowY = UIOverflow.Scroll;
    }

    public Vector2 AbsolutePositionToScreenPosition(Vector2 pos)
    {
        pos += ScrollOffset;
        pos += CalculatedMetrics.Position.ToVec2();
        return pos;
    }
}
