#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Common.Configuration;
using Adfectus.Input;
using Adfectus.Logging;

#endregion

namespace Adfectus.Implementation.GLFW
{
    /// <inheritdoc />
    public class GlfwInputManager : IInputManager
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.ScrollFun _scrollCallback;

        private Glfw.CharFun _textInputCallback;
        // ReSharper enable PrivateFieldCanBeConvertedToLocalVariable

        private IntPtr _win;
        private KeyCode[] _allKeys;
        private MouseKey[] _mouseKeys;

        #region State

        private float _mouseScroll;
        private float _mouseScrollThisFrame;
        private float _mouseScrollAccum;

        private Vector2 _mousePosition;

        private Dictionary<KeyCode, bool> _keyStatus = new Dictionary<KeyCode, bool>();
        private Dictionary<KeyCode, bool> _keyStatusShadow = new Dictionary<KeyCode, bool>();

        private Dictionary<MouseKey, bool> _mouseStatus = new Dictionary<MouseKey, bool>();
        private Dictionary<MouseKey, bool> _mouseStatusShadow = new Dictionary<MouseKey, bool>();

        private Queue<char> _textInputQueue = new Queue<char>();

        #endregion

        /// <inheritdoc />
        public GlfwInputManager(IntPtr win)
        {
            _win = win;

            // Attach callbacks.
            _scrollCallback = ScrollCallback;
            Glfw.SetScrollCallback(win, _scrollCallback);

            _textInputCallback = TextInputCallback;
            Glfw.SetCharCallback(win, _textInputCallback);

            // Cache all keys.
            _allKeys = (KeyCode[]) Enum.GetValues(typeof(KeyCode));

            // Populate the key status checkers.
            foreach (KeyCode key in _allKeys)
            {
                _keyStatus[key] = false;
                _keyStatusShadow[key] = false;
            }

            _mouseKeys = (MouseKey[]) Enum.GetValues(typeof(MouseKey));
            foreach (MouseKey key in _mouseKeys)
            {
                _mouseStatus[key] = false;
                _mouseStatusShadow[key] = false;
            }
        }

        #region Callbacks

        private void TextInputCallback(IntPtr _, uint charCode)
        {
            _textInputQueue.Enqueue((char) charCode);
        }

        private void ScrollCallback(IntPtr _, double scrollX, double scrollY)
        {
            _mouseScrollAccum += (float) scrollY * -1;
        }

        #endregion

        /// <inheritdoc />
        public void Update()
        {
            // Update the mouse position.
            Glfw.GetCursorPos(_win, out double posX, out double posY);
            _mousePosition.X = (float) posX;
            _mousePosition.Y = (float) posY;

            // Reset mouse scroll.
            _mouseScrollThisFrame = _mouseScroll;
            _mouseScroll = _mouseScrollAccum;

            // Calculate mouse status.
            foreach (MouseKey key in _mouseKeys)
            {
                _mouseStatusShadow[key] = _mouseStatus[key];

                int state = Glfw.GetMouseButton(_win, (int) key);
                _mouseStatus[key] = state >= 1;
            }

            // Calculate keyboard status.
            foreach (KeyCode key in _allKeys)
            {
                // Transfer from last frame.
                _keyStatusShadow[key] = _keyStatus[key];

                int state = Glfw.GetKey(_win, (int) key);
                _keyStatus[key] = state >= 1;
            }

            // Check for fullscreen toggling key combo.
            if (IsKeyHeld("LeftAlt") && IsKeyDown("Enter"))
            {
                Engine.Host.WindowMode = Engine.Host.WindowMode == WindowMode.Fullscreen ? WindowMode.Windowed : WindowMode.Fullscreen;
            }

            // Check for closing combo.
            if (IsKeyDown("Escape")) Engine.Quit();
        }

        /// <inheritdoc />
        public float GetMouseScroll()
        {
            return _mouseScroll;
        }

        /// <inheritdoc />
        public float GetMouseScrollRelative()
        {
            return _mouseScrollThisFrame - _mouseScroll;
        }

        /// <inheritdoc />
        public Vector2 GetMousePosition()
        {
            // Get the difference in scale.
            float scaleX = Engine.Renderer.HostScale.X / Engine.GraphicsManager.RenderSize.X;
            float scaleY = Engine.Renderer.HostScale.Y / Engine.GraphicsManager.RenderSize.Y;

            // Calculate letterbox/pillarbox margins.
            float marginX = Engine.Host.Size.X / 2 - Engine.Renderer.HostScale.X / 2;
            float marginY = Engine.Host.Size.Y / 2 - Engine.Renderer.HostScale.Y / 2;

            return new Vector2((_mousePosition.X - marginX) / scaleX, (_mousePosition.Y - marginY) / scaleY);
        }

        /// <inheritdoc />
        public bool IsMouseKeyDown(MouseKey key)
        {
            bool down = _mouseStatus[key] && !_mouseStatusShadow[key];

            if (down) Engine.Log.Trace($"Mouse button {key} is pressed down.", MessageSource.Input);

            return down;
        }

        /// <inheritdoc />
        public bool IsMouseKeyUp(MouseKey key)
        {
            bool up = !_mouseStatus[key] && _mouseStatusShadow[key];

            if (up) Engine.Log.Trace($"Mouse button {key} is released.", MessageSource.Input);

            return up;
        }

        /// <inheritdoc />
        public bool IsMouseKeyHeld(MouseKey key)
        {
            return _mouseStatus[key] && _mouseStatusShadow[key];
        }

        /// <inheritdoc />
        public IEnumerable<KeyCode> GetAllKeysHeld()
        {
            return _keyStatus.AsParallel().Where(x => x.Value && _keyStatusShadow[x.Key]).Select(x => x.Key);
        }

        /// <inheritdoc />
        public IEnumerable<KeyCode> GetAllKeysDown()
        {
            return _keyStatus.AsParallel().Where(x => x.Value && !_keyStatusShadow[x.Key]).Select(x => x.Key);
        }

        /// <inheritdoc />
        public bool IsKeyHeld(string key)
        {
            KeyCode codeFromString = (KeyCode) Enum.Parse(typeof(KeyCode), key);

            return IsKeyHeld(codeFromString);
        }

        /// <inheritdoc />
        public bool IsKeyHeld(KeyCode key)
        {
            return _keyStatus[key] && _keyStatusShadow[key];
        }

        /// <inheritdoc />
        public bool IsKeyDown(string key)
        {
            KeyCode codeFromString = (KeyCode) Enum.Parse(typeof(KeyCode), key);
            return IsKeyDown(codeFromString);
        }

        /// <inheritdoc />
        public bool IsKeyDown(KeyCode key)
        {
            bool down = _keyStatus[key] && !_keyStatusShadow[key];
            if (down) Engine.Log.Trace($"Key {key} is pressed down.", MessageSource.Input);
            return down;
        }

        /// <inheritdoc />
        public bool IsKeyUp(string key)
        {
            KeyCode codeFromString = (KeyCode) Enum.Parse(typeof(KeyCode), key);
            return IsKeyUp(codeFromString);
        }

        /// <inheritdoc />
        public bool IsKeyUp(KeyCode key)
        {
            bool up = !_keyStatus[key] && _keyStatusShadow[key];
            if (up) Engine.Log.Trace($"Key {key} is let go.", MessageSource.Input);
            return up;
        }

        /// <inheritdoc />
        public bool IsKeyHeld(short key)
        {
            return IsKeyHeld(GetKeyNameFromCode(key));
        }

        /// <inheritdoc />
        public bool IsKeyDown(short key)
        {
            return IsKeyDown(GetKeyNameFromCode(key));
        }

        /// <inheritdoc />
        public bool IsKeyUp(short key)
        {
            return IsKeyUp(GetKeyNameFromCode(key));
        }

        /// <inheritdoc />
        public string GetKeyNameFromCode(short key)
        {
            return Enum.GetName(typeof(KeyCode), key);
        }

        /// <inheritdoc />
        public char GetNextTextInput(bool handle = true)
        {
            if (_textInputQueue.Count == 0) return (char) 0;

            return handle ? _textInputQueue.Dequeue() : _textInputQueue.Peek();
        }
    }
}