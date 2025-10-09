#nullable enable

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

    protected override void InternalCustomLayout()
    {
        if (AttachedTo != null)
            CalculatedMetrics.Position = GetAnchorPosition(Layout.ParentAnchor, AttachedTo.CalculatedMetrics.Bounds, Layout.Anchor, CalculatedMetrics.Size);

        DefaultLayout();
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
        Assert(Engine.UI.Dropdown == this); // Dropdowns should always be opened by the system and managed.
    }

    protected override void OnClose()
    {
        base.OnClose();
        Engine.UI.CloseDropdown(); // In case the dropdown was closed by the parent dying or something.
    }

    // Close on defocus logic is implemented in the UISystem
}