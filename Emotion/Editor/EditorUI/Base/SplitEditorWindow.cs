#nullable enable

using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.Components.One;

namespace Emotion.Editor.EditorUI.Base;

public class SplitEditorWindow : EditorWindow
{
    private UIBaseWindow[] _split;

    public SplitEditorWindow(int splitCount, string header) : base(header)
    {
        _split = new UIBaseWindow[splitCount];

        UIBaseWindow contentParent = GetContentParent();
        contentParent.Layout.LayoutMethod = UILayoutMethod.HorizontalList(0);
        contentParent.Layout.OverflowX = UIOverflow.Visible;
        contentParent.Layout.OverflowY = UIOverflow.Visible;

        for (int i = 0; i < splitCount; i++)
        {
            var panel = new EditorPanel();
            if (i != 0)
                contentParent.AddChild(new OneHorizontalPanelSeparator());
            contentParent.AddChild(panel);
            _split[i] = panel;
        }
    }

    public UIBaseWindow GetSplitContentParent(int splitIdx)
    {
        return _split[splitIdx];
    }
}
