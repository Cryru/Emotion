// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Libraries
{
    public static class CurrentPlatform
    {
        public static PlatformID OS { get; private set; }

        [DllImport("libc")]
        private static extern int uname(IntPtr buf);

        static CurrentPlatform()
        {
            PlatformID pid = Environment.OSVersion.Platform;

            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    OS = PlatformID.Win32NT;
                    break;
                case PlatformID.MacOSX:
                    OS = PlatformID.MacOSX;
                    break;
                case PlatformID.Unix:

                    // Mac can return a value of Unix sometimes, We need to double check it.
                    IntPtr buf = IntPtr.Zero;
                    try
                    {
                        buf = Marshal.AllocHGlobal(8192);

                        if (uname(buf) == 0)
                        {
                            string sos = Marshal.PtrToStringAnsi(buf);
                            if (sos == "Darwin")
                            {
                                OS = PlatformID.MacOSX;
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        if (buf != IntPtr.Zero)
                            Marshal.FreeHGlobal(buf);
                    }

                    OS = PlatformID.Unix;
                    break;
                default:
                    OS = PlatformID.Xbox;
                    break;
            }
        }
    }
}