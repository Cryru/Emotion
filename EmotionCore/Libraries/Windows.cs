// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Libraries
{
    /// <summary>
    /// Functions relating to the Windows OS.
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// Tells the OS in which folder it should look for DLL files.
        /// This only works on Windows, on other OSs this is done through
        /// the Mono dllmap.
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
    }
}