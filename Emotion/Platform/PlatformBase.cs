#region Using

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Emotion.Common;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Implementation.Win32;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using OpenGL;

#if GLFW
using Emotion.Platform.Implementation.GlfwImplementation;

#endif

#endregion

namespace Emotion.Platform
{
    public abstract partial class PlatformBase
    {
        /// <summary>
        /// Whether the platform's window is open, and considered active.
        /// </summary>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// The graphics context.
        /// </summary>
        public GraphicsContext Context { get; protected set; }

        /// <summary>
        /// The platform's audio adapter. If any.
        /// </summary>
        public IAudioAdapter Audio { get; protected set; }

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
        /// On some platforms the window cannot be unfocused.
        /// The input parameter is the new focus state.
        /// </summary>
        public Action<bool> OnFocusChanged;

        /// <summary>
        /// This event is called when the platform's display size changes.
        /// The input parameter is the new size.
        /// </summary>
        public event Action<Vector2> OnResize;

        /// <summary>
        /// Detect and return the correct platform instance for the engine host.
        /// </summary>
        /// <param name="engineConfig"></param>
        /// <returns></returns>
        public static PlatformBase CreateDetectedPlatform(Configurator engineConfig)
        {
            // ReSharper disable once RedundantAssignment
            PlatformBase platform = null;
            if (engineConfig?.PlatformOverride != null)
            {
                platform = engineConfig.PlatformOverride;
                Engine.Log.Info($"Platform override of \"{platform}\" accepted", MessageSource.Engine);
            }

#if GLFW
            platform ??= new GlfwPlatform();
#endif
            if (platform == null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) platform = new Win32Platform();

            // If none initialized - fallback to null.
            platform ??= new NullPlatform();
            Engine.Log.Info($"Platform created: {platform}", MessageSource.Engine);

            // Set platform as default native library resolver.
            NativeLibrary.SetDllImportResolver(typeof(PlatformBase).Assembly, (libName, _, _) => platform.LoadLibrary(libName));

            // Initialize platform.
            platform.Setup(engineConfig);
            Engine.Log.Trace("Platform loaded.", MessageSource.Engine);
            return platform;
        }

        /// <summary>
        /// Setup the native platform and creates a window.
        /// </summary>
        /// <param name="config">Configuration for the platform - usually passed from the engine.</param>
        protected virtual void Setup(Configurator config)
        {
            SetupInput();
            SetupInternal(config);

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
            IsOpen = true;
            _pauseOnFocusLoss = !config.DebugMode;
        }

        #region Implementation API

        /// <summary>
        /// Platform setup.
        /// </summary>
        protected abstract void SetupInternal(Configurator config);

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

        /// <summary>
        /// Whether the platform is currently the OS focus.
        /// This usually means that the platform's window is focused.
        /// On some platforms this is always true.
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        /// Whether the platform host is paused by a system event or something.
        /// Example: When the window is unfocused outside of debug mode.
        /// </summary>
        public bool HostPaused;

        /// <summary>
        /// An event to wait on for the host to be unpaused.
        /// </summary>
        public ManualResetEvent HostPausedWaiter = new(true);

        private bool _pauseOnFocusLoss;

        protected abstract void UpdateDisplayMode();
        protected abstract Vector2 GetPosition();
        protected abstract void SetPosition(Vector2 position);
        protected abstract Vector2 GetSize();
        protected abstract void SetSize(Vector2 size);

        protected void Resized(Vector2 newSize)
        {
            // Occurs when minimized on some platforms.
            if (newSize == Vector2.Zero) return;
            OnResize?.Invoke(newSize);
        }

        protected void FocusChanged(bool focused)
        {
            IsFocused = focused;
            if (focused)
            {
                Engine.Log.Info("Focus regained.", MessageSource.Platform);
                HostPausedWaiter.Set();
                HostPaused = false;
            }
            else
            {
                Engine.Log.Info("Focus lost.", MessageSource.Platform);

                if (_pauseOnFocusLoss)
                {
                    HostPausedWaiter.Reset();
                    HostPaused = true;
                }

                // Pull all buttons up.
                for (var i = 0; i < _keys.Length; i++)
                {
                    UpdateKeyStatus((Key) i, false);
                }
            }

            OnFocusChanged?.Invoke(IsFocused);
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
        }
    }
}