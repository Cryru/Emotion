#region Using

using System.Runtime.InteropServices;

#endregion

namespace FreeImageAPI.IO
{
    /// <summary>
    /// Structure for implementing access to custom handles.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FreeImageIO
    {
        /// <summary>
        /// Delegate to the C++ function <b>fread</b>.
        /// </summary>
        public ReadProc readProc;

        /// <summary>
        /// Delegate to the C++ function <b>fwrite</b>.
        /// </summary>
        public WriteProc writeProc;

        /// <summary>
        /// Delegate to the C++ function <b>fseek</b>.
        /// </summary>
        public SeekProc seekProc;

        /// <summary>
        /// Delegate to the C++ function <b>ftell</b>.
        /// </summary>
        public TellProc tellProc;
    }
}