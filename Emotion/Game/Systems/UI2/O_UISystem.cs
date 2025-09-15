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
        Name = "UISystem";
    }

    public override void Update()
    {
        foreach (O_UIBaseWindow child in Children)
        {
            UpdateLayoutIfNeeded();
        }
    }

    #region System

    protected override Vector2 InternalGetWindowMinSize()
    {
        return Engine.Renderer.ScreenBuffer.Size;
    }

    #endregion
}