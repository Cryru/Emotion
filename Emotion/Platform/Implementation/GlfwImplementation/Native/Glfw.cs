#region Using

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#endregion

// ReSharper disable once CheckNamespace
namespace Emotion.Platform.Implementation.GlfwImplementation.Native
{
    /// <summary>
    /// <a href="http://www.glfw.org/">GLFW 3</a> bindings.
    /// </summary>
    public static partial class Glfw
    {
        private const string LIBRARY_NAME = "glfw";

        /// <summary>
        /// Adds the specified native directory path to the Path environment variable to facilitate
        /// native loading.
        /// </summary>
        /// <param name="nativeDirectory">
        /// The directory that the native library is stored
        /// in.
        /// </param>
        /// <exception cref="DirectoryNotFoundException">
        /// When <paramref name="nativeDirectory" /> is
        /// not found.
        /// </exception>
        public static void ConfigureNativesDirectory(string nativeDirectory)
        {
            if (Directory.Exists(nativeDirectory))
                Environment.SetEnvironmentVariable("Path", Environment.GetEnvironmentVariable("Path") + ";" + Path.GetFullPath(nativeDirectory) + ";");
            else
                throw new DirectoryNotFoundException(nativeDirectory);
        }

        /// <summary>
        /// GLFW_DONT_CARE
        /// </summary>
        public static readonly int DontCare = -1;

        /// <summary>
        /// The major version number of the GLFW library. This is incremented when the API is
        /// changed in non-compatible ways.
        /// </summary>
        public static readonly int VersionMajor = 3;

        /// <summary>
        /// The minor version number of the GLFW library. This is incremented when features are
        /// added to the API but it remains backward-compatible.
        /// </summary>
        public static readonly int VersionMinor = 2;

        /// <summary>
        /// The revision number of the GLFW library. This is incremented when a bug fix release is
        /// made that does not contain any API changes.
        /// </summary>
        public static readonly int VersionRevision = 1;

        // string <> utf8 utility functions
        internal static IntPtr ToUTF8(string text)
        {
            int len = Encoding.UTF8.GetByteCount(text);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            var nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

        internal static string FromUTF8(IntPtr ptr)
        {
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
                ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(ptr, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}