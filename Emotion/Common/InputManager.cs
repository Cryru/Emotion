#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Common
{
    /// <summary>
    /// Legacy Emotion input manager. Still has its uses.
    /// A class which manages input from a mouse and keyboard, regardless of whether they are physical or virtual.
    /// On a device with a touchscreen for instance the mouse would be the touches.
    /// </summary>
    public class InputManager
    {
        private float _mouseScroll;
        private float _mouseScrollThisFrame;
        private float _mouseScrollAccum;

        private Key[] _allKeys;
        private MouseKey[] _mouseKeys;

        private Dictionary<Key, bool> _keyStatus = new Dictionary<Key, bool>();
        private Dictionary<Key, bool> _keyStatusShadow = new Dictionary<Key, bool>();

        private Dictionary<MouseKey, bool> _mouseStatus = new Dictionary<MouseKey, bool>();
        private Dictionary<MouseKey, bool> _mouseStatusShadow = new Dictionary<MouseKey, bool>();

        /// <summary>
        /// Redirect of Engine.Host.MousePosition
        /// </summary>
        public Vector2 MousePosition
        {
            get => Engine.Host.MousePosition;
        }

        public InputManager()
        {
            // Cache all keys.
            _allKeys = (Key[]) Enum.GetValues(typeof(Key));

            // Populate the key status checkers.
            foreach (Key key in _allKeys)
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

            Engine.Host.OnMouseScroll.AddListener(scroll => { _mouseScrollAccum += scroll; return true; });
        }

        /// <summary>
        /// Update the input manager module. Is called every tick, as opposed to the input events which are checked every frame.
        /// </summary>
        public void Update()
        {
            // Reset mouse scroll.
            _mouseScrollThisFrame = _mouseScroll;
            _mouseScroll = _mouseScrollAccum;

            // Transfer key status.
            foreach (Key key in _allKeys)
            {
                // Transfer from last frame.
                _keyStatusShadow[key] = _keyStatus[key];
                _keyStatus[key] = Engine.Host.GetKeyDown(key);
            }

            // Transfer mouse status.
            foreach (MouseKey key in _mouseKeys)
            {
                _mouseStatusShadow[key] = _mouseStatus[key];
                _mouseStatus[key] = Engine.Host.GetMouseKeyDown(key);
            }
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

        /// <summary>
        /// Returns whether the mouse key was pressed down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was pressed down.</returns>
        public bool IsMouseKeyDown(MouseKey key)
        {
            bool down = _mouseStatus[key] && !_mouseStatusShadow[key];

            if (down) Engine.Log.Trace($"Mouse button {key} is pressed down.", MessageSource.Other);

            return down;
        }

        /// <summary>
        /// Returns whether the mouse key was let go.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was let go.</returns>
        public bool IsMouseKeyUp(MouseKey key)
        {
            bool up = !_mouseStatus[key] && _mouseStatusShadow[key];

            if (up) Engine.Log.Trace($"Mouse button {key} is released.", MessageSource.Other);

            return up;
        }

        /// <summary>
        /// Returns whether the mouse key is being held down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is being held down.</returns>
        public bool IsMouseKeyHeld(MouseKey key)
        {
            return _mouseStatus[key] && _mouseStatusShadow[key];
        }

        /// <summary>
        /// Get a list of all keys which are held down.
        /// </summary>
        /// <returns>A list of all keys which are currently held down.</returns>
        public IEnumerable<Key> GetAllKeysHeld()
        {
            return _keyStatus.AsParallel().Where(x => x.Value && _keyStatusShadow[x.Key]).Select(x => x.Key);
        }

        /// <summary>
        /// Get a list of all keys which were pressed down this frame.
        /// </summary>
        /// <returns>A list of all keys which were pressed down.</returns>
        public IEnumerable<Key> GetAllKeysDown()
        {
            return _keyStatus.AsParallel().Where(x => x.Value && !_keyStatusShadow[x.Key]).Select(x => x.Key);
        }

        /// <summary>
        /// Returns whether the key is being held down, using its entry in the KeyCode enum.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        public bool IsKeyHeld(Key key)
        {
            return _keyStatus[key] && _keyStatusShadow[key];
        }

        /// <summary>
        /// Returns whether the key was pressed down, using its entry in the KeyCode enum.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(Key key)
        {
            bool down = _keyStatus[key] && !_keyStatusShadow[key];
            if (down) Engine.Log.Trace($"Key {key} is pressed down.", MessageSource.Other);
            return down;
        }

        /// <summary>
        /// Returns whether the key was let go, using its entry in the KeyCode enum.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was let go.</returns>
        public bool IsKeyUp(Key key)
        {
            bool up = !_keyStatus[key] && _keyStatusShadow[key];
            if (up) Engine.Log.Trace($"Key {key} is let go.", MessageSource.Other);
            return up;
        }
    }
}