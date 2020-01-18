#region Using

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi
{
    public static class Shell32
    {
        public const string LIBRARY_NAME = "Shell32";

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-dragacceptfiles
        /// </summary>
        [DllImport(LIBRARY_NAME, CharSet = CharSet.Auto)]
        public static extern void DragAcceptFiles(
            IntPtr hWnd,
            bool fAccept
        );
    }
}