#region Using

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.GlfwImplementation.Native;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using WinApi.Kernel32;

#endregion

namespace Emotion.Platform.Implementation.GlfwImplementation
{
    public class GlfwPlatform : DesktopPlatform
    {
        private Glfw.Window _win;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.WindowFocusFunc _focusCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.ErrorFunc _errorCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.KeyFunc _keyInputCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.FramebufferSizeFunc _resizeCallback;

        [DllImport("msvcrt")]
        public static extern int _putenv_s(string e, string v);

        protected override void SetupPlatform(Configurator config)
        {
            bool initSuccess = Glfw.Init();
            if (!initSuccess)
            {
                Engine.Log.Error("Couldn't initialize glfw.", MessageSource.Glfw);
                return;
            }

            _errorCallback = ErrorCallback;
            Glfw.SetErrorCallback(_errorCallback);

#if ANGLE
            LoadLibrary("libEGL");
            LoadLibrary("libGLESv2");
            Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.OpenGLES);
            Glfw.WindowHint(Glfw.Hint.ContextCreationApi, Glfw.ContextApi.EGL);
            Glfw.WindowHint(Glfw.Hint.ContextVersionMajor, 3);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Kernel32Methods.GetModuleHandle("renderdoc.dll") != IntPtr.Zero) Glfw.WindowHint(Glfw.Hint.ContextVersionMinor, 1);
#endif

            _win = Glfw.CreateWindow((int) config.HostSize.X, (int) config.HostSize.Y, config.HostTitle);
            if (_win == null)
            {
                Engine.Log.Error("Couldn't create window.", MessageSource.Glfw);
                return;
            }

            _focusCallback = FocusCallback;
            Glfw.SetWindowFocusCallback(_win, _focusCallback);
            _resizeCallback = ResizeCallback;
            Glfw.SetFramebufferSizeCallback(_win, _resizeCallback);
            Context = new GlfwGraphicsContext(_win);
            Context.MakeCurrent();

            _keyInputCallback = KeyInput;
            Glfw.SetKeyCallback(_win, _keyInputCallback);

            Glfw.Monitor[] monitors = Glfw.GetMonitors();
            for (var i = 0; i < monitors.Length; i++)
            {
                Glfw.GetMonitorPos(monitors[i], out int x, out int y);
                Glfw.VideoMode videoMode = Glfw.GetVideoMode(monitors[i]);
                var mon = new GlfwMonitor(new Vector2(x, y), new Vector2(videoMode.Width, videoMode.Height));
                UpdateMonitor(mon, true, i == 0);
            }

            UpdateFocus(true);
            Glfw.FocusWindow(_win);
        }

        protected override bool UpdatePlatform()
        {
            IsOpen = !Glfw.WindowShouldClose(_win);
            Glfw.PollEvents();
            return true;
        }

        private static void ErrorCallback(Glfw.ErrorCode id, string info)
        {
            Engine.Log.Error($"{id} - {info}", MessageSource.Glfw);
        }

        private void FocusCallback(Glfw.Window _, bool state)
        {
            UpdateFocus(state);
        }

        private void KeyInput(Glfw.Window window, Glfw.KeyCode key, int scancode, Glfw.InputState action, Glfw.KeyMods mods)
        {
            UpdateKeyStatus((Key) key, action == Glfw.InputState.Press || action == Glfw.InputState.Repeat);
        }

        #region Window API

        internal override void UpdateDisplayMode()
        {
            Glfw.Monitor monitor = Glfw.GetWindowMonitor(_win);
            if (monitor.Ptr == IntPtr.Zero) monitor = Glfw.GetPrimaryMonitor();

            Glfw.GetMonitorPos(monitor, out int mX, out int mY);
            Glfw.VideoMode vidMode = Glfw.GetVideoMode(monitor);
            switch (DisplayMode)
            {
                case DisplayMode.Fullscreen:
                    // This is not actually borderless windowed :(
                    Glfw.SetWindowMonitor(_win, monitor, 0, 0, vidMode.Width, vidMode.Height, vidMode.RefreshRate);
                    break;
                case DisplayMode.Windowed:
                    Vector2 size = _windowModeSize ?? GetSize();
                    _windowModeSize = null;
                    Vector2 pos = new Vector2(mX, mY) + (new Vector2(vidMode.Width, vidMode.Height) / 2 - size / 2);
                    Glfw.SetWindowMonitor(_win, new Glfw.Monitor(IntPtr.Zero), (int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y, Glfw.DontCare);
                    break;
            }
        }

        private bool _suppressResize;

        /// <inheritdoc />
        public override WindowState WindowState
        {
            get
            {
                bool iconified = Glfw.GetWindowAttrib(_win, Glfw.WindowAttrib.Iconified);
                if (iconified) return WindowState.Minimized;

                bool maximized = Glfw.GetWindowAttrib(_win, Glfw.WindowAttrib.Maximized);
                return maximized ? WindowState.Maximized : WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Minimized:
                        Glfw.IconifyWindow(_win);
                        break;
                    case WindowState.Maximized:
                        Glfw.MaximizeWindow(_win);
                        break;
                    case WindowState.Normal:
                        _suppressResize = true;
                        if (WindowState == WindowState.Minimized) Glfw.RestoreWindow(_win);
                        _suppressResize = false;
                        Glfw.RestoreWindow(_win);
                        Glfw.FocusWindow(_win);
                        break;
                }
            }
        }

        protected override Vector2 GetPosition()
        {
            Glfw.GetWindowPos(_win, out int x, out int y);
            return new Vector2(x, y);
        }

        protected override void SetPosition(Vector2 position)
        {
            Glfw.SetWindowPos(_win, (int) position.X, (int) position.Y);
        }

        protected override Vector2 GetSize()
        {
            Glfw.GetFramebufferSize(_win, out int width, out int height);
            return new Vector2(width, height);
        }

        protected override void SetSize(Vector2 size)
        {
            Glfw.SetWindowSize(_win, (int) size.X, (int) size.Y);
        }

        private void ResizeCallback(Glfw.Window _, int newSizeX, int newSizeY)
        {
            if (_suppressResize) return;

            // Check if minimized.
            if (newSizeX == 0 && newSizeY == 0) return;
            OnResize.Invoke(new Vector2(newSizeX, newSizeY));
        }

        #endregion
    }
}