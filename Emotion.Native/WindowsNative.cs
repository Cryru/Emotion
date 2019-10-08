#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Native
{
    /// <summary>
    /// Functions relating to the Window OS api.
    /// </summary>
    public static class WindowsNative
    {
        /// <summary>
        /// Tells the OS in which folder it should look for DLL files.
        /// </summary>
        /// <param name="path">The path in which to look.</param>
        /// <returns>Whether the operation was successful.</returns>
        [DllImport("kernel32.dll")]
        public static extern bool SetDllDirectory(string path);

        /// <summary>
        /// Returns a handle to a loaded dll. Used to detect RenderDoc.
        /// </summary>
        /// <param name="lpModuleName">The name of the dll to look for.</param>
        /// <returns>A pointer to the dll, or zero pointer if not loaded.</returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Returns the error code of the last win32 error occured.
        /// </summary>
        /// <returns>The error code of the last win32 error occured.</returns>
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        /// <summary>
        /// Display a message box.
        /// </summary>
        /// <param name="win">The window this message box belongs to. Pass IntPtr.Zero for none.</param>
        /// <param name="text">The text of the message box.</param>
        /// <param name="caption">The title of the message box.</param>
        /// <param name="type">
        /// The type of message box. Lookup types here -
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-messagebox
        /// </param>
        /// <returns>Input result of the message box.</returns>
        [DllImport("user32.dll")]
        public static extern uint MessageBox(IntPtr win, string text, string caption, uint type);
    }
}