#region Using

using System.Runtime.InteropServices;
using WinApi.Core;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ShCore
{
    public static class ShCoreMethods
    {
        public const string LibraryName = "shcore";

        [DllImport(LibraryName, ExactSpelling = true)]
        public static extern HResult GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX,
            out uint dpiY);
    }
}