#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

namespace Emotion.Platform
{
    public abstract partial class PlatformBase
    {
        /// <summary>
        /// Called when a key is pressed, let go, or a held event is triggered.
        /// </summary>
        public EmotionKeyEvent OnKey { get; } = new();

        /// <summary>
        /// Called when the mouse scrolls.
        /// </summary>
        public event Action<float> OnMouseScroll;

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
        public Vector2 MousePosition
        {
            get => _mousePosition;
            protected set => _mousePosition = WindowPointToViewportPoint(value);
        }

        private Vector2 _mousePosition;
        private bool _skipTextInputThisTick;

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
            const int totalKeys = (int)Key.Last;
            _keys = new bool[totalKeys];

            _keysIm = new bool[totalKeys];
            _keysPreviousIm = new bool[totalKeys];

            SetupLegacy();
            OnKey.AddListener(DefaultButtonBehavior);
            OnMouseScroll += scroll => { _mouseScrollAccum += scroll; };
        }

        /// <summary>
        /// Provides default button behavior for all platforms.
        /// Includes debug shortcuts and universal engine shortcuts.
        /// </summary>
        protected bool DefaultButtonBehavior(Key key, KeyStatus state)
        {
            if (Engine.Configuration.DebugMode)
            {
                Engine.Log.Trace($"Key {key} is {state}.", MessageSource.Input);

                bool ctrl = IsCtrlModifierHeld();
                if (key >= Key.F1 && key <= Key.F10 && state == KeyStatus.Down && ctrl)
                {
                    Vector2 chosenSize = _windowSizes[key - Key.F1];
                    Size = chosenSize;
                    Engine.Log.Info($"Set window size to {chosenSize}", MessageSource.Platform);
                    return false;
                }

                switch (key)
                {
                    case Key.F11 when state == KeyStatus.Down && ctrl:
                        Size = Engine.Configuration.RenderSize * 1.999f - Vector2.One;
                        return false;
                    case Key.Pause when state == KeyStatus.Down:
                        PerfProfiler.ProfileNextFrame();
                        break;
                }
            }

            bool alt = IsKeyHeld(Key.LeftAlt) || IsKeyHeld(Key.RightAlt);

            if (key == Key.Enter && state == KeyStatus.Down && alt)
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
        }

        protected void UpdateKeyStatus(Key key, bool down)
        {
            var keyIndex = (short)key;
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
            if (wasDown && !down) OnKey.Invoke(key, KeyStatus.Up);

            // If it was up, and now is down - it was pressed.
            var downHandled = false;
            if (!wasDown && down) downHandled = OnKey.Invoke(key, KeyStatus.Down);

            // The click was handled, disable text input in case we get an event.
            if (down && downHandled) _skipTextInputThisTick = true;
        }

        protected void UpdateScroll(float amount)
        {
            _mouseScrollAccum += amount;
            OnMouseScroll?.Invoke(amount);
        }

        protected void UpdateTextInput(char c)
        {
            OnTextInputAll?.Invoke(c);
            if (_skipTextInputThisTick) return;
            OnTextInput?.Invoke(c);
        }

        /// <summary>
        /// Transforms the given point from window/screen coordinates to coordinates within the draw buffers viewport.
        /// </summary>
        public static Vector2 WindowPointToViewportPoint(Vector2 pos)
        {
            if (Engine.Renderer == null) return pos;

            // Get the difference in scale.
            float scaleX = Engine.Renderer.ScreenBuffer.Viewport.Size.X / Engine.Renderer.DrawBuffer.Size.X;
            float scaleY = Engine.Renderer.ScreenBuffer.Viewport.Size.Y / Engine.Renderer.DrawBuffer.Size.Y;

            // Calculate letterbox/pillarbox margins.
            float marginX = Engine.Renderer.ScreenBuffer.Size.X / 2 - Engine.Renderer.ScreenBuffer.Viewport.Size.X / 2;
            float marginY = Engine.Renderer.ScreenBuffer.Size.Y / 2 - Engine.Renderer.ScreenBuffer.Viewport.Size.Y / 2;

            return new Vector2((pos.X - marginX) / scaleX, (pos.Y - marginY) / scaleY);
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
            { Key.UpArrow, Key.AxisUpDown },
            { Key.DownArrow, Key.AxisUpDown },
            { Key.LeftArrow, Key.AxisLeftRight },
            { Key.RightArrow, Key.AxisLeftRight },
            { Key.W, Key.AxisWS },
            { Key.S, Key.AxisWS },
            { Key.A, Key.AxisAD },
            { Key.D, Key.AxisAD },
        };

        /// <summary>
        /// Checks if the provided key is part of the provided axis, and if it is returns a vector of
        /// where that key is within the axis.
        /// This might leak key status outside of your event order.
        /// </summary>
        public Vector2 IsKeyPartOfAxis(Key keyToCheck, Key axis)
        {
            if (!_keyToDirectionalAxis.TryGetValue(keyToCheck, out Key directionAxis)) return Vector2.Zero;
            if (axis != directionAxis && !axis.HasFlag(directionAxis)) return Vector2.Zero;

            var value = new Vector2();
            if (directionAxis == Key.AxisUpDown || axis.HasFlag(Key.AxisUpDown))
                value.Y = (_keys[(int)Key.DownArrow] ? 1 : 0) - (_keys[(int)Key.UpArrow] ? 1 : 0);
            if (directionAxis == Key.AxisLeftRight || axis.HasFlag(Key.AxisLeftRight))
                value.X = (_keys[(int)Key.RightArrow] ? 1 : 0) - (_keys[(int)Key.LeftArrow] ? 1 : 0);
            if (directionAxis == Key.AxisWS || axis.HasFlag(Key.AxisWS))
                value.Y = value.Y == 0 ? (_keys[(int)Key.S] ? 1 : 0) - (_keys[(int)Key.W] ? 1 : 0) : value.Y;
            if (directionAxis == Key.AxisAD || axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (_keys[(int)Key.D] ? 1 : 0) - (_keys[(int)Key.A] ? 1 : 0) : value.X;

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
            var idx = (short)key;
            return _keysIm[idx] && !_keysPreviousIm[idx];
        }

        /// <summary>
        /// Returns whether the key is currently being pressed down, regardless of the state of the last tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public bool KeyState(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short)key;
            return _keysIm[idx];
        }

        /// <summary>
        /// Returns whether the key is being held down this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        public bool IsKeyHeld(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short)key;
            return _keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <summary>
        /// Returns whether the key was let go this tick.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        public bool IsKeyUp(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short)key;
            return !_keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <summary>
        /// Get a list of all keys which are held down this tick.
        /// </summary>
        public IEnumerable<Key> GetAllKeysHeld()
        {
            return _keys.Where((x, i) => x && _keysPreviousIm[i]).Select((x, i) => (Key)i);
        }

        /// <summary>
        /// Get a list of all keys which were pressed down this tick.
        /// </summary>
        public IEnumerable<Key> GetAllKeysDown()
        {
            return _keys.Where((x, i) => x && !_keysPreviousIm[i]).Select((x, i) => (Key)i);
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

        #region Legacy

        private void SetupLegacy()
        {
            OnKey.AddListener((key, status) =>
            {
#pragma warning disable 618
                if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) OnMouseKey.Invoke((MouseKey)key, status);
#pragma warning restore 618
                return true;
            });
        }

        /// <summary>
        /// Called when a mouse key is pressed or let go.
        /// </summary>
        [Obsolete("Please use OnKey instead of OnMouseKey")]
        public EmotionEvent<MouseKey, KeyStatus> OnMouseKey { get; } = new EmotionEvent<MouseKey, KeyStatus>();

        /// <summary>
        /// Returns whether the specified mouse key was pressed down this tick.
        /// </summary>
        /// <param name="key">To mouse key to check.</param>
        [Obsolete("Please use IsKeyDown instead of IsMouseKeyDown")]
        public bool IsMouseKeyDown(MouseKey key)
        {
            return IsKeyDown((Key)key);
        }

        /// <summary>
        /// Returns whether the key was let go this tick.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        [Obsolete("Please use IsKeyUp instead of IsMouseKeyUp")]
        public bool IsMouseKeyUp(MouseKey key)
        {
            return IsKeyUp((Key)key);
        }

        /// <summary>
        /// Returns whether the key is being held down this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        [Obsolete("Please use IsKeyHeld instead of IsMouseKeyHeld")]
        public bool IsMouseKeyHeld(MouseKey key)
        {
            return IsKeyHeld((Key)key);
        }

        #endregion
    }
}