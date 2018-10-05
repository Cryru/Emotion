// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Libraries
{
    public static class Unix
    {
        #region dlopen Flags

        // Info: https://linux.die.net/man/3/dlopen

        public const int RTLD_LAZY = 1;

        public const int RTLD_NOW = 2;

        #endregion

        [DllImport("libc")]
        public static extern int uname(IntPtr buf);

        [DllImport("libdl.so")]
        public static extern IntPtr dlopen(string fileName, int flags);
    }
}