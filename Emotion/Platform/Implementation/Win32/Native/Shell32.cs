#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Platform.Implementation.Win32.Native
{
    public static class Shell32
    {
        public const string DLL_NAME = "Shell32";

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-dragacceptfiles
        /// </summary>
        [DllImport(DLL_NAME, CharSet = CharSet.Auto)]
        public static extern void DragAcceptFiles(
            IntPtr hWnd,
            bool fAccept
        );
    }
}