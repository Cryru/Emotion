#region Using

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.GLFW;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation.GlfwImplementation
{
    public class GlfwPlatform : DesktopPlatform
    {
        private IntPtr _glfwLibrary;
        private IntPtr _win;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.WindowFocusFun _focusCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.ErrorFun _errorCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.KeyFun _keyInputCallback;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private Glfw.FramebufferSizeFun _resizeCallback;

        [DllImport("msvcrt")]
        public static extern int _putenv_s(string e, string v);

        protected override void SetupPlatform(Configurator config)
        {
            string glfwLibPath = ResolveLibraryPath();
            _glfwLibrary = LoadLibrary(glfwLibPath);
            if (_glfwLibrary == IntPtr.Zero)
            {
                Engine.Log.Error($"{glfwLibPath} could not be loaded.", MessageSource.Glfw);
                return;
            }

            string angleLib = Path.Join("AssetsNativeLibs", "ANGLE", "win64");
            LoadLibrary(Path.Join(angleLib, "libEGL.dll"));
            LoadLibrary(Path.Join(angleLib, "libGLESv2.dll"));
            int initSuccess = Glfw.Init(_glfwLibrary);
            if (initSuccess != 1)
            {
                Engine.Log.Error($"Couldn't initialize glfw. Error code {initSuccess}.", MessageSource.Glfw);
                return;
            }

            _errorCallback = ErrorCallback;
            Glfw.SetErrorCallback(_errorCallback);

            Glfw.WindowHint(Glfw.ClientApi, Glfw.OpenglEsApi);
            Glfw.WindowHint(Glfw.ContextCreationApi, Glfw.EglContextApi);
            Glfw.WindowHint(Glfw.ContextVersionMajor, 3);
            Glfw.WindowHint(Glfw.ContextVersionMinor, 1);
            _win = Glfw.CreateWindow((int) config.HostSize.X, (int) config.HostSize.Y, config.HostTitle, IntPtr.Zero, IntPtr.Zero);
            if (_win == IntPtr.Zero)
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
        }

        protected override bool UpdatePlatform()
        {
            IsOpen = Glfw.WindowShouldClose(_win) == 0;
            Glfw.PollEvents();
            return true;
        }

        private static string ResolveLibraryPath()
        {
            string libraryPath = Path.Join("AssetsNativeLibs", "GLFW");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                libraryPath = Path.Join(libraryPath, "win");
                if (RuntimeInformation.OSArchitecture == Architecture.X64)
                    libraryPath += "64";
                else
                    libraryPath += "32";

                libraryPath = Path.Join(libraryPath, "glfw3.dll");
            }

            return libraryPath;
        }

        private static void ErrorCallback(int id, string info)
        {
            Engine.Log.Error($"{id} - {info}", MessageSource.Glfw);
        }

        private void FocusCallback(IntPtr _, int state)
        {
            UpdateFocus(state == 1);
        }

        private void KeyInput(IntPtr window, int key, int scancode, int action, int mods)
        {
            UpdateKeyStatus((Key) key, action >= 1);
        }

        #region Window API

        public override WindowState WindowState { get; set; }

        internal override void UpdateDisplayMode()
        {
            IntPtr monitor = Glfw.GetWindowMonitor(_win);
            if (monitor == IntPtr.Zero)
            {
                monitor = Glfw.GetPrimaryMonitor();
            }

            Glfw.GetMonitorPos(monitor, out int mX, out int mY);
            Glfw.VidMode vidMode = Glfw.GetVideoMode(monitor);
            switch (DisplayMode)
            {
                case DisplayMode.Fullscreen:
                    Glfw.SetWindowMonitor(_win, IntPtr.Zero, 0, 0, vidMode.Width, vidMode.Height, vidMode.RefreshRate);
                    break;
                case DisplayMode.Windowed:
                    Vector2 size = _windowModeSize ?? GetSize();
                    _windowModeSize = null;
                    Vector2 pos = new Vector2(mX, mY) + (new Vector2(vidMode.Width, vidMode.Height) / 2 - size / 2);
                    Glfw.SetWindowMonitor(_win, IntPtr.Zero, (int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y, Glfw.DontCare);
                    break;
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

        private void ResizeCallback(IntPtr _, int newSizeX, int newSizeY)
        {
            // Check if minimized.
            if (newSizeX == 0 && newSizeY == 0) return;
            OnResize.Invoke(new Vector2(newSizeX, newSizeY));
        }

        #endregion
    }
}