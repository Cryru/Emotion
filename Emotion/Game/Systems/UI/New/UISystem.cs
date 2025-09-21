#nullable enable

using Emotion.Core.Systems.Input;
using Emotion.Core.Systems.Logging;

namespace Emotion.Game.Systems.UI.New;

// todo: add to Scenes
// todo: scenes - make current scene current even while loading, add loaded bool
// todo: investigate loading exceptions for the 100th time

public class UISystem : UIBaseWindow
{
    public Vector2 TargetResolution = new Vector2(1920, 1080);
    public Vector2 TargetDPI = new Vector2(96);

    public UISystem()
    {
        State = UIWindowState.Open;
        Engine.Host.OnResize += HostResized;
        HostResized(Engine.Renderer.ScreenBuffer.Size);
    }

    private void HostResized(Vector2 size)
    {
        Vector2 scaledTarget = TargetResolution * TargetDPI;
        Vector2 scaledCurrent = size * Engine.Host.GetDPI();

        Layout.Scale = scaledCurrent / scaledTarget;
        Engine.Log.Info($"UI Scale is {Layout.Scale}", MessageSource.UI);
    }

    protected override Vector2 InternalGetWindowMinSize()
    {
        return Engine.Renderer.ScreenBuffer.Size;
    }

    protected override void RenderChildren(Renderer c)
    {
        //c.EnableSpriteBatcher(true);
        base.RenderChildren(c);
        //c.EnableSpriteBatcher(false);

        if (_debugInspectMode && _debugWindowsUnderMouse!.Count > 0)
        {
            var windowUnderMouse = _debugWindowsUnderMouse[^1];
            c.RenderRectOutline(windowUnderMouse.Position, windowUnderMouse.Size, Color.Red);
        }
    }

    //protected override void UpdateMouseFocus()
    //{
    //    base.UpdateMouseFocus();

    //    if (_debugInspectMode) DebugInspectModeUpdate();
    //}

    //protected override bool MouseFocusOnKey(Key key, KeyState status)
    //{
    //    if (_debugInspectMode && key == Key.MouseKeyLeft && status == KeyState.Down)
    //    {
    //        _debugInspectMode = false;
    //        return false;
    //    }
    //    return base.MouseFocusOnKey(key, status);
    //}

    #region Input

    public void SetInputFocus(UIBaseWindow window)
    {

    }

    #endregion

    #region Debugger

    private bool _debugInspectMode = false;
    private List<UIBaseWindow>? _debugWindowsUnderMouse;

    public void EnterInspectMode()
    {
        _debugInspectMode = true;
        _debugWindowsUnderMouse = new List<UIBaseWindow>();
    }

    private void DebugInspectModeUpdate()
    {
        //Debug_GetWindowsUnderMouse(_debugWindowsUnderMouse);
    }

    public List<UIBaseWindow>? GetInspectModeSelectedWindow()
    {
        if (!_debugInspectMode) return null;
        return _debugWindowsUnderMouse;
    }

    #endregion
}

