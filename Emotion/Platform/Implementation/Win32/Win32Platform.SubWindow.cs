using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Audio;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Implementation.Win32.Audio;
using Emotion.Platform.Implementation.Win32.Wgl;
using Emotion.Platform.Input;
using Emotion.Utility;
using WinApi;
using WinApi.Kernel32;
using WinApi.User32;
using User32 = WinApi.User32.User32Methods;
using Kernel32 = WinApi.Kernel32.Kernel32Methods;
using WinApi.Gdi32;
using static Emotion.Platform.PlatformBase;

#nullable enable

namespace Emotion.Platform.Implementation.Win32
{
    public partial class Win32Platform
    {
        private const string SUB_WINDOW_CLASS_NAME = "Emotion_Sub";
        private List<Win32SubWindow>? _subWindows = null;
        private WindowProc? _subWndProcDelegate; // This needs to be assigned so the garbage collector doesn't collect it.

        public override bool SupportsSubWindows()
        {
            return Context is WglGraphicsContext && Engine.Renderer.Dsa;
        }

        public bool SubWindowIsFocused()
        {
            if (_subWindows == null || _subWindows.Count == 0) return false;
            if (IsFocused) return false;
            for (int i = 0; i < _subWindows.Count; i++)
            {
                var subWin = _subWindows[i];
                if (subWin.IsFocused) return true;
            }

            return false;
        }

        public override PlatformSubWindow CreateSubWindow(string title, Vector2 size)
        {
            if (!SupportsSubWindows()) return null!;
            if (_subWndProcDelegate == null) CreateSubWindowClass();

            WindowStyles windowStyle = DEFAULT_WINDOW_STYLE;
            IntPtr windowHandle = User32.CreateWindowEx(
                DEFAULT_WINDOW_STYLE_EX,
                SUB_WINDOW_CLASS_NAME,
                title,
                windowStyle,
                (int)CreateWindowFlags.CW_USEDEFAULT, (int)CreateWindowFlags.CW_USEDEFAULT, // Position - default
                (int)size.X, (int)size.Y, // Size - initial
                IntPtr.Zero, // No parent window
                IntPtr.Zero, // No window menu
                Kernel32.GetModuleHandle(null),
                IntPtr.Zero
            );

            IntPtr dc = User32.GetDC(windowHandle);
            int pixelFormatIdx = (Context as WglGraphicsContext)!.PixelFormatId;
            var pfd = new PixelFormatDescriptor();
            Gdi32Methods.SetPixelFormat(dc, pixelFormatIdx, ref pfd);

            var thisWindow = new Win32SubWindow(windowHandle, dc);
            if (thisWindow.Size == Vector2.Zero) thisWindow.Size = size;

            InternalFocusWindow(windowHandle);
            InternalSetSize(windowHandle, size);
            User32.ShowWindow(windowHandle, ShowWindowCommands.SW_SHOWNORMAL);

            _subWindows ??= new List<Win32SubWindow>();
            _subWindows.Add(thisWindow);

            return thisWindow;
        }

        private void CreateSubWindowClass()
        {
            _subWndProcDelegate = SubWindowWndProc;
            var wc = new WindowClassEx
            {
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = _subWndProcDelegate,
                InstanceHandle = Kernel32.GetModuleHandle(null),
                CursorHandle = User32.LoadCursor(IntPtr.Zero, (IntPtr)SystemCursor.IDC_ARROW),
                ClassName = SUB_WINDOW_CLASS_NAME
            };
            wc.Size = (uint)Marshal.SizeOf(wc);

            // Load user icon - if any.
            wc.IconHandle = User32.LoadImage(Kernel32.GetModuleHandle(null), "#32512", ResourceImageType.IMAGE_ICON, 0, 0, LoadResourceFlags.LR_DEFAULTSIZE | LoadResourceFlags.LR_SHARED);
            if (wc.IconHandle == IntPtr.Zero)
            {
                Kernel32.SetLastError(0);

                // None loaded - load default.
                wc.IconHandle = User32.LoadImage(IntPtr.Zero, (IntPtr)SystemIcon.IDI_APPLICATION, ResourceImageType.IMAGE_ICON, 0, 0, LoadResourceFlags.LR_DEFAULTSIZE | LoadResourceFlags.LR_SHARED);
            }

            ushort windowClass = User32.RegisterClassEx(ref wc);
            if (windowClass == 0) CheckError("Win32: Failed to register window class.", true);

            CheckError("Win32: Could not register class.");
        }

        private unsafe IntPtr SubWindowWndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            if (_subWindows != null)
            {
                PlatformBase.PlatformSubWindow? windowFound = null;
                for (int i = 0; i < _subWindows.Count; i++)
                {
                    var win = _subWindows[i];
                    if (win.WindowHandle == hWnd)
                    {
                        windowFound = win;
                        break;
                    }
                }

                if (windowFound != null)
                {
                    switch (msg)
                    {
                        case WM.GETMINMAXINFO:
                            // ReSharper disable once NotAccessedVariable
                            var mmi = (MinMaxInfo*)lParam;
                            mmi->MinTrackSize.X = 100;
                            mmi->MinTrackSize.Y = 100;

                            return IntPtr.Zero;
                        case WM.SIZE:

                            int width = NativeHelpers.LoWord((uint)lParam);
                            int height = NativeHelpers.HiWord((uint)lParam);
                            Size = new Vector2(width, height);

                            return IntPtr.Zero;
                        case WM.SETFOCUS:
                            windowFound.IsFocused = true;
                            Engine.Log.Info($"Focus gained by sub window {hWnd}.", MessageSource.Platform);

                            return IntPtr.Zero;
                        case WM.KILLFOCUS:
                            windowFound.IsFocused = false;
                            Engine.Log.Info($"Focus lost by sub window {hWnd}.", MessageSource.Platform);

                            // Pull all buttons up.
                            for (var k = 0; k < _keys.Length; k++)
                            {
                                UpdateKeyStatus((Key)k, false);
                            }
                            User32.ReleaseCapture();

                            return IntPtr.Zero;
                        case WM.CLOSE:
                            windowFound.IsOpen = false;

                            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
                    }

                    // If the window hasn't handled it, but it is focused,
                    // then invoke the main platform's WndProc as this window is basically the
                    // main window right now. This ensures the input is sent from this window when focused.
                    // This assumes that the main window sends no input when unfocused, which is a big assumption
                    // but this whole thing is hacky editor functionality - so its ok.
                    if (windowFound.IsFocused)
                    {
                        return WndProc(hWnd, msg, wParam, lParam);
                    }
                }
            }

            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    public class Win32SubWindow : PlatformSubWindow
    {
        public IntPtr WindowHandle;

        private IntPtr _subWinDC;
        private WglGraphicsContext _glContext;

        public Win32SubWindow(IntPtr handle, IntPtr dc)
        {
            WindowHandle = handle;
            _subWinDC = dc;
            _glContext = (Engine.Host.Context as WglGraphicsContext)!;

            IsOpen = true;
            IsFocused = true;
        }

        public override void MakeCurrent()
        {
            _glContext.MakeSubWindowCurrent(_subWinDC);
        }

        public override void SwapBuffers()
        {
            Gdi32Methods.SwapBuffers(_subWinDC);
        }
    }
}
