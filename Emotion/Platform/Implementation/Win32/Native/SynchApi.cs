#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;
using WinApi;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi
{
    public static class SynchApi
    {
        public const string LIBRARY_NAME = "kernel32";

        [DllImport(LIBRARY_NAME, CharSet = Properties.BUILD_CHAR_SET)]
        public static extern IntPtr CreateEventW(IntPtr attributes, bool manualReset, bool initialState, string name);
    }
}
