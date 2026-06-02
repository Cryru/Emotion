#nullable enable

using Emotion.Core.Platform;

#if DESKTOP
using Emotion.Core.Platform.Implementation.CommonDesktop;
#endif

namespace Emotion.Game.Systems.UI;

public class HorizontalPanelSeparator : UIBaseWindow
{
    public const float MinSeparationPercent = 0.1f;
    public const float AutoSeparationPercent = 0f;

    public float SeparationPercent
    {
        get
        {
            return _separationPercent;
        }
        set
        {
            value = Math.Clamp(value, MinSeparationPercent, 1f - MinSeparationPercent);
            if (MathF.Abs(_separationPercent - value) <= float.Epsilon)
                return;

            _separationPercent = value;
            Parent?.InvalidateLayout();
        }
    }

    // todo: maybe all windows should keep track of their own grid column for input purposes or something?
    public int CalculatedMetrics_SeparatorColumn;

    private float _separationPercent = AutoSeparationPercent;
    private bool _dragging = false;

    public HorizontalPanelSeparator()
    {
        HandleInput = true;
        Layout.SizingX = UISizing.Fixed(6);
        Layout.SizingY = UISizing.Grow();
        Layout.ScaleType = ScaleType.UniformScale;
        Visuals.BackgroundColor = Color.Black;
    }

    protected override void MouseInsideChanged(bool inside, Vector2 mousePos)
    {
#if DESKTOP
        PlatformBase host = Engine.Host;
        if (host is DesktopPlatform desktopPlatform)
            desktopPlatform.SetCursor(inside ? DesktopPlatform.MouseCursor.ResizeLR : DesktopPlatform.MouseCursor.Default);
#endif
        base.MouseInsideChanged(inside, mousePos);
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            _dragging = status == KeyState.Down;
            return false;
        }

        return base.OnKey(key, status, mousePos);
    }

    public override void OnMouseMove(Vector2 mousePos)
    {
        base.OnMouseMove(mousePos);
        if (!_dragging) return;

        IntRectangle parentViewport = Parent.CalculatedMetrics.GetViewportRect();
        if (parentViewport.Width <= 0) return;

        float mousePercent = (mousePos.X - parentViewport.X) / parentViewport.Width;
        SeparationPercent = mousePercent;
    }
}
