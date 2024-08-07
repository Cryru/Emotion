﻿namespace Emotion.UI;

#nullable enable

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
    private static uint _thisTick; // The index of the current tick. Used to dedupe calls to update since every controller will call it.
    private bool _calledUpdateLastTick; // Has this particular controller called update this tick. Used to determine if the controller is being updated.
    private bool _calledUpdateTickBeforeLast;

    private void UpdateMouseFocus()
    {
        _calledUpdateLastTick = true;

        // It was already updated this tick.
        if (_thisTick == Engine.TickCount) return;
        _thisTick = Engine.TickCount;

        if (Engine.Host.HostPaused)
        {
            ClearControllersMouseInputExcept(null);
            return;
        }

        // First check if any controller has a priority focus.
        // Priority focus means currently holding down a mouse button.
        UIController? consumedInput = null;
        for (var i = 0; i < _allControllers.Count; i++) // Controllers are presorted in priority order.
        {
            UIController controller = _allControllers[i];
            if (!controller._calledUpdateLastTick) continue; // Controller inactive via update.
            if (!controller.ChildrenHandleInput) continue;

            UIBaseWindow? hasPriority = controller.HasButtonHeldMouseFocus();
            if (hasPriority == null) continue;

            controller.SetControllerMouseFocus(hasPriority);
            consumedInput = controller;
            break;
        }

        if (consumedInput != null)
        {
            ClearControllersMouseInputExcept(consumedInput);
            return;
        }

        // Check if any controller has a window under the cursor.
        Vector2 mousePos = Engine.Host.MousePosition;
        for (var i = 0; i < _allControllers.Count; i++)
        {
            UIController controller = _allControllers[i];
            if (!controller._calledUpdateLastTick) continue;
            if (!controller.ChildrenHandleInput || !controller.Visible) continue;

            UIBaseWindow? focus = controller.FindMouseInput(mousePos);

            if (focus == controller && controller._inputFocusManual == null) focus = null;

            if (focus != null)
            {
                controller.SetControllerMouseFocus(focus);
                consumedInput = controller;
                break;
            }
        }

        ClearControllersMouseInputExcept(consumedInput);
    }

    private static void ClearControllersMouseInputExcept(UIController? except)
    {
        for (var i = 0; i < _allControllers.Count; i++)
        {
            UIController controller = _allControllers[i];
            if (controller == except) continue;
            controller.SetControllerMouseFocus(null);
        }

        // Reset update trackers.
        for (var i = 0; i < _allControllers.Count; i++)
        {
            UIController controller = _allControllers[i];
            controller._calledUpdateTickBeforeLast = controller._calledUpdateLastTick;
            controller._calledUpdateLastTick = false;
        }

        MouseFocus = except?._myMouseFocus;
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
            if (_myMouseFocus != null) Engine.Host.OnKey.RemoveListener(MouseFocusOnKey);
            _myMouseFocus = newMouseFocus;
            if (_myMouseFocus != null) Engine.Host.OnKey.AddListener(MouseFocusOnKey, KeyPriority);
            _myMouseFocus?.OnMouseEnter(mousePos);

            if (CurrentRollover != null)
            {
                UIController? rolloverController = CurrentRollover.Controller;
                rolloverController?.RemoveChild(CurrentRollover);
                CurrentRollover = null;
            }

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