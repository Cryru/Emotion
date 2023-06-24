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

	private UIBaseWindow? _myMouseFocus; // The mouse focus of this controller in particular.
	private static float _thisTick; // The timestamp of the current tick. Used to dedupe calls to update since every controller will call it.
	private bool _calledUpdateLastTick; // Has this particular controller called update this tick. Used to determine if the controller is being updated.

	private static void UpdateMouseFocusAllControllers()
	{
		// It was already updated this tick.
		if (_thisTick == Engine.TotalTime) return;
		_thisTick = Engine.TotalTime;

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
			if (controller.InputTransparent) continue;

			UIBaseWindow? hasPriority = controller.HasPriorityMouseFocus();
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
			if (controller.InputTransparent || !controller.Visible) continue;

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
			controller._calledUpdateLastTick = false;
		}

		MouseFocus = except?._myMouseFocus;
	}

	private UIBaseWindow? HasPriorityMouseFocus()
	{
		// If currently holding down a mouse button don't change the mouse focus if it is still valid.
		if (_myMouseFocus == null || !_myMouseFocus.Visible || _myMouseFocus.InputTransparent) return null;

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

			// This is very spammy.
			//Engine.Log.Info($"New mouse input focus {newMouseFocus}", "UI");
		}
		else
		{
			_myMouseFocus?.OnMouseMove(mousePos);
		}
	}

	private void UpdateMouseFocus()
	{
#if true
		UpdateMouseFocusAllControllers();
		_calledUpdateLastTick = true;
#else
		Vector2 mousePos = Engine.Host.MousePosition;
		
		UIBaseWindow? newMouseFocus = null;
		newMouseFocus = HasPriorityMouseFocus();

		if (newMouseFocus == null)
		{
			newMouseFocus = Engine.Host.HostPaused || InputTransparent ? null : FindMouseInput(mousePos);

			// If the controller is focused that means there is no focus.
			// But if there is a dropdown open we want to capture the dismiss click so we keep it as a focus.
			if (newMouseFocus == this && _inputFocusManual == null) newMouseFocus = null;
		}

		SetControllerMouseFocus(newMouseFocus);
		MouseFocus = newMouseFocus;
#endif
	}
}