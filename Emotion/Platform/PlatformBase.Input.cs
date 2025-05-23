﻿#region Using

using Emotion.Common.Input;
using System.Linq;

#endregion

namespace Emotion.Platform
{
    public abstract partial class PlatformBase
    {
        /// <summary>
        /// Called when a key is pressed, let go, or a held event is triggered.
        /// </summary>
        public EmotionKeyEvent OnKey { get => Engine.Input.OnKey; }

        /// <summary>
        /// Called when the mouse moves. The first vector is the old one, the second is the new position.
        /// </summary>
        public event Action<Vector2, Vector2> OnMouseMove;

        /// <summary>
        /// Called when text input is detected. Most of the time this is identical to OnKey, but without the state.
        /// Is not called if the key is handled by an OnKey event.
        /// </summary>
        public event Action<char> OnTextInput;

        /// <summary>
        /// Called when text input is detected, even if the key is handled.
        /// </summary>
        public event Action<char> OnTextInputAll;

        /// <summary>
        /// Returns the current mouse position. Is preprocessed by the Renderer to scale to the window if possible.
        /// Therefore it is in screen coordinates which change with the size of the Engine.Renderer.ScreenBuffer.
        /// </summary>
        public Vector2 MousePosition { get => Engine.Input.MousePosition; }

        private bool _skipTextInputThisTick;
        private bool _skipKeyInputThisTick;

        protected Key[] _keyCodes;
        protected bool[] _keys;
        protected short[] _scanCodes;
        protected float _mouseScroll;
        protected float _mouseScrollThisFrame;
        protected float _mouseScrollAccum;

        // Immediate-mode input
        protected bool[] _keysIm;
        protected bool[] _keysPreviousIm;

        private void SetupInput()
        {
            const int totalKeys = (int) Key.Last;
            _keys = new bool[totalKeys];

            _keysIm = new bool[totalKeys];
            _keysPreviousIm = new bool[totalKeys];

            Engine.Input.OnMouseMove += (old, nu) =>
            {
                OnMouseMove(old, nu);
            };
            OnKey.AddListener(DefaultButtonBehavior, KeyListenerType.System);
            OnFocusChanged += PreventButtonInputOnRefocus;
        }

        private void PreventButtonInputOnRefocus(bool focused)
        {
            // This means it changed from focused.
            if (focused) _skipKeyInputThisTick = true;
        }

        /// <summary>
        /// Provides default button behavior for all platforms.
        /// Includes debug shortcuts and universal engine shortcuts.
        /// </summary>
        private bool DefaultButtonBehavior(Key key, KeyState state)
        {
            if (Engine.Configuration.DebugMode)
            {
                Engine.Log.Trace($"Key {key} is {state}.", MessageSource.Input);

                bool ctrl = IsCtrlModifierHeld();
                if (key >= Key.F1 && key <= Key.F10 && state == Common.Input.KeyState.Down && ctrl)
                {
                    Vector2 chosenSize = _windowSizes[key - Key.F1];
                    Size = chosenSize;
                    Engine.Log.Info($"Set window size to {chosenSize}", MessageSource.Platform);
                    return false;
                }

                switch (key)
                {
                    case Key.F11 when state == Common.Input.KeyState.Down && ctrl:
                        Size = Engine.Configuration.RenderSize * 1.999f - Vector2.One;
                        return false;
                    case Key.Pause when state == Common.Input.KeyState.Down:
                        PerfProfiler.ProfileNextFrame();
                        break;
                }
            }

            if (key == Key.Enter && state == Common.Input.KeyState.Down && IsAltModifierHeld())
            {
                DisplayMode = DisplayMode == DisplayMode.Fullscreen ? DisplayMode.Windowed : DisplayMode.Fullscreen;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updated only during update tick, unlike the normal button logic, which is updated every platform tick (every loop
        /// tick). This filters out any down-up event pairs which occured within the same tick.
        /// </summary>
        public virtual void UpdateInput()
        {
            // Transfer key status to previous.
            for (var i = 0; i < _keysIm.Length; i++)
            {
                _keysPreviousIm[i] = _keysIm[i];
                _keysIm[i] = _keys[i];
            }

            _mouseScrollThisFrame = _mouseScroll;
            _mouseScroll = _mouseScrollAccum;

            _skipTextInputThisTick = false;
            _skipKeyInputThisTick = false;
        }

        protected void UpdateKeyStatus(Key key, bool down)
        {
            if (_skipKeyInputThisTick && down) return;

            var keyIndex = (short) key;
            if (keyIndex < 0 || keyIndex >= _keys.Length)
            {
                Engine.Log.Warning($"Got event for unknown key - {key}/{keyIndex}", MessageSource.Platform);
                return;
            }

            bool wasDown = _keys[keyIndex];
            _keys[keyIndex] = down;

            // If it was down, and still is - then it's held.
            //if (wasDown && down) OnKey.Invoke(key, KeyStatus.Held);

            // If it was down, but no longer is - it was let go.
            if (wasDown && !down) Engine.Input.ReportKeyInput(key, Common.Input.KeyState.Up);

            // If it was up, and now is down - it was pressed.
            var downHandled = false;
            if (!wasDown && down) Engine.Input.ReportKeyInput(key, Common.Input.KeyState.Down);

            // The click was handled, disable text input in case we get an event.
            //if (down && downHandled) _skipTextInputThisTick = true;
        }

        protected void UpdateScroll(float amount)
        {
            if (amount != 0)
            {
                _mouseScrollAccum += amount;
                Engine.Input.ReportKeyInput(Key.MouseWheel, amount < 0 ? Common.Input.KeyState.Down : Common.Input.KeyState.Up);
                //UpdateKeyStatus(Key.MouseKeyWheel, amount < 0);
            }

            //OnMouseScroll?.Invoke(amount);
        }

        protected void UpdateTextInput(char c)
        {
            if (char.IsControl(c) && c != '\b' && c != '\n' && c != '\r') return;
            OnTextInputAll?.Invoke(c);
            if (_skipTextInputThisTick) return;
            OnTextInput?.Invoke(c);
        }

        /// <summary>
        /// Returns whether any of the ctrl keys are held down.
        /// </summary>
        public bool IsCtrlModifierHeld()
        {
            return IsKeyHeld(Key.LeftControl) || IsKeyHeld(Key.RightControl);
        }

        /// <summary>
        /// Returns whether any of the shift keys are held down.
        /// </summary>
        public bool IsShiftModifierHeld()
        {
            return IsKeyHeld(Key.LeftShift) || IsKeyHeld(Key.RightShift);
        }

        /// <summary>
        /// Returns whether any of the alt keys are held down.
        /// </summary>
        public bool IsAltModifierHeld()
        {
            return IsKeyHeld(Key.LeftAlt) || IsKeyHeld(Key.RightAlt);
        }

        /// <summary>
        /// Returns the input value of the specified axis or axes.
        /// The keys are considered only if held.
        /// </summary>
        /// <param name="axis">The axis to get the value of.</param>
        public Vector2 GetAxisHeld(Key axis)
        {
            var value = new Vector2();

            if (axis.HasFlag(Key.AxisUpDown)) value.Y = (IsKeyHeld(Key.DownArrow) ? 1 : 0) - (IsKeyHeld(Key.UpArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisLeftRight)) value.X = (IsKeyHeld(Key.RightArrow) ? 1 : 0) - (IsKeyHeld(Key.LeftArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisWS)) value.Y = value.Y == 0 ? (IsKeyHeld(Key.S) ? 1 : 0) - (IsKeyHeld(Key.W) ? 1 : 0) : value.Y;
            if (axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (IsKeyHeld(Key.D) ? 1 : 0) - (IsKeyHeld(Key.A) ? 1 : 0) : value.X;

            return value;
        }

        /// <summary>
        /// Returns the input value of the specified axis or axes.
        /// The keys are considered only if they were just pressed down.
        /// </summary>
        /// <param name="axis">The axis to get the value of.</param>
        public Vector2 GetAxisDown(Key axis)
        {
            var value = new Vector2();

            if (axis.HasFlag(Key.AxisUpDown)) value.Y = (IsKeyDown(Key.DownArrow) ? 1 : 0) - (IsKeyDown(Key.UpArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisLeftRight)) value.X = (IsKeyDown(Key.RightArrow) ? 1 : 0) - (IsKeyDown(Key.LeftArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisWS)) value.Y = value.Y == 0 ? (IsKeyDown(Key.S) ? 1 : 0) - (IsKeyDown(Key.W) ? 1 : 0) : value.Y;
            if (axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (IsKeyDown(Key.D) ? 1 : 0) - (IsKeyDown(Key.A) ? 1 : 0) : value.X;

            return value;
        }

        /// <summary>
        /// Returns the input value of the specified axis or axes.
        /// The keys are considered only if they were just let go.
        /// </summary>
        /// <param name="axis">The axis to get the value of.</param>
        public Vector2 GetAxisUp(Key axis)
        {
            var value = new Vector2();

            if (axis.HasFlag(Key.AxisUpDown)) value.Y = (IsKeyUp(Key.DownArrow) ? 1 : 0) - (IsKeyUp(Key.UpArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisLeftRight)) value.X = (IsKeyUp(Key.RightArrow) ? 1 : 0) - (IsKeyUp(Key.LeftArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisWS)) value.Y = value.Y == 0 ? (IsKeyUp(Key.S) ? 1 : 0) - (IsKeyUp(Key.W) ? 1 : 0) : value.Y;
            if (axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (IsKeyUp(Key.D) ? 1 : 0) - (IsKeyUp(Key.A) ? 1 : 0) : value.X;

            return value;
        }

        private static Dictionary<Key, Key> _keyToDirectionalAxis = new Dictionary<Key, Key>
        {
            {Key.UpArrow, Key.AxisUpDown},
            {Key.DownArrow, Key.AxisUpDown},
            {Key.LeftArrow, Key.AxisLeftRight},
            {Key.RightArrow, Key.AxisLeftRight},
            {Key.W, Key.AxisWS},
            {Key.S, Key.AxisWS},
            {Key.A, Key.AxisAD},
            {Key.D, Key.AxisAD},
        };

        /// <summary>
        /// Checks if the provided key is part of the provided axis, and if it is returns a vector of
        /// where that key is within the axis.
        /// This might leak key status outside of your event order.
        /// </summary>
        public Vector2 GetKeyAxisPartDown(Key keyToCheck, Key axis)
        {
            if (!_keyToDirectionalAxis.TryGetValue(keyToCheck, out Key directionAxis)) return Vector2.Zero;
            if (axis != directionAxis && !axis.HasFlag(directionAxis)) return Vector2.Zero;

            var value = new Vector2();
            if (directionAxis == Key.AxisUpDown || axis.HasFlag(Key.AxisUpDown))
                value.Y = (_keys[(int) Key.DownArrow] ? 1 : 0) - (_keys[(int) Key.UpArrow] ? 1 : 0);
            if (directionAxis == Key.AxisLeftRight || axis.HasFlag(Key.AxisLeftRight))
                value.X = (_keys[(int) Key.RightArrow] ? 1 : 0) - (_keys[(int) Key.LeftArrow] ? 1 : 0);
            if (directionAxis == Key.AxisWS || axis.HasFlag(Key.AxisWS))
                value.Y = value.Y == 0 ? (_keys[(int) Key.S] ? 1 : 0) - (_keys[(int) Key.W] ? 1 : 0) : value.Y;
            if (directionAxis == Key.AxisAD || axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (_keys[(int) Key.D] ? 1 : 0) - (_keys[(int) Key.A] ? 1 : 0) : value.X;

            return value;
        }

        /// <summary>
        /// Check if the provided key is part of the provided axis and returns a vector with
        /// the position of the key within the axis, regardless of whether it is currently down.
        /// </summary>
        public Vector2 GetKeyAxisPart(Key keyToCheck, Key axis)
        {
            if (!_keyToDirectionalAxis.TryGetValue(keyToCheck, out Key directionAxis)) return Vector2.Zero;
            if (axis != directionAxis && !axis.HasFlag(directionAxis)) return Vector2.Zero;

            var value = new Vector2();
            if (directionAxis == Key.AxisUpDown || axis.HasFlag(Key.AxisUpDown))
                value.Y = (keyToCheck == Key.DownArrow ? 1 : 0) - (keyToCheck == Key.UpArrow ? 1 : 0);
            if (directionAxis == Key.AxisLeftRight || axis.HasFlag(Key.AxisLeftRight))
                value.X = (keyToCheck == Key.RightArrow ? 1 : 0) - (keyToCheck == Key.LeftArrow ? 1 : 0);
            if (directionAxis == Key.AxisWS || axis.HasFlag(Key.AxisWS))
                value.Y = value.Y == 0 ? (keyToCheck == Key.S ? 1 : 0) - (keyToCheck == Key.W ? 1 : 0) : value.Y;
            if (directionAxis == Key.AxisAD || axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (keyToCheck == Key.D ? 1 : 0) - (keyToCheck == Key.A ? 1 : 0) : value.X;

            return value;
        }

        /// <summary>
        /// Returns whether the specified key was pressed down this tick.
        /// </summary>
        /// <param name="key">To key to check.</param>
        public bool IsKeyDown(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx] && !_keysPreviousIm[idx];
        }

        /// <summary>
        /// Returns whether the key is currently being pressed down, regardless of the state of the last tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public bool KeyState(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx];
        }

        /// <summary>
        /// Returns whether the key is being held down this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public bool IsKeyHeld(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <summary>
        /// Returns whether the key was let go this tick.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        public bool IsKeyUp(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return !_keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <summary>
        /// Get a list of all keys which are held down this tick.
        /// </summary>
        public IEnumerable<Key> GetAllKeysHeld()
        {
            return _keys.Where((x, i) => x && _keysPreviousIm[i]).Select((x, i) => (Key) i);
        }

        /// <summary>
        /// Get a list of all keys which were pressed down this tick.
        /// </summary>
        public IEnumerable<Key> GetAllKeysDown()
        {
            return _keys.Where((x, i) => x && !_keysPreviousIm[i]).Select((x, i) => (Key) i);
        }

        /// <summary>
        /// The amount the scroll wheel is scrolled.
        /// </summary>
        /// <returns>The amount the scroll wheel is scrolled.</returns>
        public float GetMouseScroll()
        {
            return _mouseScroll;
        }

        /// <summary>
        /// The amount the mouse has scrolled since the last tick.
        /// </summary>
        /// <returns>The amount the mouse has scrolled since the last tick.</returns>
        public float GetMouseScrollRelative()
        {
            return _mouseScrollThisFrame - _mouseScroll;
        }
    }
}