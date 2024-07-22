using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.UI;

public class UIRollover : UIBaseWindow
{
    public static float RolloverPadding = 5;

    public UIRollover()
    {
        RelativeTo = SPECIAL_WIN_ID_MOUSE_FOCUS;
        StretchX = true;
        StretchY = true;

        Priority = 99;
    }

    protected override Vector2 BeforeLayout(Vector2 contentPos)
    {
        float paddingScaled = RolloverPadding * GetScale();

        Rectangle controllerBox = Controller.Bounds;
        controllerBox.X += paddingScaled;
        controllerBox.Y += paddingScaled;
        controllerBox.Width -= paddingScaled * 2;
        controllerBox.Height -= paddingScaled * 2;

        Rectangle myBounds = new Rectangle(contentPos, Size);
        return controllerBox.SnapRectangleInside(myBounds);
    }
}
