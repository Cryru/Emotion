using Emotion.UI;
using Emotion.WIPUpdates.One.Tools;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI;

public abstract class TwoSplitEditorWindowFileSupport<TLeft, TRight, TEditType> : EditorWindowFileSupport<TEditType>
    where TLeft : UIBaseWindow
    where TRight : UIBaseWindow
{
    public float StartingSplit = 0.6f;

    protected TLeft? _leftContent;
    protected TRight? _rightContent;

    protected TwoSplitEditorWindowFileSupport(string title) : base(title)
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

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
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
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
