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
        OverlayWindow = true;

        OrderInParent = 99;
    }

    protected override void Layout(Vector2 pos, Vector2 size)
    {
        float paddingScaled = RolloverPadding * GetScale();

        Rectangle controllerBox = Controller.Bounds;
        controllerBox.X += paddingScaled;
        controllerBox.Y += paddingScaled;
        controllerBox.Width -= paddingScaled * 2;
        controllerBox.Height -= paddingScaled * 2;

        Rectangle myBounds = new Rectangle(pos, size);
        Vector2 snappedPos = controllerBox.SnapRectangleInside(myBounds);
        base.Layout(snappedPos, size);
    }
}
