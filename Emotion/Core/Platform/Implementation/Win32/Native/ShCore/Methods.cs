#nullable enable

#region Using

using Emotion;
using Emotion.Core.Platform.Implementation.Win32.Native.Core;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Native.ShCore;

public static class ShCore
{
    public const string LibraryName = "shcore";

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern HResult GetDpiForMonitor(nint hmonitor, MonitorDpiType dpiType, out uint dpiX,
        out uint dpiY);
}