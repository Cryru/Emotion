// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Libraries
{
    public static class Unix
    {
        [DllImport("libc")]
        public static extern int uname(IntPtr buf);
    }
}