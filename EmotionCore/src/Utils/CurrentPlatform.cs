// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Libraries;

#endregion

namespace Emotion.Utils
{
    /// <summary>
    /// Handles platform checking.
    /// </summary>
    public static class CurrentPlatform
    {
        public static PlatformName OS { get; private set; }

        static CurrentPlatform()
        {
            PlatformID pid = Environment.OSVersion.Platform;

            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    OS = PlatformName.Windows;
                    break;
                case PlatformID.MacOSX:
                    OS = PlatformName.Mac;
                    break;
                case PlatformID.Unix:

                    // Mac can return a value of Unix sometimes, We need to double check it.
                    IntPtr buf = IntPtr.Zero;
                    try
                    {
                        buf = Marshal.AllocHGlobal(8192);

                        if (Unix.uname(buf) == 0)
                        {
                            string sos = Marshal.PtrToStringAnsi(buf);
                            if (sos == "Darwin")
                            {
                                OS = PlatformName.Mac;
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

                    OS = PlatformName.Linux;
                    break;
                default:
                    OS = PlatformName.Other;
                    break;
            }
        }
    }

    public enum PlatformName
    {
        Windows,
        Linux,
        Mac,
        Other
    }
}