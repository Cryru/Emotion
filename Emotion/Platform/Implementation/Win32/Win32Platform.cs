#region Using

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
using Emotion.IO;


#if ANGLE
using Emotion.Platform.Implementation.EglAngle;
#endif

#if OpenAL
using Emotion.Platform.Implementation.OpenAL;
#endif

#endregion

namespace Emotion.Platform.Implementation.Win32
{
    public partial class Win32Platform : DesktopPlatform
    {
        public static bool IsWindows7OrGreater { get; private set; }
        public static bool IsWindows8OrGreater { get; private set; }
        public static bool IsWindows81OrGreater { get; private set; }
        public static bool IsWindows10AnniversaryUpdateOrGreaterWin32 { get; private set; }
        public static bool IsWindows10CreatorsUpdateOrGreaterWin32 { get; private set; }

        // Default constants
        internal const string CLASS_NAME = "Emotion";
        internal const string WINDOW_IDENTIFIER = "EmotionWindow";
        internal const int DEFAULT_DPI = 96;

        // Resources
        private IntPtr _helperWindowHandle;
        private IntPtr _windowHandle;
        private IntPtr _deviceNotificationHandle;

        /// <inheritdoc />
        protected override void SetupInternal(Configurator config)
        {
            // Ok so, this should exist everywhere as it is part of Windows.
            // However it seems some people don't have it or it's corrupted or something.
            // Now usually I'd say we don't need to write code for some broken installations of Windows
            // but...and hear me out, we only use NtDll to check the windows version...so does it really matter?
            // I mean we can skip it and have games work on a couple machines more. That's an easy win in my book.
            // (also chrome manages to run on them somehow, so I guess it's doable)
            bool ntDllExists = NativeLibrary.TryLoad(NtDll.LIBRARY_NAME, out IntPtr handle);
            if (ntDllExists)
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
            }
            else
            {
                Engine.Log.Warning($"No ntdll - fun :)", MessageSource.Win32);
            }

            var windowsVersionFlags = new List<string>();
            if (IsWindows7OrGreater) windowsVersionFlags.Add(nameof(IsWindows7OrGreater));
            if (IsWindows8OrGreater) windowsVersionFlags.Add(nameof(IsWindows8OrGreater));
            if (IsWindows81OrGreater) windowsVersionFlags.Add(nameof(IsWindows81OrGreater));
            if (IsWindows10AnniversaryUpdateOrGreaterWin32) windowsVersionFlags.Add(nameof(IsWindows10AnniversaryUpdateOrGreaterWin32));
            if (IsWindows10CreatorsUpdateOrGreaterWin32) windowsVersionFlags.Add(nameof(IsWindows10CreatorsUpdateOrGreaterWin32));
            Engine.Log.Trace(string.Join(", ", windowsVersionFlags), MessageSource.Win32);

            if (IsWindows10CreatorsUpdateOrGreaterWin32)
                User32.SetProcessDpiAwarenessContext(DpiAwarenessContext.DpiAwarenessContextPerMonitorAwareV2);
            else if (IsWindows81OrGreater)
                User32.SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
            else
                User32.SetProcessDPIAware();

            // todo: load libraries - if any, probably XInput?

            // Initialize audio.
            AudioContext ctx = null;
#if OpenAL
            ctx ??= OpenALAudioAdapter.TryCreate(this);
#endif
            ctx ??= WasApiAudioContext.TryCreate(this);
            ctx ??= new NullAudioContext(this);
            Audio = ctx;
            Engine.Log.Trace("Audio init complete.", MessageSource.Win32);

            PopulateKeyCodes();
            RegisterWindowClass();
            CreateHelperWindow();
            PollMonitors();
            Engine.Log.Trace("Platform helpers created.", MessageSource.Win32);

            WindowStyles windowStyle = DEFAULT_WINDOW_STYLE;
            if (config.HiddenWindow)
            {
                windowStyle &= ~WindowStyles.WS_VISIBLE;
                windowStyle &= ~WindowStyles.WS_MINIMIZE;

                // This will override the hide otherwise
                config.InitialDisplayMode = DisplayMode.Initial;
            }

            IntPtr windowHandle = User32.CreateWindowEx(
                DEFAULT_WINDOW_STYLE_EX,
                CLASS_NAME,
                config.HostTitle,
                windowStyle,
                (int) CreateWindowFlags.CW_USEDEFAULT, (int) CreateWindowFlags.CW_USEDEFAULT, // Position - default
                (int) config.HostSize.X, (int) config.HostSize.Y, // Size - initial
                IntPtr.Zero, // No parent window
                IntPtr.Zero, // No window menu
                Kernel32.GetModuleHandle(null),
                IntPtr.Zero
            );
            if (windowHandle == IntPtr.Zero)
            {
                CheckError("Couldn't create window.", true);
                return;
            }

            Engine.Log.Trace("Window created.", MessageSource.Win32);

            string[] args = config.GetExecutionArguments();
            bool forceAngle = CommandLineParser.FindArgument(args, "angle", out string _);
            bool forceMesa = CommandLineParser.FindArgument(args, "software", out string _);

            // Create graphics context.

            if (!forceAngle && !forceMesa)
                try
                {
                    var wgl = new WglGraphicsContext();
                    wgl.Init(User32.GetDC(_helperWindowHandle), windowHandle, this);
                    if (wgl.Valid) Context = wgl;
                }
                catch (Exception ex)
                {
                    Engine.Log.Warning($"Couldn't create WGL context, falling back if possible.\n{ex}", MessageSource.Win32);
                }

#if ANGLE
            if (Context == null && !forceMesa)
                try
                {
                    var egl = new EglGraphicsContext();
                    egl.Init(User32.GetDC(windowHandle), windowHandle, this);
                    if (egl.Valid) Context = egl;
                }
                catch (Exception ex)
                {
                    Engine.Log.Warning($"Couldn't create EGL context, falling back if possible.\n{ex}", MessageSource.Win32);
                }
#endif

            if (Context == null)
                try
                {
                    var gallium = new GalliumGraphicsContext();
                    gallium.Init(windowHandle, this);
                    if (gallium.Valid) Context = gallium;
                }
                catch (Exception ex)
                {
                    Engine.CriticalError(new Exception("Couldn't create MESA context.", ex));
                }

            if (Context == null)
            {
                Engine.CriticalError(new Exception("Couldn't create graphics context!"));
                return;
            }

            _windowHandle = windowHandle;
        }

        /// <summary>
        /// Polls and updates the statuses of connected monitors.
        /// </summary>
        private void PollMonitors()
        {
            // All monitors are considered inactive until proven otherwise.
            var inactive = new List<Monitor>();
            inactive.AddRange(Monitors);

            for (uint adapterIndex = 0;; adapterIndex++)
            {
                var first = false;

                var adapter = new DisplayDeviceW();
                adapter.Cb = (uint) Marshal.SizeOf(adapter);

                if (!User32.EnumDisplayDevicesW(null, adapterIndex, ref adapter, 0)) break;

                // If the adapter is inactive - continue;
                if ((adapter.StateFlags & DisplayDeviceStateFlags.Active) == 0) continue;

                if ((adapter.StateFlags & DisplayDeviceStateFlags.PrimaryDevice) != 0) first = true;

                uint displayIndex;
                for (displayIndex = 0;; displayIndex++)
                {
                    var display = new DisplayDeviceW();
                    display.Cb = (uint) Marshal.SizeOf(display);

                    if (!User32.EnumDisplayDevicesW(adapter.DeviceName, displayIndex, ref display, 0)) break;

                    // If not active - continue;
                    if ((display.StateFlags & DisplayDeviceStateFlags.Active) == 0) continue;

                    // If active - remove from inactive list.
                    // If something was removed, that means this display exists in the monitor array.
                    if (inactive.RemoveAll(x => x.Name == display.DeviceName) > 0) continue;

                    UpdateMonitor(new Win32Monitor(adapter, display), true, first);
                }

                // HACK: If an active adapter does not have any display devices (as sometimes happens), add it directly as a monitor.
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

        #region API

        /// <inheritdoc />
        protected override bool UpdatePlatform()
        {
            // Shift keys on windows tend to "stick"
            // When one key is held, the other doesn't receive down events, and an up event
            // is sent only to the latter one. To work around this manually query the status of both shift keys every tick.
            if (IsFocused)
            {
                Win32KeyState lShift = User32.GetAsyncKeyState(VirtualKey.LSHIFT);
                UpdateKeyStatus(Key.LeftShift, lShift.IsPressed);
                Win32KeyState rShift = User32.GetAsyncKeyState(VirtualKey.RSHIFT);
                UpdateKeyStatus(Key.RightShift, rShift.IsPressed);
            }

            while (User32.PeekMessage(out Message msg, IntPtr.Zero, 0, 0, PeekMessageFlags.PM_REMOVE))
            {
                // Generated by task manager and stuff like that.
                // The WM_QUIT message is not associated with a window and therefore will never be received through a window's window procedure.
                if (msg.Value == WM.QUIT)
                {
                    IsOpen = false;
                    return false;
                }

                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }

            if (HostPaused) User32.WaitMessage();

            return true;
        }

        #endregion

        #region Helpers

        private void RegisterWindowClass()
        {
            _wndProcDelegate = WndProc;
            var wc = new WindowClassEx
            {
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = _wndProcDelegate,
                InstanceHandle = Kernel32.GetModuleHandle(null),
                CursorHandle = User32.LoadCursor(IntPtr.Zero, (IntPtr) SystemCursor.IDC_ARROW),
                ClassName = CLASS_NAME
            };
            wc.Size = (uint) Marshal.SizeOf(wc);

            // Load user icon - if any.
            wc.IconHandle = User32.LoadImage(Kernel32.GetModuleHandle(null), "#32512", ResourceImageType.IMAGE_ICON, 0, 0, LoadResourceFlags.LR_DEFAULTSIZE | LoadResourceFlags.LR_SHARED);
            if (wc.IconHandle == IntPtr.Zero)
            {
                Kernel32.SetLastError(0);

                // None loaded - load default.
                wc.IconHandle = User32.LoadImage(IntPtr.Zero, (IntPtr) SystemIcon.IDI_APPLICATION, ResourceImageType.IMAGE_ICON, 0, 0, LoadResourceFlags.LR_DEFAULTSIZE | LoadResourceFlags.LR_SHARED);
            }

            ushort windowClass = User32.RegisterClassEx(ref wc);
            if (windowClass == 0) CheckError("Win32: Failed to register window class.", true);

            CheckError("Win32: Could not register class.");
        }

        private void CreateHelperWindow()
        {
            _helperWindowHandle = User32.CreateWindowEx(
                WindowExStyles.WS_EX_OVERLAPPEDWINDOW,
                CLASS_NAME,
                "Emotion Helper Window",
                WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN,
                0, 0, 100, 100,
                IntPtr.Zero, IntPtr.Zero,
                Kernel32.GetModuleHandle(null),
                IntPtr.Zero
            );

            if (_helperWindowHandle == IntPtr.Zero) CheckError("Win32: Failed to create helper window.", true);

            // HACK: The command to the first ShowWindow call is ignored if the parent
            //       process passed along a STARTUPINFO, so clear that with a no-op call
            User32.ShowWindow(_helperWindowHandle, ShowWindowCommands.SW_HIDE);

            // Register for HID device notifications
            var dbi = new DevBroadcastDeviceInterfaceW();
            dbi.DbccSize = (uint) Marshal.SizeOf(dbi);
            dbi.DbccDeviceType = DeviceType.DeviceInterface;
            dbi.DbccClassGuid = User32Guids.GuidDevInterfaceHid;
            _deviceNotificationHandle = User32.RegisterDeviceNotificationW(_helperWindowHandle, ref dbi, DeviceNotificationFlags.WindowHandle);

            CheckError("Registering for device notifications.");

            while (User32.PeekMessage(out Message msg, _helperWindowHandle, 0, 0, PeekMessageFlags.PM_REMOVE))
            {
                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }

            Kernel32.SetLastError(0);

            CheckError("Creating helper window.");
        }

        /// <summary>
        /// Returns whether the windows version running on is at least the specified one.
        /// </summary>
        private static unsafe bool IsWindowsVersionOrGreaterWin32(int major, int minor, int sp)
        {
            var osVer = new NtDll.OsVersionInfoEXW
                {DwMajorVersion = (uint) major, DwMinorVersion = (uint) minor, DwBuildNumber = 0, DwPlatformId = 0, WServicePackMajor = (ushort) sp};
            osVer.DwOSVersionInfoSize = (uint) Marshal.SizeOf(osVer);
            osVer.SzCsdVersion[0] = (char) 0;

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
                {DwMajorVersion = 10, DwMinorVersion = 0, DwBuildNumber = (uint) build};

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
        public static void CheckError(string msg, bool sureError = false, bool critical = true)
        {
            uint errorCheck = Kernel32.GetLastError();
            if (errorCheck == 0 && !sureError) return;

            // Check predefined errors.
            Exception exToLog;
            switch (errorCheck)
            {
                case ERROR_INVALID_VERSION_ARB:
                    exToLog = new Exception($"Driver doesn't support version of {msg}");
                    break;
                case ERROR_INVALID_PROFILE_ARB:
                    exToLog = new Exception($"Driver doesn't support profile of {msg}");
                    break;
                default:
                    exToLog = new Exception(msg, new Win32Exception((int) errorCheck));
                    break;
            }

            if (critical)
                Engine.CriticalError(exToLog);
            else
                Engine.Log.Error(exToLog);
        }

        #endregion

        #region Input

        private void PopulateKeyCodes()
        {
            _keyCodes = new Key[512];

            for (var i = 0; i < _keyCodes.Length; i++)
            {
                _keyCodes[i] = Key.Unknown;
            }

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
            _keyCodes[0x150] = Key.DownArrow;
            _keyCodes[0x14B] = Key.LeftArrow;
            _keyCodes[0x14D] = Key.RightArrow;
            _keyCodes[0x148] = Key.UpArrow;

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
        }

        /// <summary>
        /// Converts Win32 key event params to a member of the Key enum.
        /// </summary>
        private Key TranslateKey(VirtualKey virtualKey, ulong lParam)
        {
            switch (virtualKey)
            {
                // Control keys require special handling.
                case VirtualKey.CONTROL when (lParam & 0x01000000) != 0:
                    return Key.RightControl;
                case VirtualKey.CONTROL:
                {
                    // HACK: Alt Gr sends Left Ctrl and then Right Alt in close sequence
                    //       We only want the Right Alt message, so if the next message is
                    //       Right Alt we ignore this (synthetic) Left Ctrl message
                    uint time = User32.GetMessageTime();
                    if (!User32.PeekMessage(out Message next, IntPtr.Zero, 0, 0, PeekMessageFlags.PM_NOREMOVE)) return Key.LeftControl;
                    if (next.Value != WM.KEYDOWN && next.Value != WM.SYSKEYDOWN && next.Value != WM.KEYUP && next.Value != WM.SYSKEYUP) return Key.LeftControl;
                    if ((uint) next.WParam == (uint) VirtualKey.MENU && ((uint) next.LParam & 0x01000000) != 0 && next.Time == time)
                        // Next message is Right Alt down so discard this
                        return Key.Unknown;

                    return Key.LeftControl;
                }
                // IME notifies that keys have been filtered by setting this key.
                case VirtualKey.PROCESSKEY:
                    return Key.Unknown;
            }

            int index = NativeHelpers.HiWord(lParam) & 0x1FF;
            if (index < 0 || index >= _keyCodes.Length) return Key.Unknown;
            return _keyCodes[index];
        }

        #endregion

        #region File Helpers

        protected override string DeveloperMode_OSSelectFileToImport<T>()
        {
            const int MAX_PATH = 260;

            OpenFileName ofn = new OpenFileName();
            ofn.lStructSize = Marshal.SizeOf<OpenFileName>();

            string[] extensions = T.GetFileExtensionsSupported();
            StringBuilder b = new();
            if (extensions.Length > 0)
            {
                string typeName = typeof(T).Name;
                b.Append(typeName);
                b.Append(" (");

                for (int i = 0; i < extensions.Length; i++)
                {
                    string ext = extensions[i];
                    if (i != 0) b.Append(", ");
                    b.Append($"*.{ext}");
                }

                b.Append(")\0");

                for (int i = 0; i < extensions.Length; i++)
                {
                    string ext = extensions[i];
                    if (i != 0) b.Append(";");
                    b.Append($"*.{ext}");
                }

                b.Append(")\0");
            }
            b.Append("All Files (*.*)\0*.*\0");

            ofn.lpstrFilter = b.ToString();
            ofn.nFilterIndex = 1;

            ofn.lpstrFile = new string(new char[MAX_PATH]);
            ofn.nMaxFile = MAX_PATH;

            ofn.lpstrFileTitle = null;
            ofn.nMaxFileTitle = MAX_PATH;

            ofn.lpstrTitle = "Emotion: Select a file";

            string assetPath = _devModeAssetFolder;
            string fullAssetPath = Path.GetFullPath(assetPath, AssetLoader.GameDirectory);
            ofn.lpstrInitialDir = fullAssetPath;

            ofn.Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR;

            if (GetOpenFileName(ref ofn))
                return ofn.lpstrFile;

            return string.Empty;
        }

        public void OpenFolderAndSelectFile(string filePath)
        {
            if (Path.EndsInDirectorySeparator(filePath))
            {
                Process.Start("explorer", $"\"{filePath}\"");
                return;
            }

            SHParseDisplayName(Path.GetDirectoryName(filePath), IntPtr.Zero, out IntPtr nativeFolder, 0, out _);
            SHParseDisplayName(filePath, IntPtr.Zero, out IntPtr nativeFile, 0, out _);
            SHOpenFolderAndSelectItems(nativeFile, 0, nativeFile, 0);
            Marshal.FreeCoTaskMem(nativeFolder);
            Marshal.FreeCoTaskMem(nativeFile);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern void SHParseDisplayName(string name, IntPtr bindingContext, out IntPtr pidl, uint sfgaoIn, out uint psfgaoOut);

        [DllImport("shell32.dll")]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

        public AssertMessageBoxResponse OpenAssertMessageBox(string message)
        {
            MessageBoxResult result = User32.MessageBox(_windowHandle, message, "Assert", (uint) MessageBoxFlags.MB_ABORTRETRYIGNORE);
            switch (result)
            {
                case MessageBoxResult.IDABORT:
                    return AssertMessageBoxResponse.Break;
                case MessageBoxResult.IDRETRY:
                    return AssertMessageBoxResponse.IgnoreCurrent;
                case MessageBoxResult.IDIGNORE:
                    return AssertMessageBoxResponse.IgnoreAll;
            }

            return AssertMessageBoxResponse.Break;
        }

        // From https://www.pinvoke.net/default.aspx/Structures/OPENFILENAME.html
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetOpenFileName(ref OpenFileName lpofn);

        private const int OFN_FILEMUSTEXIST = 0x00001000;
        private const int OFN_PATHMUSTEXIST = 0x00000800;
        private const int OFN_NOCHANGEDIR = 0x00000008;

        #endregion

        #region Clipboard

        public override void SetClipboard(string data)
        {
            User32.OpenClipboard(_helperWindowHandle);
            User32.EmptyClipboard();

            var dataAsBytes = Encoding.Unicode.GetBytes(data);
            nint globalMemoryHandle = Kernel32.GlobalAlloc((uint) GlobalMemoryFlags.GMEM_MOVEABLE, (nuint) dataAsBytes.Length + 2);

            nint ptr = Kernel32.GlobalLock(globalMemoryHandle);
            Marshal.Copy(dataAsBytes, 0, ptr, dataAsBytes.Length);
            Marshal.WriteByte(ptr + dataAsBytes.Length, 0);
            Marshal.WriteByte(ptr + dataAsBytes.Length + 1, 0);
            Kernel32.GlobalUnlock(globalMemoryHandle);

            User32.SetClipboardData((uint) ClipboardFormat.CF_UNICODETEXT, globalMemoryHandle);
            User32.CloseClipboard();
            CheckError("ClipboardSet");

            // no need to free the clipboard memory, SetClipboardData will do it for us
            // (only if the copy is successful, but don't worry about it)
        }

        public override string GetClipboard()
        {
            User32.OpenClipboard(_helperWindowHandle);

            nint globalMemoryHandle = User32.GetClipboardData((uint) ClipboardFormat.CF_UNICODETEXT);

            if (globalMemoryHandle != 0)
            {
                nint ptr = Kernel32.GlobalLock(globalMemoryHandle);
                string str = Marshal.PtrToStringUni(ptr);
                Kernel32.GlobalUnlock(globalMemoryHandle);
                User32.CloseClipboard();
                return str;
            }

            User32.CloseClipboard();
            CheckError("ClipboardGet");

            return string.Empty;
        }

        #endregion
    }
}