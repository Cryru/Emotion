#region Using

using System;
using Adfectus.Common;
using Adfectus.Input;

#endregion

namespace Adfectus.Platform.DesktopGL
{
    /// <summary>
    /// A joystick loaded by glfw.
    /// </summary>
    public sealed class GlfwJoystick : Joystick
    {
        private float[] _axisLastFrame;
        private byte[] _buttonLastFrame;
        private float[] _axisThisFrame;
        private byte[] _buttonThisFrame;

        private int _expectedAxis;
        private int _expectedButtons;

        /// <summary>
        /// A joystick loaded by glfw.
        /// </summary>
        /// <param name="id">The id of the joystick.</param>
        /// <param name="name">The name of the joystick.</param>
        public GlfwJoystick(int id, string name) : base(id, name)
        {
            _expectedAxis = Enum.GetValues(typeof(JoystickAxis)).Length;
            _expectedButtons = Enum.GetValues(typeof(JoystickButton)).Length;
        }

        public void Update()
        {
            // Check if still connected.
            Connected = Glfw.JoystickPresent(Id) == 1;

            if (!Connected) return;

            _axisLastFrame = _axisThisFrame;
            _axisThisFrame = Glfw.GetJoystickAxes(Id, out int axisC);

            _buttonLastFrame = _buttonThisFrame;
            _buttonThisFrame = Glfw.GetJoystickButtons(Id, out int buttonC);

            // If the number of axes or buttons doesn't match the expected amount - the controller is not XInput.
            if (axisC != _expectedAxis || buttonC != _expectedButtons)
            {
                Connected = false;
            }
        }

        /// <inheritdoc />
        public override bool IsKeyDown(JoystickButton button)
        {
            if (!Connected) return false;

            int buttonIndex = (int) button;
            if (buttonIndex < _buttonLastFrame.Length && buttonIndex < _buttonThisFrame.Length) return _buttonLastFrame[buttonIndex] == 0 && _buttonThisFrame[buttonIndex] == 1;

            Engine.Log.Warning($"Tried to poll Joystick button {button} ({buttonIndex}), but the joystick didn't report it.", Logging.MessageSource.Input);
            return false;
        }

        /// <inheritdoc />
        public override bool IsKeyUp(JoystickButton button)
        {
            if (!Connected) return false;

            int buttonIndex = (int) button;
            if (buttonIndex < _buttonLastFrame.Length && buttonIndex < _buttonThisFrame.Length) return _buttonLastFrame[buttonIndex] == 1 && _buttonThisFrame[buttonIndex] == 0;

            Engine.Log.Warning($"Tried to poll Joystick button {button} ({buttonIndex}), but the joystick didn't report it.", Logging.MessageSource.Input);
            return false;
        }

        /// <inheritdoc />
        public override bool IsKeyHeld(JoystickButton button)
        {
            if (!Connected) return false;

            int buttonIndex = (int) button;
            if (buttonIndex < _buttonLastFrame.Length && buttonIndex < _buttonThisFrame.Length) return _buttonLastFrame[buttonIndex] == 1 && _buttonThisFrame[buttonIndex] == 1;

            Engine.Log.Warning($"Tried to poll Joystick button {button} ({buttonIndex}), but the joystick didn't report it.", Logging.MessageSource.Input);
            return false;

        }

        /// <inheritdoc />
        public override float GetAxis(JoystickAxis axis)
        {
            if (!Connected) return 0;

            int axisIndex = (int) axis;
            if (axisIndex < _axisLastFrame.Length && axisIndex < _axisThisFrame.Length) return _axisThisFrame[axisIndex];

            Engine.Log.Warning($"Tried to poll Joystick axis {axis} ({axisIndex}), but the joystick didn't report it.", Logging.MessageSource.Input);
            return 0;
        }

        /// <inheritdoc />
        public override float GetAxisRelative(JoystickAxis axis)
        {
            if (!Connected) return 0;

            int axisIndex = (int) axis;
            if (axisIndex < _axisLastFrame.Length && axisIndex < _axisThisFrame.Length) return _axisThisFrame[axisIndex] - _axisLastFrame[axisIndex];

            Engine.Log.Warning($"Tried to poll Joystick axis {axis} ({axisIndex}), but the joystick didn't report it.", Logging.MessageSource.Input);
            return 0;
        }
    }
}