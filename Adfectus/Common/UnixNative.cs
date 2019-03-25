#region Using

using System.Runtime.InteropServices;

#endregion

namespace Adfectus.Common
{
    /// <summary>
    /// Functions relating to the Unix family of OS.
    /// </summary>
    public static class UnixNative
    {
        /// <summary>
        /// Changes the current directory. Used for dynamic library mapping.
        /// </summary>
        /// <param name="path">Path to the new directory.</param>
        /// <returns>?</returns>
        [DllImport("libc", SetLastError = true)]
        public static extern int chdir(string path);
    }
}