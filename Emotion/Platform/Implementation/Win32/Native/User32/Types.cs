#region Using

using System;
using System.Runtime.InteropServices;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.User32
{
    public delegate IntPtr WindowProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public delegate IntPtr GetMsgProc(int code, IntPtr wParam, IntPtr lParam);

    public delegate void TimerProc(IntPtr hWnd, uint uMsg, IntPtr nIdEvent, uint dwTickCountMillis);

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref Rect rect, object dwData);

    [StructLayout(LayoutKind.Sequential)]
    public struct Message
    {
        public IntPtr Hwnd;
        public WM Value;
        public IntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public Point Point;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PaintStruct
    {
        public IntPtr HandleDC;
        public int EraseBackgroundValue;
        public Rect PaintRect;
        private int ReservedInternalRestore;
        private int ReservedInternalIncUpdate;
        private fixed byte ReservedInternalRgb[32];

        public bool ShouldEraseBackground
        {
            get => EraseBackgroundValue > 0;
            set => EraseBackgroundValue = value ? 1 : 0;
        }
    }

    /// <summary>
    /// Note: Marshalled
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WindowClassEx
    {
        public uint Size;
        public WindowClassStyles Styles;
        [MarshalAs(UnmanagedType.FunctionPtr)] public WindowProc WindowProc;
        public int ClassExtraBytes;
        public int WindowExtraBytes;
        public IntPtr InstanceHandle;
        public IntPtr IconHandle;
        public IntPtr CursorHandle;
        public IntPtr BackgroundBrushHandle;
        public string MenuName;
        public string ClassName;
        public IntPtr SmallIconHandle;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct WindowClassExBlittable
    {
        public uint Size;
        public WindowClassStyles Styles;
        public IntPtr WindowProc;
        public int ClassExtraBytes;
        public int WindowExtraBytes;
        public IntPtr InstanceHandle;
        public IntPtr IconHandle;
        public IntPtr CursorHandle;
        public IntPtr BackgroundBrushHandle;
        public IntPtr MenuName;
        public IntPtr ClassName;
        public IntPtr SmallIconHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowInfo
    {
        public uint Size;
        public Rect WindowRect;
        public Rect ClientRect;
        public WindowStyles Styles;
        public WindowExStyles ExStyles;
        public uint WindowStatus;
        public uint BorderX;
        public uint BorderY;
        public ushort WindowType;
        public ushort CreatorVersion;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DeviceMode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;

        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public DeviceUp dmFields;

        //public short dmOrientation;
        //public short dmPaperSize;
        //public short dmPaperLength;
        //public short dmPaperWidth;
        //public short dmScale;
        //public short dmCopies;
        //public short dmDefaultSource;
        //public short dmPrintQuality;

        public Point dmPosition;
        public ScreenOrientation dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;

        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;

        public uint dmDisplayFlags;

        //public uint dmNup;
        public uint dmDisplayFrequency;
        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;
        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    public unsafe struct DevBroadcastDeviceInterfaceW
    {
        public uint DbccSize;
        public DeviceType DbccDeviceType;
        public uint DbccReserved;
        public Guid DbccClassGuid;
        public fixed char DbccName[1];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ChangeFilter
    {
        public uint Size;
        public MessageFilterInfo Info;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DisplayDeviceW
    {
        public uint Cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        public DisplayDeviceStateFlags StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CreateStruct
    {
        public IntPtr CreateParams;
        public IntPtr InstanceHandle;
        public IntPtr MenuHandle;
        public IntPtr ParentHwnd;
        public int Height;
        public int Width;
        public int Y;
        public int X;
        public WindowStyles Styles;
        public IntPtr Name;
        public IntPtr ClassName;
        public WindowExStyles ExStyles;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPlacement
    {
        public uint Size;
        public WindowPlacementFlags Flags;
        public ShowWindowCommands ShowCmd;
        public Point MinPosition;
        public Point MaxPosition;
        public Rect NormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public AlphaFormat AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AnimationInfo
    {
        /// <summary>
        /// Creates an AMINMATIONINFO structure.
        /// </summary>
        /// <param name="iMinAnimate">If non-zero and SPI_SETANIMATION is specified, enables minimize/restore animation.</param>
        public AnimationInfo(int iMinAnimate)
        {
            Size = (uint) Marshal.SizeOf<AnimationInfo>();
            MinAnimate = iMinAnimate;
        }

        /// <summary>
        /// Always must be set to (System.UInt32)Marshal.SizeOf(typeof(ANIMATIONINFO)).
        /// </summary>
        public uint Size;

        /// <summary>
        /// If non-zero, minimize/restore animation is enabled, otherwise disabled.
        /// </summary>
        public int MinAnimate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MinimizedMetrics
    {
        public uint Size;
        public int Width;
        public int HorizontalGap;
        public int VerticalGap;
        public ArrangeFlags Arrange;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TrackMouseEventOptions
    {
        public uint Size;
        public TrackMouseEventFlags Flags;
        public IntPtr TrackedHwnd;
        public uint HoverTime;

        public const uint DefaultHoverTime = 0xFFFFFFFF;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MinMaxInfo
    {
        private Point Reserved;

        /// <summary>
        /// The maximized width (x member) and the maximized height (y member) of the window. For top-level windows, this value
        /// is based on the width of the primary monitor.
        /// </summary>
        public Point MaxSize;

        /// <summary>
        /// The position of the left side of the maximized window (x member) and the position of the top of the maximized
        /// window (y member). For top-level windows, this value is based on the position of the primary monitor.
        /// </summary>
        public Point MaxPosition;

        /// <summary>
        /// The minimum tracking width (x member) and the minimum tracking height (y member) of the window. This value can be
        /// obtained programmatically from the system metrics SM_CXMINTRACK and SM_CYMINTRACK (see the GetSystemMetrics
        /// function).
        /// </summary>
        public Point MinTrackSize;

        /// <summary>
        /// The maximum tracking width (x member) and the maximum tracking height (y member) of the window. This value is based
        /// on the size of the virtual screen and can be obtained programmatically from the system metrics SM_CXMAXTRACK and
        /// SM_CYMAXTRACK (see the GetSystemMetrics function).
        /// </summary>
        public Point MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPosition
    {
        public IntPtr Hwnd;
        public IntPtr HwndZOrderInsertAfter;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public WindowPositionFlags Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NcCalcSizeParams
    {
        public NcCalcSizeRegionUnion Region;
        public WindowPosition* Position;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NcCalcSizeRegionUnion
    {
        [FieldOffset(0)] public NcCalcSizeInput Input;
        [FieldOffset(0)] public NcCalcSizeOutput Output;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NcCalcSizeInput
    {
        public Rect TargetWindowRect;
        public Rect CurrentWindowRect;
        public Rect CurrentClientRect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NcCalcSizeOutput
    {
        public Rect TargetClientRect;
        public Rect DestRect;
        public Rect SrcRect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfo
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. Note that
        /// if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public Rect MonitorRect;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor, expressed in virtual-screen
        /// coordinates. Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may
        /// be negative values.
        /// </summary>
        public Rect WorkRect;

        /// <summary>
        /// A set of flags that represent attributes of the display monitor.
        /// </summary>
        public MonitorInfoFlag Flags;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MonitorInfoEx
    {
        /// <summary>
        /// The size of the structure, in bytes.
        /// </summary>
        public uint Size;

        /// <summary>
        /// A RECT structure that specifies the display monitor rectangle, expressed in virtual-screen coordinates. Note that
        /// if the monitor is not the primary display monitor, some of the rectangle's coordinates may be negative values.
        /// </summary>
        public Rect MonitorRect;

        /// <summary>
        /// A RECT structure that specifies the work area rectangle of the display monitor, expressed in virtual-screen
        /// coordinates. Note that if the monitor is not the primary display monitor, some of the rectangle's coordinates may
        /// be negative values.
        /// </summary>
        public Rect WorkRect;

        /// <summary>
        /// A set of flags that represent attributes of the display monitor.
        /// </summary>
        public MonitorInfoFlag Flags;

        /// <summary>
        /// A string that specifies the device name of the monitor being used.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string SzDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TitleBarInfo
    {
        public uint Size;
        public Rect TitleBarRect;
        public ElementSystemStates TitleBarStates;
        private ElementSystemStates Reserved;
        public ElementSystemStates MinimizeButtonStates;
        public ElementSystemStates MaximizeButtonStates;
        public ElementSystemStates HelpButtonStates;
        public ElementSystemStates CloseButtonStates;
    }
}