#nullable enable

namespace Emotion.UI;

public class UIDropDown : UIBaseWindow
{
    public object? OwningObject = null;

    public UIDropDown()
    {
        CodeGenerated = true;
        HandleInput = true;
        Priority = 99;

        FillX = false;
        FillY = false;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        controller.SetInputFocus(this);
        controller.DropDown = this;
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        if (controller.DropDown == this) controller.DropDown = null;
    }

    public override void InputFocusChanged(bool haveFocus)
    {
        base.InputFocusChanged(haveFocus);

        if (!haveFocus) Parent?.RemoveChild(this);
    }
}