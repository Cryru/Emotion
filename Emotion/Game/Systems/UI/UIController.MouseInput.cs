#nullable enable

namespace Emotion.Game.Systems.UI;

/// <summary>
/// Mouse focus is shared between all active controllers.
/// Active controllers are those which have called their update last tick.
/// </summary>
public partial class UIController
{
    /// <summary>
    /// The first non-input transparent visible window in any active controller. Can vary depending on window logic etc.
    /// </summary>
    public static UIBaseWindow? MouseFocus { get; private set; }

    public static UIRollover? CurrentRollover { get; private set; }

    private UIBaseWindow? _myMouseFocus; // The mouse focus of this controller in particular.

    public static void RemoveCurrentRollover()
    {
        CurrentRollover?.Close();
        CurrentRollover = null;
    }

    protected virtual void UpdateMouseFocus()
    {
        if (Engine.Host.HostPaused || !ChildrenHandleInput || !Visible)
        {
            SetControllerMouseFocus(null);
            return;
        }

        UIBaseWindow? hasPriority = HasButtonHeldMouseFocus();
        if (hasPriority != null)
        {
            SetControllerMouseFocus(hasPriority);
            return;
        }

        Vector2 pos = Engine.Host.MousePosition;
        UIBaseWindow? focus = FindMouseInput(pos);
        if (focus == this && _inputFocusManual == null) focus = null;
        SetControllerMouseFocus(focus);
    }

    /// <summary>
    /// If currently holding down a mouse button don't change the mouse focus if it is still valid.
    /// </summary>
    private UIBaseWindow? HasButtonHeldMouseFocus()
    {
        if (_myMouseFocus == null || !_myMouseFocus.VisibleAlongTree() || !_myMouseFocus.HandleInput) return null;

        for (var i = 0; i < _mouseFocusKeysHeld.Length; i++)
        {
            if (_mouseFocusKeysHeld[i]) return _myMouseFocus;
        }

        return null;
    }

    private void SetControllerMouseFocus(UIBaseWindow? newMouseFocus)
    {
        Vector2 mousePos = Engine.Host.MousePosition;
        if (newMouseFocus != _myMouseFocus)
        {
            _myMouseFocus?.OnMouseLeft(mousePos);
            if (_myMouseFocus != null) Engine.Host.OnKey.RemoveListener(_mouseFocusOnKeyDelegateCache);
            _myMouseFocus = newMouseFocus;
            if (_myMouseFocus != null) Engine.Host.OnKey.AddListener(_mouseFocusOnKeyDelegateCache, KeyListenerType.UI);
            _myMouseFocus?.OnMouseEnter(mousePos);
            MouseFocus = newMouseFocus;

            RemoveCurrentRollover();

            if (newMouseFocus != null && newMouseFocus is not UIController)
            {
                UIRollover? newRollover = newMouseFocus.GetRollover();
                CurrentRollover = newRollover;
                newMouseFocus.Controller!.AddChild(newRollover);
            }

            // This is very spammy.
            //Engine.Log.Info($"New mouse input focus {newMouseFocus}", "UI");
        }
        else
        {
            _myMouseFocus?.OnMouseMove(mousePos);
        }
    }
}