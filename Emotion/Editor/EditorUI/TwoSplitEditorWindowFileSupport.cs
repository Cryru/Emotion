﻿using Emotion.Game.Systems.UI;

#nullable enable

namespace Emotion.Editor.EditorUI;

public abstract class TwoSplitEditorWindowFileSupport<TLeft, TRight, TEditType> : EditorWindowFileSupport<TEditType>
    where TLeft : UIBaseWindow
    where TRight : UIBaseWindow
{
    public float StartingSplit = 0.5f;

    protected TLeft? _leftContent;
    protected TRight? _rightContent;

    protected TwoSplitEditorWindowFileSupport(string title) : base(title)
    {
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow contentParent = GetContentParent();
        var content = new UIBaseWindow()
        {

        };
        contentParent.AddChild(content);

        var contentBg = new UISolidColor()
        {
            WindowColor = new Color(0, 0, 0, 50),
            BackgroundWindow = true
        };
        content.AddChild(contentBg);

        var contentPanel = new UIBaseWindow
        {
            MinSize = new Vector2(100),
            Paddings = new Rectangle(5, 5, 5, 5),
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        content.AddChild(contentPanel);

        _leftContent = GetLeftSideContent();
        contentPanel.AddChild(_leftContent);

        contentPanel.AddChild(new HorizontalPanelSeparator()
        {
            SeparationPercent = StartingSplit
        });

        _rightContent = GetRightSideContent();
        contentPanel.AddChild(_rightContent);
    }

    protected abstract TLeft GetLeftSideContent();

    protected abstract TRight GetRightSideContent();
}
