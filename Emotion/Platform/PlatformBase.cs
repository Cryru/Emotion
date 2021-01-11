#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Platform
{
    public abstract class PlatformBase : IInputManager
    {
        /// <summary>
        /// Whether the platform is setup.
        /// </summary>
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Whether the platform's window is open, and considered active.
        /// </summary>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// Whether the platform is currently the OS focus.
        /// This usually means that the platform's window is focused.
        /// On some platforms this is always true.
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// The graphics context.
        /// </summary>
        public GraphicsContext Context { get; protected set; }

        /// <summary>
        /// The platform's audio context. If any.
        /// </summary>
        public AudioContext Audio { get; protected set; }

        /// <summary>
        /// The event is set while the window is focused.
        /// </summary>
        public ManualResetEvent FocusWait { get; set; } = new ManualResetEvent(true);

        /// <summary>
        /// Whether this platform supports naming threads.
        /// </summary>
        public bool NamedThreads { get; set; } = true;

        /// <summary>
        /// The sizes to switch between in debug mode by using ctrl + F1-F9
        /// Debug functionality.
        /// </summary>
        private readonly Vector2[] _windowSizes =
        {
            new Vector2(640, 360), // Lowest 16:9 good integer scaling potential
            new Vector2(960, 540), // Low 16:9
            new Vector2(1280, 720), // 16:9 720p HD
            new Vector2(1920, 1080), // 16:9 1080p FullHD

            new Vector2(640, 400), // Lowest 16:10
            new Vector2(768, 480), // Low 16:10
            new Vector2(1280, 800), // 16:10 HD
            new Vector2(1920, 1200), // 16:10 FullHD

            new Vector2(800, 600), // 4:3
            new Vector2(1600, 768) // Super Wide
        };

        /// <summary>
        /// This event is called when the window's focus changes
        /// On some platforms the window cannot be unfocused - and this will never be called.
        /// The input parameter is the new focus state.
        /// </summary>
        public EmotionEvent<bool> OnFocusChanged { get; protected set; } = new EmotionEvent<bool>();

        /// <summary>
        /// This event is called when the platform's display size changes.
        /// The input parameter is the new size.
        /// </summary>
        public EmotionEvent<Vector2> OnResize { get; protected set; } = new EmotionEvent<Vector2>();

        #region Internal

        /// <summary>
        /// Setup the native platform and creates a window.
        /// </summary>
        /// <param name="config">Configuration for the platform - usually passed from the engine.</param>
        public virtual void Setup(Configurator config)
        {
            OnMouseScroll.AddListener(scroll =>
            {
                _mouseScrollAccum += scroll;
                return true;
            });

            const int totalKeys = (int) Key.Last;
            _keys = new bool[totalKeys];
            _keysPrevious = new bool[totalKeys];

            _keysIm = new bool[totalKeys];
            _keysPreviousIm = new bool[totalKeys];

            SetupLegacy();
            SetupPlatform(config);

            // Check if the platform and graphics initialization was successful.
            if (Context == null)
            {
                Engine.CriticalError(new Exception("Platform couldn't create graphics context."));
                return;
            }

            // Make this the current context, and bind it.
            // "There /can/ be only one."
            Context.MakeCurrent();
            Gl.BindAPI(Context);

            // Set display mode, show and focus.
            DisplayMode = config.InitialDisplayMode;
            WindowState = WindowState.Normal;

            // Attach default key behavior.
            OnKey.AddListener(DefaultButtonBehavior);

            IsSetup = true;
            IsOpen = true;
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
                        break;
                    case Key.Pause when state == KeyStatus.Down:
                        PerfProfiler.ProfileNextFrame();
                        break;
                }
            }

            bool alt = IsKeyHeld(Key.LeftAlt) || IsKeyHeld(Key.RightAlt);

            if (key == Key.Enter && state == KeyStatus.Down && alt) DisplayMode = DisplayMode == DisplayMode.Fullscreen ? DisplayMode.Windowed : DisplayMode.Fullscreen;

            return true;
        }

        #endregion

        #region Internal API

        protected void UpdateKeyStatus(Key key, bool down)
        {
            var keyIndex = (short) key;

            // If it was down, and still is - then it's held.
            if (_keys[keyIndex] && down) OnKey.Invoke(key, KeyStatus.Held);

            // If it was down, but no longer is - it was let go.
            if (_keys[keyIndex] && !down) OnKey.Invoke(key, KeyStatus.Up);

            // If it was up, and now is down - it was pressed.
            if (!_keys[keyIndex] && down) OnKey.Invoke(key, KeyStatus.Down);
            _keys[keyIndex] = down;
        }

        protected void UpdateScroll(float amount)
        {
            _mouseScrollAccum += amount;
            OnMouseScroll.Invoke(amount);
        }

        protected void UpdateFocus(bool focused)
        {
            IsFocused = focused;
            if (focused)
            {
                Engine.Log.Info("Focus regained.", MessageSource.Platform);
                FocusWait.Set();
            }
            else
            {
                Engine.Log.Info("Focus lost.", MessageSource.Platform);

                // Pull all buttons up.
                for (var i = 0; i < _keys.Length; i++)
                {
                    UpdateKeyStatus((Key) i, false);
                }

                if (!Engine.Configuration.DebugMode) FocusWait.Reset();
            }

            OnFocusChanged.Invoke(IsFocused);
        }

        #endregion

        #region Implementation API

        /// <summary>
        /// Platform setup.
        /// </summary>
        protected abstract void SetupPlatform(Configurator config);

        /// <summary>
        /// Display an error message natively.
        /// Usually this means a popup will show up.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public abstract void DisplayMessageBox(string message);

        /// <summary>
        /// Update the platform. If this returns false then the platform/its window was closed.
        /// If the window is unfocused this blocks until a platform message is received.
        /// </summary>
        /// <returns>Whether the platform is alive.</returns>
        public bool Update()
        {
            // Check if open.
            return IsOpen && UpdatePlatform();
        }

        protected abstract bool UpdatePlatform();

        #endregion

        #region Window API

        /// <summary>
        /// The state of the window on the screen.
        /// Whether it is maximized, minimized etc.
        /// Is set to "Normal" when the window is created.
        /// </summary>
        public abstract WindowState WindowState { get; set; }

        /// <summary>
        /// The window's display mode. Windowed, fullscreen, borderless fullscreen.
        /// Is originally set by the config.
        /// </summary>
        public DisplayMode DisplayMode
        {
            get => _mode;
            set
            {
                if (value == DisplayMode.Initial) return;
                if (value == _mode) return;
                _mode = value;

                if (_mode == DisplayMode.Fullscreen)
                    // Save window size for when exiting fullscreen.
                    _windowModeSize = Size;

                UpdateDisplayMode();
            }
        }

        protected DisplayMode _mode = DisplayMode.Initial;
        protected Vector2? _windowModeSize; // When entering a fullscreen mode the window size is stored here so it can be restored later.

        /// <summary>
        /// The position of the window on the screen.
        /// </summary>
        public Vector2 Position
        {
            get => GetPosition();
            set => SetPosition(value);
        }

        /// <summary>
        /// The size of the window in pixels. The size must be an even number on both axes.
        /// </summary>
        public Vector2 Size
        {
            get => GetSize();
            set
            {
                if (DisplayMode != DisplayMode.Windowed) return;

                Vector2 val = value.Floor();
                if (val.X % 2 != 0) val.X++;
                if (val.Y % 2 != 0) val.Y++;
                SetSize(val);
            }
        }

        protected abstract void UpdateDisplayMode();
        protected abstract Vector2 GetPosition();
        protected abstract void SetPosition(Vector2 position);
        protected abstract Vector2 GetSize();
        protected abstract void SetSize(Vector2 size);

        #endregion

        #region Input API

        protected Key[] _keyCodes;
        protected bool[] _keys;
        protected bool[] _keysPrevious;
        protected short[] _scanCodes;
        protected float _mouseScroll;
        protected float _mouseScrollThisFrame;
        protected float _mouseScrollAccum;

        // Immediate-mode input
        protected bool[] _keysIm;
        protected bool[] _keysPreviousIm;

        /// <summary>
        /// Updated every update tick, unlike the normal button logic, which is updated loop tick.
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
        }

        /// <inheritdoc />
        public EmotionEvent<Key, KeyStatus> OnKey { get; } = new EmotionEvent<Key, KeyStatus>();

        /// <inheritdoc />
        public EmotionEvent<float> OnMouseScroll { get; } = new EmotionEvent<float>();

        /// <inheritdoc />
        public EmotionEvent<char> OnTextInput { get; } = new EmotionEvent<char>();

        /// <inheritdoc />
        public Vector2 MousePosition { get; protected set; } = Vector2.Zero;

        /// <inheritdoc />
        public bool IsKeyDown(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx] && !_keysPreviousIm[idx];
        }

        /// <inheritdoc />
        public bool KeyState(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx];
        }

        /// <inheritdoc />
        public bool IsKeyHeld(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return _keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <inheritdoc />
        public bool IsKeyUp(Key key)
        {
            if (key == Key.Unknown || key == Key.KeyboardLast) return false;
            var idx = (short) key;
            return !_keysIm[idx] && _keysPreviousIm[idx];
        }

        /// <inheritdoc />
        public IEnumerable<Key> GetAllKeysHeld()
        {
            return _keys.Where((x, i) => x && _keysPreviousIm[i]).Select((x, i) => (Key) i);
        }

        /// <inheritdoc />
        public IEnumerable<Key> GetAllKeysDown()
        {
            return _keys.Where((x, i) => x && !_keysPreviousIm[i]).Select((x, i) => (Key) i);
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

        public bool IsCtrlModifierHeld()
        {
            return IsKeyHeld(Key.LeftControl) || IsKeyHeld(Key.RightControl);
        }

        public bool IsShiftModifierHeld()
        {
            return IsKeyHeld(Key.LeftShift) || IsKeyHeld(Key.RightShift);
        }

        public bool IsAltModifierHeld()
        {
            return IsKeyHeld(Key.LeftAlt) || IsKeyHeld(Key.RightAlt);
        }

        /// <inheritdoc />
        public Vector2 GetAxisHeld(Key axis)
        {
            var value = new Vector2();

            if (axis.HasFlag(Key.AxisUpDown)) value.Y = (IsKeyHeld(Key.DownArrow) ? 1 : 0) - (IsKeyHeld(Key.UpArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisLeftRight)) value.X = (IsKeyHeld(Key.RightArrow) ? 1 : 0) - (IsKeyHeld(Key.LeftArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisWS)) value.Y = value.Y == 0 ? (IsKeyHeld(Key.S) ? 1 : 0) - (IsKeyHeld(Key.W) ? 1 : 0) : value.Y;
            if (axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (IsKeyHeld(Key.D) ? 1 : 0) - (IsKeyHeld(Key.A) ? 1 : 0) : value.X;

            return value;
        }

        /// <inheritdoc />
        public Vector2 GetAxisDown(Key axis)
        {
            var value = new Vector2();

            if (axis.HasFlag(Key.AxisUpDown)) value.Y = (IsKeyDown(Key.DownArrow) ? 1 : 0) - (IsKeyDown(Key.UpArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisLeftRight)) value.X = (IsKeyDown(Key.RightArrow) ? 1 : 0) - (IsKeyDown(Key.LeftArrow) ? 1 : 0);
            if (axis.HasFlag(Key.AxisWS)) value.Y = value.Y == 0 ? (IsKeyDown(Key.S) ? 1 : 0) - (IsKeyDown(Key.W) ? 1 : 0) : value.Y;
            if (axis.HasFlag(Key.AxisAD)) value.X = value.X == 0 ? (IsKeyDown(Key.D) ? 1 : 0) - (IsKeyDown(Key.A) ? 1 : 0) : value.X;

            return value;
        }

        #endregion

        #region Library API

        /// <summary>
        /// Load a native library.
        /// </summary>
        /// <param name="path">The path to the native library.</param>
        public abstract IntPtr LoadLibrary(string path);

        /// <summary>
        /// Get the pointer which corresponds to a symbol (such as a function) in a native library.
        /// </summary>
        /// <param name="library">The pointer to a native library..</param>
        /// <param name="symbolName">The name of the symbol to find.</param>
        /// <returns>The pointer which corresponds to the library looking for.</returns>
        public abstract IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName);

        /// <summary>
        /// Get a function delegate from a library.
        /// </summary>
        public TDelegate GetFunctionByName<TDelegate>(IntPtr library, string funcName)
        {
            IntPtr funcAddress = GetLibrarySymbolPtr(library, funcName);
            return funcAddress == IntPtr.Zero ? default : Marshal.GetDelegateForFunctionPointer<TDelegate>(funcAddress);
        }

        #endregion

        /// <summary>
        /// Close the platform.
        /// This call is meant to notify the platform of a shut-down.
        /// No calls to the platform should be made afterward.
        /// </summary>
        public virtual void Close()
        {
            IsOpen = false;
            Audio?.Dispose();
        }

        #region Legacy API

        private void SetupLegacy()
        {
            OnKey.AddListener((key, status) =>
            {
#pragma warning disable 618
                if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) OnMouseKey.Invoke((MouseKey) key, status);
#pragma warning restore 618
                return true;
            });
        }

        /// <inheritdoc />
        [Obsolete("Please use OnKey instead of OnMouseKey")]
        public EmotionEvent<MouseKey, KeyStatus> OnMouseKey { get; } = new EmotionEvent<MouseKey, KeyStatus>();

        /// <inheritdoc />
        [Obsolete("Please use IsKeyDown instead of IsMouseKeyDown")]
        public bool IsMouseKeyDown(MouseKey key)
        {
            return IsKeyDown((Key) key);
        }

        /// <inheritdoc />
        [Obsolete("Please use IsKeyHeld instead of IsMouseKeyHeld")]
        public bool IsMouseKeyHeld(MouseKey key)
        {
            return IsKeyHeld((Key) key);
        }

        /// <inheritdoc />
        [Obsolete("Please use IsKeyUp instead of IsMouseKeyUp")]
        public bool IsMouseKeyUp(MouseKey key)
        {
            return IsKeyUp((Key) key);
        }

        #endregion
    }
}