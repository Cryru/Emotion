#region Using

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Common;
using Emotion.Platform.Config;
using Emotion.Platform.Helpers;
using Emotion.Platform.Implementation.Null;
using Emotion.Platform.Implementation.Win32.Audio;
using Emotion.Platform.Implementation.Win32.Wgl;
using Emotion.Platform.Input;
using Emotion.Standard.Logging;
using WinApi;
using WinApi.Kernel32;
using WinApi.User32;
using User32 = WinApi.User32.User32Methods;
using Kernel32 = WinApi.Kernel32.Kernel32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32
{
    public class Win32Platform : PlatformBase
    {
        public static bool IsWindows7OrGreater { get; private set; }
        public static bool IsWindows8OrGreater { get; private set; }
        public static bool IsWindows81OrGreater { get; private set; }
        public static bool IsWindows10AnniversaryUpdateOrGreaterWin32 { get; private set; }
        public static bool IsWindows10CreatorsUpdateOrGreaterWin32 { get; private set; }

        static Win32Platform()
        {
            IsWindows7OrGreater = IsWindowsVersionOrGreaterWin32(
                NativeHelpers.HiByte((ushort)NtDll.WinVer.Win32WinNTWin7),
                NativeHelpers.LoByte((ushort)NtDll.WinVer.Win32WinNTWin7),
                0);
            IsWindows8OrGreater = IsWindowsVersionOrGreaterWin32(
                NativeHelpers.HiByte((ushort)NtDll.WinVer.Win32WinNTWin8),
                NativeHelpers.LoByte((ushort)NtDll.WinVer.Win32WinNTWin8),
                0);
            IsWindows81OrGreater = IsWindowsVersionOrGreaterWin32(
                NativeHelpers.HiByte((ushort)NtDll.WinVer.Win32WinNTWinBlue),
                NativeHelpers.LoByte((ushort)NtDll.WinVer.Win32WinNTWinBlue),
                0);
            IsWindows10AnniversaryUpdateOrGreaterWin32 = IsWindows10BuildOrGreaterWin32(14393);
            IsWindows10CreatorsUpdateOrGreaterWin32 = IsWindows10BuildOrGreaterWin32(15063);

            Engine.Log.Trace($"IsWindows7OrGreater: {IsWindows7OrGreater}", MessageSource.Win32);
            Engine.Log.Trace($"IsWindows8OrGreater: {IsWindows8OrGreater}", MessageSource.Win32);
            Engine.Log.Trace($"IsWindows81OrGreater: {IsWindows81OrGreater}", MessageSource.Win32);
            Engine.Log.Trace($"IsWindows10AnniversaryUpdateOrGreaterWin32: {IsWindows10AnniversaryUpdateOrGreaterWin32}", MessageSource.Win32);
            Engine.Log.Trace($"IsWindows10CreatorsUpdateOrGreaterWin32: {IsWindows10CreatorsUpdateOrGreaterWin32}", MessageSource.Win32);
        }

        // Default constants
        internal const string CLASS_NAME = "Emotion";
        internal const string WINDOW_IDENTIFIER = "EmotionWindow";
        internal const int DEFAULT_DPI = 96;

        // Other
        public static IntPtr HelperWindowHandle;
        private IntPtr _foregroundLockTimeout;
        private ushort _windowClass;
        private IntPtr _deviceNotificationHandle;
        private WindowProc _wndProcDelegate; // This needs to be assigned so the garbage collector doesn't collect it.

        // Input
        private Key[] _keyCodes = new Key[512];
        private bool[] _keys;
        private short[] _scanCodes;
        private string[] _keyNames;
        private bool[] _mouseKeys = new bool[3];

        /// <inheritdoc />
        protected override void SetupPlatform()
        {
            // To make SetForegroundWindow work as we want, we need to fiddle
            // with the "FOREGROUNDLOCKTIMEOUT" system setting (we do this as early
            // as possible in the hope of still being the foreground process)
            User32.SystemParametersInfo((uint)SystemParametersWindowInfo.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, ref _foregroundLockTimeout, 0);
            User32.SystemParametersInfo((uint)SystemParametersWindowInfo.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, ref NativeHelpers.Nullptr, SystemParamtersInfoFlags.SPIF_SENDWININICHANGE);

            // todo: load libraries - if any
            // probably XInput
            Audio = CreateAudio();

            PopulateKeyCodes();
            PopulateKeyNames();

            if (IsWindows10CreatorsUpdateOrGreaterWin32)
                User32.SetProcessDpiAwarenessContext(User32.DpiAwarenessContext.DpiAwarenessContextPerMonitorAwareV2);
            else if (IsWindows81OrGreater)
                User32.SetProcessDpiAwareness(User32.ProcessDpiAwareness.ProcessPerMonitorDpiAware);
            else
                User32.SetProcessDPIAware();

            RegisterWindowClass();
            CreateHelperWindow();

            PollMonitors();
        }

        private static AudioContext CreateAudio()
        {
            // Try to create WasApi - otherwise return the fake context so execution can go on.
            WasApiAudioContext wasapi = WasApiAudioContext.TryCreate();
            if (wasapi != null) return wasapi;

            return new NullAudioContext();
        }

        private void RegisterWindowClass()
        {
            _wndProcDelegate = WndProc;
            var wc = new WindowClassEx
            {
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = _wndProcDelegate,
                InstanceHandle = Kernel32.GetModuleHandle(null),
                CursorHandle = User32.LoadCursor(IntPtr.Zero, (IntPtr)SystemCursor.IDC_ARROW),
                ClassName = CLASS_NAME
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

            _windowClass = User32.RegisterClassEx(ref wc);
            if (_windowClass == 0) CheckError("Win32: Failed to register window class.", true);

            CheckError("Win32: Could not register class.");
        }

        private void CreateHelperWindow()
        {
            HelperWindowHandle = User32.CreateWindowEx(
                WindowExStyles.WS_EX_OVERLAPPEDWINDOW,
                CLASS_NAME,
                "Emotion Helper Window",
                WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN,
                0, 0, 100, 100,
                IntPtr.Zero, IntPtr.Zero,
                Kernel32.GetModuleHandle(null),
                IntPtr.Zero
            );

            if (HelperWindowHandle == IntPtr.Zero) CheckError("Win32: Failed to create helper window.", true);

            // HACK: The command to the first ShowWindow call is ignored if the parent
            //       process passed along a STARTUPINFO, so clear that with a no-op call
            User32.ShowWindow(HelperWindowHandle, ShowWindowCommands.SW_HIDE);

            // Register for HID device notifications
            var dbi = new DevBroadcastDeviceInterfaceW();
            dbi.DbccSize = (uint)Marshal.SizeOf(dbi);
            dbi.DbccDeviceType = DeviceType.DeviceInterface;
            dbi.DbccClassGuid = User32Guids.GuidDevInterfaceHid;
            _deviceNotificationHandle = User32.RegisterDeviceNotificationW(HelperWindowHandle, ref dbi, DeviceNotificationFlags.WindowHandle);

            CheckError("Registering for device notifications.");

            while (User32.PeekMessage(out Message msg, HelperWindowHandle, 0, 0, PeekMessageFlags.PM_REMOVE))
            {
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }

            Kernel32.SetLastError(0);

            CheckError("Creating helper window.");
        }

        #region Input

        /// <summary>
        /// Create the map for looking up keycodes.
        /// </summary>
        private void PopulateKeyCodes()
        {
            _scanCodes = new short[(int)Key.Last];

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
                    _scanCodes[(short)_keyCodes[scanCode]] = scanCode;
            }
        }

        /// <summary>
        /// Updates key names according to the current keyboard layout.
        /// </summary>
        private void PopulateKeyNames()
        {
            var state = new byte[256];

            // Initialize the key names array.
            _keyNames = new string[_scanCodes.Length];
            _keys = new bool[_keyNames.Length];

            for (var key = (int)Key.Space; key < (int)Key.Last; key++)
            {
                uint vk;

                int scanCode = _scanCodes[key];
                if (scanCode == -1) continue;

                if (key >= (int)Key.Kp0 && key <= (int)Key.KpAdd)
                {
                    uint[] vks =
                    {
                        (uint) VirtualKey.NUMPAD0, (uint) VirtualKey.NUMPAD1, (uint) VirtualKey.NUMPAD2, (uint) VirtualKey.NUMPAD3,
                        (uint) VirtualKey.NUMPAD4, (uint) VirtualKey.NUMPAD5, (uint) VirtualKey.NUMPAD6, (uint) VirtualKey.NUMPAD7,
                        (uint) VirtualKey.NUMPAD8, (uint) VirtualKey.NUMPAD9, (uint) VirtualKey.DECIMAL, (uint) VirtualKey.DIVIDE,
                        (uint) VirtualKey.MULTIPLY, (uint) VirtualKey.SUBTRACT, (uint) VirtualKey.ADD
                    };

                    vk = vks[key - (int)Key.Kp0];
                }
                else
                {
                    vk = User32.MapVirtualKey((uint)scanCode, VirtualKeyMapType.MAPVK_VSC_TO_VK);
                }

                var chars = new StringBuilder(16);
                int length = User32.ToUnicode(vk, (uint)scanCode, state, chars, chars.Capacity, 0);
                if (length == -1) length = User32.ToUnicode(vk, (uint)scanCode, state, chars, chars.Capacity, 0);

                if (length < 1) continue;

                var keyName = new char[5];
                Kernel32.WideCharToMultiByte(CodePage.CpUtf8, 0, chars, 1, keyName, keyName.Length, IntPtr.Zero, out bool _);

                _keyNames[key] = NativeHelpers.StringFromNullTerminated(keyName);
            }
        }

        private Key TranslateKey(ulong wParam, ulong lParam)
        {
            switch (wParam)
            {
                // Control keys require special handling.
                case (uint)VirtualKey.CONTROL when (lParam & 0x01000000) != 0:
                    return Key.RightControl;
                case (uint)VirtualKey.CONTROL:
                    {
                        uint time = User32.GetMessageTime();

                        // HACK: Alt Gr sends Left Ctrl and then Right Alt in close sequence
                        //       We only want the Right Alt message, so if the next message is
                        //       Right Alt we ignore this (synthetic) Left Ctrl message
                        if (!User32.PeekMessage(out Message next, IntPtr.Zero, 0, 0, PeekMessageFlags.PM_NOREMOVE)) return Key.LeftControl;
                        if (next.Value != WM.KEYDOWN && next.Value != WM.SYSKEYDOWN && next.Value != WM.KEYUP && next.Value != WM.SYSKEYUP) return Key.LeftControl;
                        if ((uint)next.WParam == (uint)VirtualKey.MENU && ((uint)next.LParam & 0x01000000) != 0 && next.Time == time)
                            // Next message is Right Alt down so discard this
                            return Key.Unknown;

                        return Key.LeftControl;
                    }
                // IME notifies that keys have been filtered by setting the virtual
                // key-code to VK_PROCESSKEY
                case (uint)VirtualKey.PROCESSKEY:
                    return Key.Unknown;
            }

            int index = NativeHelpers.HiWord(lParam) & 0x1FF;
            if (index < 0 || index >= _keyCodes.Length) return Key.Unknown;
            return _keyCodes[index];
        }

        private void UpdateKeyStatus(Key key, bool down)
        {
            var keyIndex = (short)key;

            // If it was down, and still is - then it's held.
            if (_keys[keyIndex] && down) OnKey.Invoke(key, KeyStatus.Held);

            // If it was down, but no longer is - it was let go.
            if (_keys[keyIndex] && !down) OnKey.Invoke(key, KeyStatus.Up);

            // If it was up, and now is down - it was pressed.
            if (!_keys[keyIndex] && down) OnKey.Invoke(key, KeyStatus.Down);
            _keys[keyIndex] = down;
        }

        private void UpdateMouseKeyStatus(MouseKey key, bool down)
        {
            if (key == MouseKey.Unknown) return;
            int keyIndex = (short)key - 1;
            if (keyIndex > _mouseKeys.Length - 1) return;

            // If it was down, but no longer is - it was let go.
            if (_mouseKeys[keyIndex] && !down) OnMouseKey.Invoke(key, KeyStatus.Up);

            // If it was up, and now is down - it was pressed.
            if (!_mouseKeys[keyIndex] && down) OnMouseKey.Invoke(key, KeyStatus.Down);
            _mouseKeys[keyIndex] = down;
        }

        /// <inheritdoc />
        public override bool GetKeyDown(Key key)
        {
            if (key == Key.Unknown || key == Key.Last) return false;
            return _keys[(short)key];
        }

        /// <inheritdoc />
        public override bool GetMouseKeyDown(MouseKey key)
        {
            if (key == MouseKey.Unknown) return false;
            int keyIndex = (short)key - 1;
            return _mouseKeys[keyIndex];
        }
        #endregion

        /// <inheritdoc />
        public override IntPtr LoadLibrary(string path)
        {
            return Kernel32.LoadLibrary(path);
        }

        /// <inheritdoc />
        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return Kernel32.GetProcAddress(library, symbolName);
        }

        /// <summary>
        /// Polls and updates the statuses of connected monitors.
        /// </summary>
        private void PollMonitors()
        {
            // All monitors are considered inactive until proven otherwise.
            var inactive = new List<Monitor>();
            inactive.AddRange(Monitors);

            for (uint adapterIndex = 0; ; adapterIndex++)
            {
                var first = false;

                var adapter = new DisplayDeviceW();
                adapter.Cb = (uint)Marshal.SizeOf(adapter);

                if (!User32.EnumDisplayDevicesW(null, adapterIndex, ref adapter, 0)) break;

                // If the adapter is inactive - continue;
                if ((adapter.StateFlags & DisplayDeviceStateFlags.Active) == 0) continue;

                if ((adapter.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0) first = true;

                uint displayIndex;
                for (displayIndex = 0; ; displayIndex++)
                {
                    var display = new DisplayDeviceW();
                    display.Cb = (uint)Marshal.SizeOf(display);

                    if (!User32.EnumDisplayDevicesW(adapter.DeviceName, displayIndex, ref display, 0)) break;

                    // If not active - continue;
                    if ((display.StateFlags & DisplayDeviceStateFlags.Active) == 0) continue;

                    // If active - remove from inactive list.
                    // If something was removed, that means this display exists in the monitor array.
                    if (inactive.RemoveAll(x => x.Name == display.DeviceName) > 0) continue;

                    UpdateMonitor(new Win32Monitor(adapter, display), true, first);
                }

                // HACK: If an active adapter does not have any display devices
                //       (as sometimes happens), add it directly as a monitor
                if (displayIndex != 0) continue;

                // Remove self from inactive list.
                if (inactive.RemoveAll(x => x.Name == adapter.DeviceName) > 0) continue;

                UpdateMonitor(new Win32Monitor(adapter, null), true, first);
            }

            foreach (Monitor inactiveMonitor in inactive)
            {
                UpdateMonitor(inactiveMonitor, false, false);
            }
        }

        /// <summary>
        /// Handler for messages.
        /// </summary>
        private unsafe IntPtr WndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            // Check if the message is for this window.
            Win32Window win = null;
            if (Window != null && hWnd == ((Win32Window)Window).Handle)
                win = (Win32Window)Window;

            if (win == null)
            {
                // This is the message handling for the hidden helper window
                // and for a regular window during its initial creation

                switch (msg)
                {
                    case WM.CREATE:
                        if (IsWindows10AnniversaryUpdateOrGreaterWin32)
                            User32.EnableNonClientDpiScaling(hWnd);
                        break;
                    case WM.DISPLAYCHANGE:
                        PollMonitors();
                        break;
                    case WM.DEVICECHANGE:
                        break;
                }

                return User32.DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch (msg)
            {
                case WM.GETMINMAXINFO:
                    Rect windowR = win.GetFullWindowRect(0, 0);
                    // ReSharper disable once NotAccessedVariable
                    var mmi = (MinMaxInfo*)lParam;

                    int offX = windowR.Right - windowR.Left;
                    int offY = windowR.Bottom - windowR.Top;

                    if (Config.MinWidth != -1 && Config.MinHeight != -1)
                    {
                        mmi->MinTrackSize.X = Config.MinWidth + offX;
                        mmi->MinTrackSize.Y = Config.MinHeight + offY;
                    }

                    if (Config.MaxWidth != -1 && Config.MaxHeight != -1)
                    {
                        mmi->MaxTrackSize.X = Config.MaxWidth + offX;
                        mmi->MaxTrackSize.Y = Config.MaxHeight + offY;
                    }

                    return IntPtr.Zero;
                case WM.SIZE:

                    int width = NativeHelpers.LoWord((uint)lParam);
                    int height = NativeHelpers.HiWord((uint)lParam);

                    // Don't send resize event when minimized.
                    if (width != 0 && height != 0) win.OnResize.Invoke(new Vector2(width, height));

                    return IntPtr.Zero;
                case WM.SETFOCUS:
                    win.Focused = true;

                    return IntPtr.Zero;
                case WM.KILLFOCUS:

                    // Pull all buttons up.
                    for (var i = 0; i < _keys.Length; i++)
                    {
                        UpdateKeyStatus((Key)i, false);
                    }

                    win.Focused = false;

                    return IntPtr.Zero;

                case WM.SYSCOMMAND:

                    switch ((int)wParam & 0xfff0)
                    {
                        case (int)SysCommand.SC_SCREENSAVE:
                        case (int)SysCommand.SC_MONITORPOWER:
                            if (Window.DisplayMode == DisplayMode.Fullscreen)
                                // We are running in full screen mode, so disallow
                                // screen saver and screen blanking
                                return IntPtr.Zero;

                            break;

                        // User trying to access application menu using ALT?
                        case (int)SysCommand.SC_KEYMENU:
                            return IntPtr.Zero;
                    }

                    break;

                case WM.CLOSE:
                    IsOpen = false;
                    return IntPtr.Zero;

                // ------------------- INPUT -------------------
                case WM.INPUTLANGCHANGE:
                    PopulateKeyNames();
                    break;

                case WM.CHAR:
                case WM.SYSCHAR:
                case WM.UNICHAR:

                    if (msg == WM.UNICHAR && (int)wParam == 0xFFFF)
                        // WM_UNICHAR is not sent by Windows, but is sent by some
                        // third-party input method engine
                        // Returning TRUE here announces support for this message
                        return (IntPtr)1;

                    OnTextInput.Invoke((char)wParam);

                    break;

                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                case WM.KEYUP:
                case WM.SYSKEYUP:

                    var lParamLong = (ulong)lParam;
                    Key key = TranslateKey((ulong)wParam, lParamLong);
                    if (key == Key.Unknown) break;

                    // Scan code can be used for debugging purposes.
                    //uint scanCode = NativeHelpers.HiWord(lParamLong) & (uint) 0x1ff;
                    bool up = (((ulong)lParam >> 31) & 1) != 0;

                    if (up && (uint)wParam == (uint)VirtualKey.SHIFT)
                    {
                        // HACK: Release both Shift keys on Shift up event, as when both
                        //       are pressed the first release does not emit any event
                        // NOTE: The other half of this is in Update()
                        UpdateKeyStatus(Key.LeftShift, false);
                        UpdateKeyStatus(Key.RightShift, false);
                    }
                    else
                    {
                        UpdateKeyStatus(key, !up);
                    }

                    break;

                case WM.MOUSEMOVE:

                    int x = NativeHelpers.LoWord((uint)lParam);
                    int y = NativeHelpers.HiWord((uint)lParam);

                    var pos = new Vector2(x, y);
                    MousePosition = Engine.Renderer != null ? Engine.Renderer.ScaleMousePosition(pos) : pos;
                    return IntPtr.Zero;

                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.XBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                case WM.XBUTTONUP:

                    var mouseKey = MouseKey.Unknown;
                    var buttonDown = false;
                    mouseKey = msg switch
                    {
                        WM.LBUTTONDOWN => MouseKey.Left,
                        WM.LBUTTONUP => MouseKey.Left,
                        WM.RBUTTONDOWN => MouseKey.Right,
                        WM.RBUTTONUP => MouseKey.Right,
                        WM.MBUTTONDOWN => MouseKey.Middle,
                        WM.MBUTTONUP => MouseKey.Middle,
                        _ => mouseKey
                    };

                    if (msg == WM.LBUTTONDOWN || msg == WM.RBUTTONDOWN ||
                        msg == WM.MBUTTONDOWN || msg == WM.XBUTTONDOWN)
                        buttonDown = true;

                    var nonePressed = true;
                    foreach (bool keyDown in _mouseKeys)
                    {
                        if (keyDown) nonePressed = false;
                    }

                    if (nonePressed)
                        User32.SetCapture(win.Handle);

                    UpdateMouseKeyStatus(mouseKey, buttonDown);

                    nonePressed = true;
                    foreach (bool keyDown in _mouseKeys)
                    {
                        if (keyDown) nonePressed = false;
                    }

                    if (nonePressed)
                        User32.ReleaseCapture();

                    if (msg == WM.XBUTTONDOWN || msg == WM.XBUTTONUP)
                        return (IntPtr)1;

                    return IntPtr.Zero;

                case WM.MOUSEWHEEL:

                    var scrollAmount = (short)NativeHelpers.HiWord((ulong)wParam);
                    OnMouseScroll.Invoke(scrollAmount / 120f);

                    return IntPtr.Zero;
            }

            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        /// <inheritdoc />
        public override bool Update()
        {
            // Check if open.
            if (!IsOpen) return false;

            // Update input.

            // NOTE: Shift keys on Windows tend to "stick" when both are pressed as
            //       no key up message is generated by the first key release
            //       The other half of this is in the handling of WM_KEYUP
            // HACK: Query actual key state and synthesize release events as needed
            KeyState lShift = User32.GetAsyncKeyState(VirtualKey.LSHIFT);
            KeyState rShift = User32.GetAsyncKeyState(VirtualKey.RSHIFT);

            // Check if it was down, but no longer is.
            if (_keys[(short)Key.LeftShift] && lShift.Value != 0)
                UpdateKeyStatus(Key.LeftShift, false);
            if (_keys[(short)Key.RightShift] && rShift.Value != 0)
                UpdateKeyStatus(Key.RightShift, false);

            while (User32.PeekMessage(out Message msg, IntPtr.Zero, 0, 0, PeekMessageFlags.PM_REMOVE))
            {
                // Things like the task manager will generate this message.
                if (msg.Value == WM.QUIT)
                {
                    IsOpen = false;
                    return false;
                }

                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }

            // Check if focused.
            if (!Window.Focused && Engine.Configuration != null && !Engine.Configuration.DebugMode) User32.WaitMessage();

            return true;
        }

        protected override Window CreateWindow()
        {
            var windowInitialSize = new Rect
            {
                Right = Config.Width,
                Bottom = Config.Height
            };
            Win32Window.GetFullWindowRect(Win32Window.DEFAULT_WINDOW_STYLE, Win32Window.DEFAULT_WINDOW_STYLE_EX, DEFAULT_DPI, ref windowInitialSize);
            int initialWidth = windowInitialSize.Right - windowInitialSize.Left;
            int initialHeight = windowInitialSize.Bottom - windowInitialSize.Top;

            IntPtr handle = User32.CreateWindowEx(
                Win32Window.DEFAULT_WINDOW_STYLE_EX,
                CLASS_NAME,
                Config.Title,
                Win32Window.DEFAULT_WINDOW_STYLE,
                (int)CreateWindowFlags.CW_USEDEFAULT, (int)CreateWindowFlags.CW_USEDEFAULT, // Position - default
                initialWidth, initialHeight, // Size - initial
                IntPtr.Zero, // No parent window
                IntPtr.Zero, // No window menu
                Kernel32.GetModuleHandle(null),
                IntPtr.Zero
            );

            if (handle == IntPtr.Zero)
            {
                CheckError("Couldn't create window.", true);
                return null;
            }

            GraphicsContext context = null;
            try
            {
                context = new WglGraphicsContext(handle, this);
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't create WGL context, falling back to MESA if possible.\n{ex}", MessageSource.Win32);
            }

            if (context == null || !context.Valid)
                try
                {
                    context = new GalliumGraphicsContext(handle, this);
                }
                catch (Exception ex)
                {
                    Engine.SubmitError(new Exception("Couldn't create MESA context.", ex));
                }

            if (context == null || !context.Valid)
            {
                Engine.SubmitError(new Exception("Couldn't create graphics context!"));
                return null;
            }

            var windowInstance = new Win32Window(handle, context, this);

            if (IsWindows7OrGreater)
            {
                User32.ChangeWindowMessageFilterEx(handle, WM.DROPFILES, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero);
                User32.ChangeWindowMessageFilterEx(handle, WM.COPYDATA, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero);
                User32.ChangeWindowMessageFilterEx(handle, WM.COPYGLOBALDATA, ChangeWindowMessageFilterExAction.Allow, IntPtr.Zero);
            }

            // Adjust window size to account for DPI scaling of the window frame and
            // optionally DPI scaling of the content area
            // This cannot be done until we know what monitor it was placed on
            var rect = new Rect
            {
                Right = Config.Width,
                Bottom = Config.Height
            };

            var leftTop = new Point
            {
                X = rect.Left,
                Y = rect.Top
            };

            var rightBottom = new Point
            {
                X = rect.Right,
                Y = rect.Bottom
            };

            User32.ClientToScreen(windowInstance.Handle, ref leftTop);
            User32.ClientToScreen(windowInstance.Handle, ref rightBottom);

            rect.Left = leftTop.X;
            rect.Top = leftTop.Y;
            rect.Right = rightBottom.X;
            rect.Bottom = rightBottom.Y;

            windowInstance.GetFullWindowRect(ref rect);

            User32.SetWindowPos(windowInstance.Handle, IntPtr.Zero,
                rect.Left, rect.Top,
                rect.Right - rect.Left, rect.Bottom - rect.Top,
                WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOZORDER);

            Shell32.DragAcceptFiles(windowInstance.Handle, true);

            return windowInstance;
        }

        /// <inheritdoc />
        public override void DisplayMessageBox(string message)
        {
            IntPtr parentWindow = IntPtr.Zero;

            if (Window != null)
                parentWindow = ((Win32Window)Window).Handle;
            else if (HelperWindowHandle != IntPtr.Zero)
                parentWindow = HelperWindowHandle;

            User32.MessageBox(parentWindow, message, "Something went wrong!", (uint)(0x00000000L | 0x00000010L));
        }

        #region Helpers

        /// <summary>
        /// Returns whether the windows version running on is at least the specified one.
        /// </summary>
        private static unsafe bool IsWindowsVersionOrGreaterWin32(int major, int minor, int sp)
        {
            var osVer = new NtDll.OsVersionInfoEXW
            { DwMajorVersion = (uint)major, DwMinorVersion = (uint)minor, DwBuildNumber = 0, DwPlatformId = 0, WServicePackMajor = (ushort)sp };
            osVer.DwOSVersionInfoSize = (uint)Marshal.SizeOf(osVer);
            osVer.SzCsdVersion[0] = (char)0;

            const NtDll.TypeMask mask = NtDll.TypeMask.VerMajorVersion | NtDll.TypeMask.VerMinorVersion | NtDll.TypeMask.VerServicePackMajor;

            ulong cond = NtDll.VerSetConditionMask(0, NtDll.TypeMask.VerMajorVersion, NtDll.Condition.VerGreaterEqual);
            cond = NtDll.VerSetConditionMask(cond, NtDll.TypeMask.VerMinorVersion, NtDll.Condition.VerGreaterEqual);
            cond = NtDll.VerSetConditionMask(cond, NtDll.TypeMask.VerServicePackMajor, NtDll.Condition.VerGreaterEqual);

            // HACK: Use RtlVerifyVersionInfo instead of VerifyVersionInfoW as the
            //       latter lies unless the user knew to embed a non-default manifest
            //       announcing support for Windows 10 via supportedOS GUID
            return NtDll.RtlVerifyVersionInfo(ref osVer, mask, cond) == 0;
        }

        private static bool IsWindows10BuildOrGreaterWin32(int build)
        {
            var osVer = new NtDll.OsVersionInfoEXW
            { DwMajorVersion = 10, DwMinorVersion = 0, DwBuildNumber = (uint)build };

            const NtDll.TypeMask mask = NtDll.TypeMask.VerMajorVersion | NtDll.TypeMask.VerMinorVersion | NtDll.TypeMask.VerBuildNumber;

            ulong cond = NtDll.VerSetConditionMask(0, NtDll.TypeMask.VerMajorVersion, NtDll.Condition.VerGreaterEqual);
            cond = NtDll.VerSetConditionMask(cond, NtDll.TypeMask.VerMinorVersion, NtDll.Condition.VerGreaterEqual);
            cond = NtDll.VerSetConditionMask(cond, NtDll.TypeMask.VerBuildNumber, NtDll.Condition.VerGreaterEqual);

            // HACK: Use RtlVerifyVersionInfo instead of VerifyVersionInfoW as the
            //       latter lies unless the user knew to embed a non-default manifest
            //       announcing support for Windows 10 via supportedOS GUID
            return NtDll.RtlVerifyVersionInfo(ref osVer, mask, cond) == 0;
        }

        public const uint ERROR_INVALID_VERSION_ARB = 0xc0070000 | 0x2095;
        public const uint ERROR_INVALID_PROFILE_ARB = 0xc0070000 | 0x2096;

        /// <summary>
        /// Check for a Win32 error.
        /// </summary>
        /// <param name="msg">A message to display with the error.</param>
        /// <param name="sureError">Whether you are sure an error occured.</param>
        internal static void CheckError(string msg, bool sureError = false)
        {
            uint errorCheck = Kernel32.GetLastError();
            if (errorCheck == 0 && !sureError) return;

            // Check predefined errors.
            switch (errorCheck)
            {
                case ERROR_INVALID_VERSION_ARB:
                    Engine.SubmitError(new Exception($"Driver doesn't support version of {msg}"));
                    break;
                case ERROR_INVALID_PROFILE_ARB:
                    Engine.SubmitError(new Exception($"Driver doesn't support profile of {msg}"));
                    break;
                default:
                    Engine.SubmitError(new Exception(msg, new Win32Exception((int)errorCheck)));
                    break;
            }
        }

        #endregion
    }
}