#nullable enable

namespace Emotion.Game.Systems.UI2;

public class O_UISystem : O_UIBaseWindow
{
    public O_UISystem()
    {
        Name = "UISystem";
    }

    public override void Update()
    {
        foreach (O_UIBaseWindow child in Children)
        {
            UpdateLayoutIfNeeded();
        }
    }

    #region Layout

    public void UpdateLayoutIfNeeded()
    {
        //if (!_layoutDirty) return;
        SystemDoLayout(UILayoutPass.Measure);
        SystemDoLayout(UILayoutPass.Grow);
        SystemDoLayout(UILayoutPass.Layout);
    }

    protected void SystemDoLayout(UILayoutPass pass)
    {
        switch (pass)
        {
            case UILayoutPass.Measure:
                CalculatedMetrics.Size = MeasureWindow();
                break;
            case UILayoutPass.Grow:
                GrowWindow();
                break;
            case UILayoutPass.Layout:
                LayoutWindow(Vector2.Zero);
                break;
        }
    }


    #endregion

    #region System

    protected override Vector2 InternalGetWindowMinSize()
    {
        return Engine.Renderer.ScreenBuffer.Size;
    }

    #endregion
}