#nullable enable

// ONE

using Emotion.Editor.EditorUI;

namespace Emotion.Game.Systems.UI;

public class UIScrollArea : UIBaseWindow
{
    public UIScrollArea()
    {
        Layout.OverflowX = UIOverflow.Scroll;
        Layout.OverflowY = UIOverflow.Scroll;
        HandleInput = true;
    }

    protected override UIScrollbar? CreateScrollbarVertical()
    {
        return new EditorScrollBar();
    }

    protected override UIScrollbar? CreateScrollbarHorizontal()
    {
        return new EditorScrollBarHorizontal();
    }

    // Backward-compat wrappers
    public void AddChildInside(UIBaseWindow win)
    {
        AddChild(win);
    }

    public void ClearChildrenInside()
    {
        ClearChildren();
    }
}
