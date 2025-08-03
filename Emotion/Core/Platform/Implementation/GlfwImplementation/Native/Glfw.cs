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
    }
}