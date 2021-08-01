#region Using

#if OpenAL
using Emotion.Platform.Implementation.OpenAL;
#endif
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.GlfwImplementation.Native;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
#if ANGLE
using WinApi.Kernel32;

#endif

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
        private Glfw.CharFunc _textInputCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.FramebufferSizeFunc _resizeCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.MouseButtonFunc _mouseButtonFunc;

        //[DllImport("msvcrt")]
        //public static extern int _putenv_s(string e, string v);

        protected override void SetupInternal(Configurator config)
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Macs need a specific context to be requested.
                Glfw.WindowHint(Glfw.Hint.ContextVersionMajor, 3);
                Glfw.WindowHint(Glfw.Hint.ContextVersionMinor, 2);
                Glfw.WindowHint(Glfw.Hint.OpenglForwardCompat, true);
                Glfw.WindowHint(Glfw.Hint.OpenglProfile, Glfw.OpenGLProfile.Core);
            }

            Glfw.Window? win = Glfw.CreateWindow((int) config.HostSize.X, (int) config.HostSize.Y, config.HostTitle);
            if (win == null)
            {
                Engine.Log.Error("Couldn't create window.", MessageSource.Glfw);
                return;
            }

            _win = win.Value;

            Glfw.SetWindowSizeLimits(_win, (int) config.RenderSize.X, (int) config.RenderSize.Y, -1, -1);

            _focusCallback = FocusCallback;
            Glfw.SetWindowFocusCallback(_win, _focusCallback);
            _resizeCallback = ResizeCallback;
            Glfw.SetFramebufferSizeCallback(_win, _resizeCallback);
            Context = new GlfwGraphicsContext(_win);
            Context.MakeCurrent();

            _keyInputCallback = KeyInput;
            Glfw.SetKeyCallback(_win, _keyInputCallback);

            _mouseButtonFunc = MouseButtonKeyInput;
            Glfw.SetMouseButtonCallback(_win, _mouseButtonFunc);

            void TextInputRedirect(Glfw.Window _, uint codePoint)
            {
                UpdateTextInput((char) codePoint);
            }
            _textInputCallback = TextInputRedirect;
            Glfw.SetCharCallback(_win, _textInputCallback);

            Glfw.Monitor[] monitors = Glfw.GetMonitors();
            for (var i = 0; i < monitors.Length; i++)
            {
                Glfw.GetMonitorPos(monitors[i], out int x, out int y);
                Glfw.VideoMode videoMode = Glfw.GetVideoMode(monitors[i]);
                var mon = new GlfwMonitor(new Vector2(x, y), new Vector2(videoMode.Width, videoMode.Height));
                UpdateMonitor(mon, true, i == 0);
            }

            FocusChanged(true);
            Glfw.FocusWindow(_win);

#if OpenAL
            Audio = OpenALAudioAdapter.TryCreate(this) ?? (IAudioAdapter) new NullAudioAdapter();
#else
            Audio = new NullAudioAdapter();
#endif
        }

        protected override bool UpdatePlatform()
        {
            IsOpen = !Glfw.WindowShouldClose(_win);
            Glfw.PollEvents();
            Glfw.GetCursorPos(_win, out double xPos, out double yPos);
            MousePosition = new Vector2((float) xPos, (float) yPos);
            return true;
        }

        private static void ErrorCallback(Glfw.ErrorCode id, string info)
        {
            Engine.Log.Error($"{id} - {info}", MessageSource.Glfw);
        }

        private void FocusCallback(Glfw.Window _, bool state)
        {
            FocusChanged(state);
        }

        private void KeyInput(Glfw.Window window, Glfw.KeyCode key, int scancode, Glfw.InputState action, Glfw.KeyMods mods)
        {
            UpdateKeyStatus((Key) key, action == Glfw.InputState.Press || action == Glfw.InputState.Repeat);
        }

        private void MouseButtonKeyInput(Glfw.Window window, Glfw.MouseButton button, Glfw.InputState state, Glfw.KeyMods mods)
        {
            Key key = button switch
            {
                Glfw.MouseButton.ButtonLeft => Key.MouseKeyLeft,
                Glfw.MouseButton.ButtonRight => Key.MouseKeyRight,
                Glfw.MouseButton.Button3 => Key.MouseKeyMiddle,
                Glfw.MouseButton.Button4 => Key.MouseKey4,
                Glfw.MouseButton.Button5 => Key.MouseKey5,
                _ => Key.Unknown
            };

            UpdateKeyStatus(key, state == Glfw.InputState.Press || state == Glfw.InputState.Repeat);
        }

        #region Window API

        protected override void UpdateDisplayMode()
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
            Resized(new Vector2(newSizeX, newSizeY));
        }

        #endregion
    }
}