#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Emotion.Common;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Implementation.Win32;
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
            PopulateKeyCodes();

            SetupPlatform(config);

            // Check if the platform and graphics initialization was successful.
            if (Context == null)
            {
                Engine.CriticalError(new Exception("Platform couldn't create context."));
                return;
            }

            // Bind this window and its context.
            // "There /can/ be only one."
            Context.MakeCurrent();
            Gl.BindAPI(Context.GetProcAddress);

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
        /// </summary>
        protected bool DefaultButtonBehavior(Key key, KeyStatus state)
        {
            if (Engine.Configuration.DebugMode)
            {
                Engine.Log.Trace($"Key {key} is {state}.", MessageSource.Input);

                bool ctrl = IsKeyHeld(Key.LeftControl) || IsKeyHeld(Key.RightControl);
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

        /// <summary>
        /// Create the map for looking up keycodes.
        /// </summary>
        private void PopulateKeyCodes()
        {
            _keyCodes = new Key[512];
            _scanCodes = new short[(int) Key.Last];
            _keys = new bool[_scanCodes.Length];
            _keysPrevious = new bool[_scanCodes.Length];

            _keysIM = new bool[_scanCodes.Length];
            _keysPreviousIM = new bool[_scanCodes.Length];

            for (var i = 0; i < _scanCodes.Length; i++)
            {
                _scanCodes[i] = -1;
            }

            for (var i = 0; i < _keyCodes.Length; i++)
            {
                _keyCodes[i] = Key.Unknown;
            }

            short scanCode;

            _keyCodes[0x00B] = Key.Num0;
            _keyCodes[0x002] = Key.Num1;
            _keyCodes[0x003] = Key.Num2;
            _keyCodes[0x004] = Key.Num3;
            _keyCodes[0x005] = Key.Num4;
            _keyCodes[0x006] = Key.Num5;
            _keyCodes[0x007] = Key.Num6;
            _keyCodes[0x008] = Key.Num7;
            _keyCodes[0x009] = Key.Num8;
            _keyCodes[0x00A] = Key.Num9;
            _keyCodes[0x01E] = Key.A;
            _keyCodes[0x030] = Key.B;
            _keyCodes[0x02E] = Key.C;
            _keyCodes[0x020] = Key.D;
            _keyCodes[0x012] = Key.E;
            _keyCodes[0x021] = Key.F;
            _keyCodes[0x022] = Key.G;
            _keyCodes[0x023] = Key.H;
            _keyCodes[0x017] = Key.I;
            _keyCodes[0x024] = Key.J;
            _keyCodes[0x025] = Key.K;
            _keyCodes[0x026] = Key.L;
            _keyCodes[0x032] = Key.M;
            _keyCodes[0x031] = Key.N;
            _keyCodes[0x018] = Key.O;
            _keyCodes[0x019] = Key.P;
            _keyCodes[0x010] = Key.Q;
            _keyCodes[0x013] = Key.R;
            _keyCodes[0x01F] = Key.S;
            _keyCodes[0x014] = Key.T;
            _keyCodes[0x016] = Key.U;
            _keyCodes[0x02F] = Key.V;
            _keyCodes[0x011] = Key.W;
            _keyCodes[0x02D] = Key.X;
            _keyCodes[0x015] = Key.Y;
            _keyCodes[0x02C] = Key.Z;

            _keyCodes[0x028] = Key.Apostrophe;
            _keyCodes[0x02B] = Key.Backslash;
            _keyCodes[0x033] = Key.Comma;
            _keyCodes[0x00D] = Key.Equal;
            _keyCodes[0x029] = Key.GraveAccent;
            _keyCodes[0x01A] = Key.LeftBracket;
            _keyCodes[0x00C] = Key.Minus;
            _keyCodes[0x034] = Key.Period;
            _keyCodes[0x01B] = Key.RightBracket;
            _keyCodes[0x027] = Key.Semicolon;
            _keyCodes[0x035] = Key.Slash;
            _keyCodes[0x056] = Key.World2;

            _keyCodes[0x00E] = Key.Backspace;
            _keyCodes[0x153] = Key.Delete;
            _keyCodes[0x14F] = Key.End;
            _keyCodes[0x01C] = Key.Enter;
            _keyCodes[0x001] = Key.Escape;
            _keyCodes[0x147] = Key.Home;
            _keyCodes[0x152] = Key.Insert;
            _keyCodes[0x15D] = Key.Menu;
            _keyCodes[0x151] = Key.PageDown;
            _keyCodes[0x149] = Key.PageUp;
            _keyCodes[0x045] = Key.Pause;
            _keyCodes[0x146] = Key.Pause;
            _keyCodes[0x039] = Key.Space;
            _keyCodes[0x00F] = Key.Tab;
            _keyCodes[0x03A] = Key.CapsLock;
            _keyCodes[0x145] = Key.NumLock;
            _keyCodes[0x046] = Key.ScrollLock;
            _keyCodes[0x03B] = Key.F1;
            _keyCodes[0x03C] = Key.F2;
            _keyCodes[0x03D] = Key.F3;
            _keyCodes[0x03E] = Key.F4;
            _keyCodes[0x03F] = Key.F5;
            _keyCodes[0x040] = Key.F6;
            _keyCodes[0x041] = Key.F7;
            _keyCodes[0x042] = Key.F8;
            _keyCodes[0x043] = Key.F9;
            _keyCodes[0x044] = Key.F10;
            _keyCodes[0x057] = Key.F11;
            _keyCodes[0x058] = Key.F12;
            _keyCodes[0x064] = Key.F13;
            _keyCodes[0x065] = Key.F14;
            _keyCodes[0x066] = Key.F15;
            _keyCodes[0x067] = Key.F16;
            _keyCodes[0x068] = Key.F17;
            _keyCodes[0x069] = Key.F18;
            _keyCodes[0x06A] = Key.F19;
            _keyCodes[0x06B] = Key.F20;
            _keyCodes[0x06C] = Key.F21;
            _keyCodes[0x06D] = Key.F22;
            _keyCodes[0x06E] = Key.F23;
            _keyCodes[0x076] = Key.F24;
            _keyCodes[0x038] = Key.LeftAlt;
            _keyCodes[0x01D] = Key.LeftControl;
            _keyCodes[0x02A] = Key.LeftShift;
            _keyCodes[0x15B] = Key.LeftSuper;
            _keyCodes[0x137] = Key.PrintScreen;
            _keyCodes[0x138] = Key.RightAlt;
            _keyCodes[0x11D] = Key.RightControl;
            _keyCodes[0x036] = Key.RightShift;
            _keyCodes[0x15C] = Key.RightSuper;
            _keyCodes[0x150] = Key.Down;
            _keyCodes[0x14B] = Key.Left;
            _keyCodes[0x14D] = Key.Right;
            _keyCodes[0x148] = Key.Up;

            _keyCodes[0x052] = Key.Kp0;
            _keyCodes[0x04F] = Key.Kp1;
            _keyCodes[0x050] = Key.Kp2;
            _keyCodes[0x051] = Key.Kp3;
            _keyCodes[0x04B] = Key.Kp4;
            _keyCodes[0x04C] = Key.Kp5;
            _keyCodes[0x04D] = Key.Kp6;
            _keyCodes[0x047] = Key.Kp7;
            _keyCodes[0x048] = Key.Kp8;
            _keyCodes[0x049] = Key.Kp9;
            _keyCodes[0x04E] = Key.KpAdd;
            _keyCodes[0x053] = Key.KpDecimal;
            _keyCodes[0x135] = Key.KpDivide;
            _keyCodes[0x11C] = Key.KpEnter;
            _keyCodes[0x059] = Key.KpEqual;
            _keyCodes[0x037] = Key.KpMultiply;
            _keyCodes[0x04A] = Key.KpSubtract;

            for (scanCode = 0; scanCode < 512; scanCode++)
            {
                if (_keyCodes[scanCode] > 0)
                    _scanCodes[(short) _keyCodes[scanCode]] = scanCode;
            }
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

        protected void UpdateMouseKeyStatus(MouseKey key, bool down)
        {
            if (key == MouseKey.Unknown) return;
            int keyIndex = (short) key;
            if (keyIndex > _mouseKeys.Length - 1) return;

            // If it was down, but no longer is - it was let go.
            if (_mouseKeys[keyIndex] && !down) OnMouseKey.Invoke(key, KeyStatus.Up);

            // If it was up, and now is down - it was pressed.
            if (!_mouseKeys[keyIndex] && down) OnMouseKey.Invoke(key, KeyStatus.Down);
            _mouseKeys[keyIndex] = down;
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

                for (var i = 1; i <= _mouseKeys.Length; i++)
                {
                    UpdateMouseKeyStatus((MouseKey) i, false);
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

        internal abstract void UpdateDisplayMode();

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
        protected bool[] _mouseKeys = new bool[(int) MouseKey.Last];
        protected bool[] _mouseKeysPrevious = new bool[(int) MouseKey.Last];
        protected float _mouseScroll;
        protected float _mouseScrollThisFrame;
        protected float _mouseScrollAccum;

        // Immediate-mode input
        protected bool[] _keysIM;
        protected bool[] _keysPreviousIM;
        protected bool[] _mouseKeysIM = new bool[(int) MouseKey.Last];
        protected bool[] _mouseKeysPreviousIM = new bool[(int) MouseKey.Last];

        public virtual void UpdateInput()
        {
            // Transfer key status to previous.
            for (var i = 0; i < _keysIM.Length; i++)
            {
                _keysPreviousIM[i] = _keysIM[i];
                _keysIM[i] = _keys[i];
            }

            for (var i = 0; i < _mouseKeys.Length; i++)
            {
                _mouseKeysPreviousIM[i] = _mouseKeysIM[i];
                _mouseKeysIM[i] = _mouseKeys[i];
            }

            _mouseScrollThisFrame = _mouseScroll;
            _mouseScroll = _mouseScrollAccum;
        }

        /// <inheritdoc />
        public EmotionEvent<Key, KeyStatus> OnKey { get; } = new EmotionEvent<Key, KeyStatus>();

        /// <inheritdoc />
        public EmotionEvent<MouseKey, KeyStatus> OnMouseKey { get; } = new EmotionEvent<MouseKey, KeyStatus>();

        /// <inheritdoc />
        public EmotionEvent<float> OnMouseScroll { get; } = new EmotionEvent<float>();

        /// <inheritdoc />
        public EmotionEvent<char> OnTextInput { get; } = new EmotionEvent<char>();

        /// <inheritdoc />
        public Vector2 MousePosition { get; protected set; } = Vector2.Zero;

        /// <inheritdoc />
        public bool IsKeyDown(Key key)
        {
            if (key == Key.Unknown || key == Key.Last) return false;
            var idx = (short) key;
            return _keysIM[idx] && !_keysPreviousIM[idx];
        }

        /// <inheritdoc />
        public bool IsKeyHeld(Key key)
        {
            if (key == Key.Unknown || key == Key.Last) return false;
            var idx = (short) key;
            return _keysIM[idx] && _keysPreviousIM[idx];
        }

        /// <inheritdoc />
        public bool IsKeyUp(Key key)
        {
            if (key == Key.Unknown || key == Key.Last) return false;
            var idx = (short) key;
            return !_keysIM[idx] && _keysPreviousIM[idx];
        }

        /// <inheritdoc />
        public bool IsMouseKeyDown(MouseKey key)
        {
            if (key == MouseKey.Unknown) return false;
            int keyIndex = (short) key;
            return _mouseKeysIM[keyIndex] && !_mouseKeysPreviousIM[keyIndex];
        }

        /// <inheritdoc />
        public bool IsMouseKeyHeld(MouseKey key)
        {
            if (key == MouseKey.Unknown) return false;
            int keyIndex = (short) key;
            return _mouseKeysIM[keyIndex] && _mouseKeysPreviousIM[keyIndex];
        }

        /// <inheritdoc />
        public bool IsMouseKeyUp(MouseKey key)
        {
            if (key == MouseKey.Unknown) return false;
            int keyIndex = (short) key;
            return !_mouseKeysIM[keyIndex] && _mouseKeysPreviousIM[keyIndex];
        }

        /// <inheritdoc />
        public IEnumerable<Key> GetAllKeysHeld()
        {
            return _keys.Where((x, i) => x && _keysPreviousIM[i]).Select((x, i) => (Key) i);
        }

        /// <inheritdoc />
        public IEnumerable<Key> GetAllKeysDown()
        {
            return _keys.Where((x, i) => x && !_keysPreviousIM[i]).Select((x, i) => (Key) i);
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
    }
}