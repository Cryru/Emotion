#region Using

using System.Runtime.InteropServices;
using WinApi;
using WinApi.Gdi32;
using WinApi.ShCore;
using WinApi.User32;
using User32 = WinApi.User32.User32Methods;
using ShCore = WinApi.ShCore.ShCoreMethods;
using Gdi32 = WinApi.Gdi32.Gdi32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32
{
    public sealed class Win32Monitor : Monitor
    {
        public IntPtr Handle { get; private set; }
        public bool ModesPruned { get; private set; }

        public Win32Monitor(DisplayDeviceW adapter, DisplayDeviceW? display)
        {
            Name = display != null ? display.Value.DeviceString : adapter.DeviceString;
            AdapterName = adapter.DeviceName;
            DeviceName = display == null ? "<<Unknown Device>>" : display.Value.DeviceName;
            Assert(Name != null);

            var dm = new DeviceMode();
            dm.dmSize = (short) Marshal.SizeOf(dm);
            User32.EnumDisplaySettingsW(adapter.DeviceName.PadRight(32, '\0'), (int) DisplaySettings.CurrentSettings, ref dm);
            Win32Platform.CheckError("Enumerating display settings.");

            Width = dm.dmPelsWidth;
            Height = dm.dmPelsHeight;

            IntPtr dc = Gdi32.CreateDCW("DISPLAY", adapter.DeviceName, null, IntPtr.Zero);
            Assert(dc != IntPtr.Zero);

            if (Win32Platform.IsWindows81OrGreater)
            {
                WidthPhysical = Gdi32.GetDeviceCaps(dc, DeviceCap.HorizontalSize);
                HeightPhysical = Gdi32.GetDeviceCaps(dc, DeviceCap.VertSize);
            }
            else
            {
                WidthPhysical = (int) (dm.dmPelsWidth * 25.4f / Gdi32.GetDeviceCaps(dc, DeviceCap.LogPixelsX));
                HeightPhysical = (int) (dm.dmPelsHeight * 25.4f / Gdi32.GetDeviceCaps(dc, DeviceCap.LogPixelsY));
            }

            Gdi32.DeleteDC(dc);

            if ((adapter.StateFlags & DisplayDeviceStateFlags.ModesPruned) != 0) ModesPruned = true;

            var rect = new Rect
            {
                Left = dm.dmPosition.X,
                Top = dm.dmPosition.Y,
                Right = dm.dmPosition.X + dm.dmPelsWidth,
                Bottom = dm.dmPosition.Y + dm.dmPelsHeight
            };

            User32.EnumDisplayMonitors(IntPtr.Zero, ref rect, MonitorCallback, null);
            Win32Platform.CheckError("Creating monitor.");
        }

        private bool MonitorCallback(IntPtr hMonitor, IntPtr hdc, ref Rect rect, object dwData)
        {
            var monitorInfo = new MonitorInfoEx();
            monitorInfo.Size = (uint) Marshal.SizeOf(monitorInfo);

            // Try to get monitor info.
            if (!User32.GetMonitorInfo(hMonitor, ref monitorInfo)) return false;
            // Check if the adapter name matches, in which case this is the handle to this monitor.
            if (monitorInfo.SzDevice == AdapterName) Handle = hMonitor;

            Position = new Vector2(monitorInfo.MonitorRect.Left, monitorInfo.MonitorRect.Top);

            return true;
        }

        public static void GetMonitorContentScale(IntPtr monitor, out float scaleX, out float scaleY)
        {
            uint xDpi, yDpi;

            if (Win32Platform.IsWindows81OrGreater)
            {
                ShCore.GetDpiForMonitor(monitor, MonitorDpiType.MDT_EFFECTIVE_DPI, out xDpi, out yDpi);
            }
            else
            {
                IntPtr dc = User32.GetDC(IntPtr.Zero);
                xDpi = (uint) Gdi32.GetDeviceCaps(dc, DeviceCap.LogPixelsX);
                yDpi = (uint) Gdi32.GetDeviceCaps(dc, DeviceCap.LogPixelsY);
                User32.ReleaseDC(IntPtr.Zero, dc);
            }

            scaleX = xDpi / (float) Win32Platform.DEFAULT_DPI;
            scaleY = yDpi / (float) Win32Platform.DEFAULT_DPI;
        }
    }
}