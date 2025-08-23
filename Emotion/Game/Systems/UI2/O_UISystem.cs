#nullable enable

namespace Emotion.Game.Systems.UI2;

public class O_UISystem : O_UIBaseWindow
{
    public O_UISystem()
    {
        Layout.FitX = false;
        Layout.FitY = false;
        Layout.GrowX = false;
        Layout.GrowY = false;
    }

    public void AddChild(O_UIBaseWindow window)
    {
        Children.Add(window);
        _needLayout = true;
    }

    public override void Update()
    {
        if (_needLayout)
            ApplyLayout();


    }

    #region System

    protected override Vector2 InternalGetWindowMinSize()
    {
        return Engine.Renderer.ScreenBuffer.Size;
    }

    #endregion

    #region Layout

    protected bool _needLayout = true;

    private void ApplyLayout()
    {
        _needLayout = false;

        SystemDoLayout(UILayoutPass.Measure);
        SystemDoLayout(UILayoutPass.Grow);
        SystemDoLayout(UILayoutPass.Layout);
    }

    #endregion
}