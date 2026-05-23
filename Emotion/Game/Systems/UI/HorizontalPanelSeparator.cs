#nullable enable

using Emotion.Core.Platform;
using Emotion.Core.Platform.Implementation.CommonDesktop;

namespace Emotion.Game.Systems.UI;

public class HorizontalPanelSeparator : UIBaseWindow
{
    public float SeparationPercent = 0.5f;

    public HorizontalPanelSeparator()
    {
        HandleInput = true;
    }

    protected override void MouseInsideChanged(bool inside, Vector2 mousePos)
    {
        PlatformBase host = Engine.Host;
        if (host is DesktopPlatform desktopPlatform)
            desktopPlatform.SetCursor(inside ? DesktopPlatform.MouseCursor.ResizeLR : DesktopPlatform.MouseCursor.Default);
        base.MouseInsideChanged(inside, mousePos);
    }
}
