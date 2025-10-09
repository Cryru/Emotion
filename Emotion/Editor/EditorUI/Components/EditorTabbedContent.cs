#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Components;

public class EditorTabbedContent : UIBaseWindow
{
    private UIBaseWindow _tabButtons;
    private UIBaseWindow _tabContent;
    private Dictionary<string, Action<UIBaseWindow>> _tabToBuildFunc = new();

    public EditorTabbedContent()
    {
        Layout.LayoutMethod = UILayoutMethod.VerticalList(0);

        var tabButtonsContainer = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5)
            }
        };
        AddChild(tabButtonsContainer);
        _tabButtons = tabButtonsContainer;

        var tabContent = new UIBaseWindow();
        AddChild(tabContent);
        _tabContent = tabContent;
    }

    public void AddTab(string tabName, Action<UIBaseWindow> buildTabContent)
    {
        _tabToBuildFunc.Add(tabName, buildTabContent);
        _tabButtons.AddChild(new EditorButton(tabName)
        {
            Name = tabName,
            OnClickedProxy = TabButtonClicked
        });
    }

    // todo: RemoveTab

    private void UnmarkAllButtons()
    {
        foreach (UIBaseWindow child in _tabButtons.Children)
        {
            if (child is EditorButton button)
                button.SetActiveMode(false);
        }
    }

    private void TabButtonClicked(UICallbackButton button)
    {
        SetTab(button.Name ?? string.Empty);
    }

    public void SetTab(string tabName)
    {
        UnmarkAllButtons();
        _tabContent.ClearChildren();
        if (_tabToBuildFunc.TryGetValue(tabName, out Action<UIBaseWindow>? buildFunc))
        {
            EditorButton? buttonOfTab = GetWindowById<EditorButton>(tabName);
            buttonOfTab?.SetActiveMode(true);

            buildFunc(_tabContent);
        }
    }
}