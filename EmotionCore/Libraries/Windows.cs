// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.External
{
    internal static class Windows
    {
        /// <summary>
        /// Tells the OS in which folder it should look for DLL files.
        /// This only works on Windows, on other OSs this is done through
        /// the Mono dllmap.
        /// </summary>
        /// <param name="path">The path in which to look.</param>
        /// <returns>Whether the operation was successful.</returns>
        [DllImport("kernel32.dll")]
        internal static extern bool SetDllDirectory(string path);
    }
}