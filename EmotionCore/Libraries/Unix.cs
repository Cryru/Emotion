// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion.Libraries
{
    /// <summary>
    /// Functions relating to the Unix family of OS.
    /// </summary>
    public static class Unix
    {
        #region dlopen Flags

        // Info: https://linux.die.net/man/3/dlopen

        /// <summary>
        /// Whether to load when a function from the module is used.
        /// </summary>
        public const int RTLD_LAZY = 1;

        /// <summary>
        /// Whether to load now.
        /// </summary>
        public const int RTLD_NOW = 2;

        #endregion

        /// <summary>
        /// Returns the name of the OS. Used to distinguish between older Macs and Linux.
        /// Used by CurrentPlatform.cs
        /// </summary>
        /// <param name="buf">Buffer to the string.</param>
        /// <returns>Whether getting the name was successful.</returns>
        [DllImport("libc")]
        public static extern int uname(IntPtr buf);

        /// <summary>
        /// Loads the dynamic library file named by the null-terminated string filename and returns an opaque "handle" for the
        /// dynamic library.
        /// This functions will sometimes be located in libc.
        /// </summary>
        /// <param name="fileName">The path to the library to load.</param>
        /// <param name="flags">Load flags.</param>
        /// <returns></returns>
        [DllImport("libdl")]
        public static extern IntPtr dlopen(string fileName, int flags);

        /// <summary>
        /// Internal function for getting the current working directory.
        /// </summary>
        /// <param name="buf">String buffer to load with the current working directory string.</param>
        /// <param name="size">The size of the buffer.</param>
        /// <returns>?</returns>
        [DllImport("MonoPosixHelper", SetLastError = true, EntryPoint = "Mono_Posix_Syscall_getcwd")]
        public static extern IntPtr getcwd([Out] StringBuilder buf, ulong size);

        /// <summary>
        /// Get the name of the current working directory.
        /// </summary>
        /// <param name="buf">An string buffer to be filled with the current working directory.</param>
        /// <returns>The string buffer containing the current directory.</returns>
        public static StringBuilder getcwd(StringBuilder buf)
        {
            getcwd(buf, (ulong) buf.Capacity);
            return buf;
        }

        /// <summary>
        /// Changes the current directory. Used for dynamic library mapping.
        /// </summary>
        /// <param name="path">Path to the new directory.</param>
        /// <returns>?</returns>
        [DllImport("libc", SetLastError = true)]
        public static extern int chdir(string path);
    }
}