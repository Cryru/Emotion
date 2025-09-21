#nullable enable

namespace Emotion.Game.Systems.UI;

public class UIDropDown : UIBaseWindow
{
    public object? OwningObject = null;

    public UIBaseWindow SpawningWindow { get; init; }

    public UIDropDown(UIBaseWindow spawningWindow)
    {
        SpawningWindow = spawningWindow;
        HandleInput = true;
        RelativeTo = SPECIAL_WIN_ID_DROPDOWN;
        OrderInParent = 99;
        OverlayWindow = true;

        GrowX = false;
        GrowY = false;
    }

    protected override bool RenderInternal(Renderer c)
    {
        return base.RenderInternal(c);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        if (controller.DropDown != null)
            controller.DropDown.Close();

        controller.SetInputFocus(this);
        controller.DropDown = this;
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        if (controller.DropDown == this) controller.DropDown = null;
    }

    // Close on defocus logic is implemented in the UISystem
}