#region Using

using System;
using System.Runtime.InteropServices;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi
{
    public static unsafe class NtDll
    {
        public const string LIBRARY_NAME = "ntdll";

        /// <summary>
        /// https://docs.microsoft.com/en-us/cpp/porting/modifying-winver-and-win32-winnt?view=vs-2019
        /// </summary>
        public enum WinVer : ushort
        {
            Win32WinNTNt4 = 0x0400, // Windows NT 4.0
            Win32WinNTWin2K = 0x0500, // Windows 2000
            Win32WinNTWinXP = 0x0501, // Windows XP
            Win32WinNTWs03 = 0x0502, // Windows Server 2003
            Win32WinNTWin6 = 0x0600, // Windows Vista
            Win32WinNTVista = 0x0600, // Windows Vista
            Win32WinNTWs08 = 0x0600, // Windows Server 2008
            Win32WinNTLonghorn = 0x0600, // Windows Vista
            Win32WinNTWin7 = 0x0601, // Windows 7
            Win32WinNTWin8 = 0x0602, // Windows 8
            Win32WinNTWinBlue = 0x0603, // Windows 8.1
            Win32WinNTWinThreshold = 0x0A00, // Windows 10
            Win32WinNTWin10 = 0x0A00 // Windows 10
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/ns-wdm-_osversioninfow
        /// </summary>
        public struct OsVersionInfoW
        {
            public uint DwOSVersionInfoSize;
            public uint DwMajorVersion;
            public uint DwMinorVersion;
            public uint DwBuildNumber;
            public uint DwPlatformId;
            public fixed char SzCsdVersion[128];
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/ns-wdm-_osversioninfoexw
        /// </summary>
        public struct OsVersionInfoEXW
        {
            public uint DwOSVersionInfoSize;
            public uint DwMajorVersion;
            public uint DwMinorVersion;
            public uint DwBuildNumber;
            public uint DwPlatformId;

            // ReSharper disable once UnassignedField.Global
            public fixed char SzCsdVersion[128];
            public ushort WServicePackMajor;
            public ushort WServicePackMinor;
            public ushort WSuiteMask;
            public byte WProductType;
            public byte WReserved;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55
        /// </summary>
        public enum NtStatus : uint
        {
            StatusSuccess = 0,
            StatusInvalidParameter = 0xC000000D,
            StatusRevisionMismatch = 0xC0000059
        }

        /// <summary>
        /// https://raw.githubusercontent.com/tpn/winsdk-10/master/Include/10.0.14393.0/km/wdm.h
        /// </summary>
        [Flags]
        public enum TypeMask : uint
        {
            VerMinorVersion = 0x0000001,
            VerMajorVersion = 0x0000002,
            VerBuildNumber = 0x0000004,
            VerPlatformId = 0x0000008,
            VerServicePackMinor = 0x0000010,
            VerServicePackMajor = 0x0000020,
            VerSuiteName = 0x0000040,
            VerProductType = 0x0000080
        }

        /// <summary>
        /// https://raw.githubusercontent.com/tpn/winsdk-10/master/Include/10.0.14393.0/km/wdm.h
        /// </summary>
        public enum Condition : byte
        {
            VerEqual = 1,
            VerGreater = 2,
            VerGreaterEqual = 3,
            VerLess = 4,
            VerLessEqual = 5,
            VerAnd = 6,
            VerOr = 7
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/api/winnt/nf-winnt-versetconditionmask
        /// </summary>
        /// <param name="conditionMask"></param>
        /// <param name="typeMask"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        [DllImport(LIBRARY_NAME)]
        public static extern ulong VerSetConditionMask(
            ulong conditionMask,
            TypeMask typeMask,
            Condition condition
        );

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/desktop/devnotes/rtlgetversion
        /// </summary>
        [DllImport(LIBRARY_NAME)]
        public static extern int RtlGetVersion(out OsVersionInfoW info);

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/content/wdm/nf-wdm-rtlverifyversioninfo
        /// </summary>
        [DllImport(LIBRARY_NAME)]
        public static extern NtStatus RtlVerifyVersionInfo(
            ref OsVersionInfoEXW versionInfo,
            TypeMask typeMask,
            ulong conditionMask
        );
    }
}