#nullable enable

using Emotion.Game.Systems.UI2;

namespace Emotion.Game.Systems.UI;

[DontSerialize]
public class UIAttachedWindow : UIBaseWindow
{
    public UIBaseWindow? AttachedTo;

    public UIAttachedWindow()
    {
        _useCustomLayout = true;
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
    }

    protected virtual IntVector2 InternalMeasureWindow()
    {
        return base.MeasureWindow();
    }

    protected override void InternalCustomLayout()
    {
        PreLayout();
        CalculatedMetrics.Size = InternalMeasureWindow();
        GrowWindow();

        if (AttachedTo == null)
        {
            CalculatedMetrics.Position = IntVector2.Zero;
            return;
        }

        CalculatedMetrics.InsideParent = AnchorsInsideParent(Layout.ParentAnchor, Layout.Anchor);

        IntVector2 pen = AttachedTo.CalculatedMetrics.Position;
        IntVector2 parentSize = AttachedTo.CalculatedMetrics.Size;
        IntRectangle parentContentRect = new IntRectangle(pen, parentSize);
        IntVector2 anchorPos = GetAnchorPosition(Layout.ParentAnchor, parentContentRect, Layout.Anchor, CalculatedMetrics.Size);
        LayoutWindow(anchorPos);
    }
}

public class UIDropDown : UIAttachedWindow
{
    public UIDropDown()
    {
        HandleInput = true;
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
        OrderInParent = 99;
        //OverlayWindow = true;
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        Assert(Engine.UI.Dropdown == this);
    }

    protected override void OnClose()
    {
        base.OnClose();
        Engine.UI.CloseDropdown();
    }

    // Close on defocus logic is implemented in the UISystem
}