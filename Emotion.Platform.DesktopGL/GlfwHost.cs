#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Adfectus;
using Adfectus.Common;
using Adfectus.Common.Configuration;
using Adfectus.Graphics;
using Adfectus.Logging;
using Emotion.Platform.DesktopGL.Native;

#endregion

namespace Emotion.Platform.DesktopGL
{
    /// <inheritdoc />
    public sealed class GlfwHost : IHost
    {
        #region Properties

        /// <inheritdoc />
        public bool Focused { get; private set; }

        /// <inheritdoc />
        public Vector2 Size
        {
            get
            {
                Glfw.GetFramebufferSize(_win, out int width, out int height);
                return new Vector2(width, height);
            }
            set
            {
                Glfw.SetWindowSize(_win, (int) value.X, (int) value.Y);
                _sizeCache = value;
            }
        }

        /// <inheritdoc />
        public WindowMode WindowMode
        {
            get => _windowMode;
            set
            {
                // If borderless fullscreen, don't allow settings to be changed.
                if (_windowMode == WindowMode.Borderless) return;

                // If the GLThread is bound then apply them directly - otherwise apply then on the gl thread.
                if (!GLThread.IsBound)
                    InternalApplySettings(value);
                else
                    Task.Run(() => GLThread.ExecuteGLThread(() => InternalApplySettings(value)));
            }
        }

        /// <inheritdoc />
        public bool Open
        {
            get => Glfw.WindowShouldClose(_win) == 0;
        }

        #endregion

        #region Callbacks

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.ErrorFun _errorCallback;
        private Glfw.FramebufferSizeFun _resizeCallback;

        private Glfw.WindowFocusFun _focusCallback;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        #endregion

        public IntPtr _win;
        private Vector2 _sizeCache;
        private WindowMode _windowMode;

        public GlfwHost(EngineBuilder builder)
        {
            // Default size.
            _sizeCache = builder.HostSize;

            // Init glfw.
            int initSuccess = Glfw.Init(NativeLoader.LoadedLibraries["glfw"]);
            if (initSuccess != 1)
            {
                ErrorHandler.SubmitError(new Exception("GLFW couldn't initialize."));
                return;
            }

            _errorCallback = ErrorCallback;
            Glfw.SetErrorCallback(_errorCallback);

            // Create a list of window configurations to try.
            List<WindowConfig> windowConfigurations = new List<WindowConfig>
            {
                new WindowConfig {VersionMajor = 4, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCoreProfile},
                new WindowConfig {VersionMajor = 4, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCompatProfile},
                new WindowConfig {VersionMajor = 4, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile},
                new WindowConfig {VersionMajor = 4, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},

                new WindowConfig {VersionMajor = 3, VersionMinor = 3, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCoreProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 3, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCompatProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 3, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 3, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},

                new WindowConfig {VersionMajor = 3, VersionMinor = 2, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCoreProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 2, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCompatProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 2, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 2, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},

                new WindowConfig {VersionMajor = 3, VersionMinor = 1, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCoreProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 1, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCompatProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 1, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 1, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},

                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCoreProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglCompatProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile},
                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},

                // ES randomly.
                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglEsApi},

                // "Unsupported" fallbacks.
                new WindowConfig {VersionMajor = 3, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},
                new WindowConfig {VersionMajor = 2, VersionMinor = 1, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},
                new WindowConfig {VersionMajor = 2, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false},
                new WindowConfig {VersionMajor = 1, VersionMinor = 0, Api = Glfw.OpenglApi, Profile = Glfw.OpenglAnyProfile, ForwardCompat = false}
            };

            // Check if the RenderDoc debugger is attached. In this case we need to create a specific context version otherwise it won't work.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                if (WindowsNative.GetModuleHandle("renderdoc.dll") != IntPtr.Zero)
                {
                    Engine.Log.Warning("Detected render doc. Adding RenderDoc window config.", MessageSource.Engine);
                    windowConfigurations.Insert(0, new WindowConfig
                    {
                        VersionMajor = 3,
                        VersionMinor = 3,
                        Api = Glfw.OpenglApi,
                        Debug = true,
                        Profile = Glfw.OpenglCoreProfile
                    });
                }

            // The window shouldn't be visible at the start.
            Glfw.WindowHint(Glfw.Visible, 0);

            // Check if creating a borderless fullscreen window.
            if (builder.HostWindowMode == WindowMode.Borderless) Glfw.WindowHint(Glfw.Decorated, 0);

            // Try to create a window using all available configurations.
            foreach (WindowConfig currentConfig in windowConfigurations)
            {
                // Try to create the window with this config.
                Glfw.WindowHint(Glfw.ContextVersionMajor, currentConfig.VersionMajor);
                Glfw.WindowHint(Glfw.ContextVersionMinor, currentConfig.VersionMinor);
                Glfw.WindowHint(Glfw.ClientApi, currentConfig.Api);
                Glfw.WindowHint(Glfw.OpenglDebugContext, currentConfig.Debug ? 1 : 0);
                Glfw.WindowHint(Glfw.OpenglProfile, currentConfig.Profile);
                Glfw.WindowHint(Glfw.OpenglForwardCompat, currentConfig.ForwardCompat ? 1 : 0);

                _win = Glfw.CreateWindow((int) _sizeCache.X, (int) _sizeCache.Y, builder.HostTitle, IntPtr.Zero, IntPtr.Zero);

                // Check if created.
                if (_win == IntPtr.Zero)
                {
                    Engine.Log.Warning($"Failed to create GLFW window. Config - {currentConfig}", MessageSource.Host);
                }
                else
                {
                    // Set flags based on which context version was created.
                    if (currentConfig.VersionMajor < 3)
                    {
                        Engine.Flags.RenderFlags.SetVertexAttribLocations = true;
                        Engine.Flags.RenderFlags.UseVao = false;
                        Engine.Flags.RenderFlags.UseFramebuffer = false;
                        Engine.Flags.RenderFlags.TextureLoadStandard = false;
                    }

                    Engine.Log.Info($"Created GLFW window using config - {currentConfig}.", MessageSource.Host);
                    break;
                }
            }

            // All the configurations were passed and no window was created.
            if (_win == IntPtr.Zero)
            {
                ErrorHandler.SubmitError(new Exception("Couldn't create GLFW window. Please update your graphics drivers."));
                return;
            }

            // Attach callbacks.
            _resizeCallback = ResizeCallback;
            Glfw.SetFramebufferSizeCallback(_win, _resizeCallback);

            _focusCallback = FocusCallback;
            Glfw.SetWindowFocusCallback(_win, _focusCallback);

            // Check if creating a borderless fullscreen window.
            if (builder.HostWindowMode == WindowMode.Borderless)
            {
                // Move to 0,0 and expand to monitor size.
                Glfw.VidMode videoMode = Glfw.GetVideoMode(Glfw.GetPrimaryMonitor());
                Glfw.SetWindowSize(_win, videoMode.Width, videoMode.Height);
                Glfw.SetWindowPos(_win, 0, 0);
            }
            else
            {
                // Center the window.
                Vector2 center = GetCenterPos();
                Glfw.SetWindowPos(_win, (int) center.X, (int) center.Y);
            }

            // Show the window.
            Glfw.SetWindowSizeLimits(_win, 100, 100, Glfw.DontCare, Glfw.DontCare);
            Glfw.ShowWindow(_win);
            Glfw.FocusWindow(_win);

            // Make the OpenGL context run on this thread.
            Glfw.MakeContextCurrent(_win);

            // Set window mode. For borderless this is set during creation and cannot be changed.
            WindowMode = builder.HostWindowMode;
        }

        private void InternalApplySettings(WindowMode wm)
        {
            lock (this)
            {
                Engine.Log.Trace($"Changing window mode from {_windowMode} to {wm}.", MessageSource.Host);

                switch (wm)
                {
                    case WindowMode.Windowed:
                    {
                        // Setting window monitor no none - and size to the size cache.
                        Glfw.SetWindowMonitor(_win, IntPtr.Zero, 0, 0, (int) _sizeCache.X, (int) _sizeCache.Y, Glfw.DontCare);

                        // Center the window.
                        Vector2 center = GetCenterPos((int) _sizeCache.X, (int) _sizeCache.Y);
                        Glfw.SetWindowPos(_win, (int) center.X, (int) center.Y);
                        _windowMode = WindowMode.Windowed;
                        break;
                    }
                    case WindowMode.Fullscreen:
                    case WindowMode.Borderless:
                    {
                        // Cache size.
                        _sizeCache = Size;

                        // Change to fullscreen.
                        Glfw.VidMode videoMode = Glfw.GetVideoMode(Glfw.GetPrimaryMonitor());
                        Glfw.SetWindowMonitor(_win, Glfw.GetPrimaryMonitor(), 0, 0, videoMode.Width, videoMode.Height, videoMode.RefreshRate);

                        _windowMode = WindowMode.Fullscreen;
                        break;
                    }
                }

                // Set vsync and save the window mode to the local variable.
                Glfw.SwapInterval(1);
            }
        }

        /// <inheritdoc />
        public void SwapBuffers()
        {
            Glfw.SwapBuffers(_win);
        }

        /// <inheritdoc />
        public Vector2 GetScreenSize()
        {
            Glfw.VidMode videoMode = Glfw.GetVideoMode(Glfw.GetPrimaryMonitor());
            return new Vector2(videoMode.Width, videoMode.Height);
        }

        /// <inheritdoc />
        public void DisplayErrorMessage(string message)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    WindowsNative.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) (0x00000000L | 0x00000010L));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    UnixNative.ExecuteBashCommand($"osascript -e 'tell app \"System Events\" to display dialog \"{message}\" buttons {{\"OK\"}} with icon caution'");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    try
                    {
                        // Display a message box using Zenity.
                        UnixNative.ExecuteBashCommand($"zenity --error --text=\"{message}\" --title=\"Something went wrong!\" 2>/dev/null");
                    }
                    catch (Exception)
                    {
                        // Fallback to xmessage.
                        UnixNative.ExecuteBashCommand($"xmessage \"{message}\"");
                    }
            }
            catch (Exception e)
            {
                Engine.Log.Error($"Couldn't display error message box - {message}. {e}", MessageSource.Host);
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            Glfw.PollEvents();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Go out of fullscreen - if the closing hangs and the window is in fullscreen mode you're going to have a bad time.
            Glfw.SetWindowMonitor(_win, IntPtr.Zero, 0, 0, 100, 100, Glfw.DontCare);

            // Make window close.
            Glfw.SetWindowShouldClose(_win, 1);
        }

        #region GLFW Callbacks

        private void ErrorCallback(int id, string info)
        {
            Engine.Log.Error($"Glfw Error: {id} - {info}", MessageSource.Host);
        }

        private void ResizeCallback(IntPtr _, int newSizeX, int newSizeY)
        {
            // Check if minimized.
            if (newSizeX == 0 && newSizeY == 0) return;

            Engine.Renderer?.HostResized(newSizeX, newSizeY);
        }

        private void FocusCallback(IntPtr _, int state)
        {
            Focused = state == 1;
        }

        #endregion

        /// <summary>
        /// Returns the central position of the screen in order for the window to be centered.
        /// </summary>
        /// <param name="windowWidth">The width of the window. If passed as 0 it is gotten from GetWindowSize.</param>
        /// <param name="windowHeight">The height of the window. If passed as 0 it is gotten from GetWindowSize.</param>
        /// <returns>The position of the window in order to be centered in the screen.</returns>
        private Vector2 GetCenterPos(int windowWidth = 0, int windowHeight = 0)
        {
            IntPtr primaryMonitor = Glfw.GetPrimaryMonitor();
            Glfw.VidMode vidMode = Glfw.GetVideoMode(primaryMonitor);
            Glfw.GetMonitorPos(primaryMonitor, out int xOffset, out int yOffset);

            if (windowWidth == 0 || windowHeight == 0)
            {
                Glfw.GetWindowSize(_win, out int width, out int height);
                if (windowWidth == 0) windowWidth = width;
                if (windowHeight == 0) windowHeight = height;
            }

            int xPos = vidMode.Width / 2 - windowWidth / 2;
            int yPos = vidMode.Height / 2 - windowHeight / 2;

            return new Vector2(xOffset + xPos, yOffset + yPos);
        }
    }

    public class WindowConfig
    {
        public int VersionMajor = 1;
        public int VersionMinor;
        public int Api = Glfw.OpenglApi;
        public bool Debug;
        public int Profile = Glfw.OpenglCoreProfile;
        public bool ForwardCompat = true;

        public override string ToString()
        {
            return $"GL: {VersionMajor}.{VersionMinor} API: {Api} Debug: {Debug} Profile: {Profile}";
        }
    }
}