#nullable enable

using Emotion.Game.Systems.UI2;

namespace Emotion.Game.Systems.UI;

public class UIDropDown : UIBaseWindow
{
    public UIDropDown()
    {
        HandleInput = true;
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
        //RelativeTo = SPECIAL_WIN_ID_DROPDOWN;
        OrderInParent = 99;
        //OverlayWindow = true;

        //GrowX = false;
        //GrowY = false;
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        //if (controller.DropDown != null)
        //    controller.DropDown.Close();

        //controller.SetInputFocus(this);
        //controller.DropDown = this;
    }

    protected override void OnClose()
    {
        base.OnClose();
        //if (controller.DropDown == this) controller.DropDown = null;
    }

    // Close on defocus logic is implemented in the UISystem
}