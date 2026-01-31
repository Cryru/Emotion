#nullable enable

#region Using

using Emotion;
using Emotion.Core.Platform.Implementation.Win32.Native;
using System.Runtime.InteropServices;
using System.Text;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native.Kernel32;

public static class Kernel32
{
    public const string LibraryName = "kernel32";

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetLastError();

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/errhandlingapi/nf-errhandlingapi-setlasterror
    /// </summary>
    [DllImport(LibraryName)]
    public static extern void SetLastError(uint error);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DisableThreadLibraryCalls(nint hModule);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern void Sleep(uint dwMilliseconds);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetTickCount();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern ulong GetTickCount64();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool QueryPerformanceCounter(out long value);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool QueryPerformanceFrequency(out long value);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern void QueryUnbiasedInterruptTime(out ulong unbiasedTime);

    [DllImport(LibraryName)]
    public static extern void GetLocalTime(out SystemTime lpSystemTime);

    [DllImport(LibraryName)]
    public static extern bool SetLocalTime(ref SystemTime lpSystemTime);

    [DllImport(LibraryName)]
    public static extern void GetSystemTime(out SystemTime lpSystemTime);

    [DllImport(LibraryName)]
    public static extern bool SetSystemTime(ref SystemTime lpSystemTime);

    #region Console Functions

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool AllocConsole();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool FreeConsole();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool AttachConsole(uint dwProcessId);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint GetConsoleWindow();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint GetStdHandle(StdHandle nStdHandle);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SetStdHandle(uint nStdHandle, nint hHandle);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool SetConsoleTitle(string lpConsoleTitle);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern uint GetConsoleTitle(StringBuilder lpConsoleTitle, uint nSize);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SetConsoleWindowInfo(nint hConsoleOutput, int bAbsolute, in RectS lpConsoleWindow);

    [DllImport(LibraryName, SetLastError = true)]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport(LibraryName, SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    #endregion

    #region Memory Methods

    [DllImport(LibraryName, ExactSpelling = true, EntryPoint = "RtlZeroMemory")]
    public static extern void ZeroMemory(nint dest, nint size);

    [DllImport(LibraryName, ExactSpelling = true, EntryPoint = "RtlSecureZeroMemory")]
    public static extern void SecureZeroMemory(nint dest, nint size);

    #endregion

    #region Handle and Object Functions

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool CloseHandle(nint hObject);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DuplicateHandle(
        nint hSourceProcessHandle, nint hSourceHandle, nint hTargetProcessHandle,
        out nint lpTargetHandle,
        uint dwDesiredAccess,
        int bInheritHandle,
        DuplicateHandleFlags dwOptions);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool GetHandleInformation(nint hObject, out HandleInfoFlags lpdwFlags);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SetHandleInformation(nint hObject, HandleInfoFlags dwMask, HandleInfoFlags dwFlags);

    #endregion

    #region DLL Methods

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint GetModuleHandle(nint modulePtr);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool GetModuleHandleEx(GetModuleHandleFlags dwFlags, string lpModuleName,
        out nint phModule);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint GetModuleHandle(string lpModuleName);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint LoadLibrary(string fileName);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint LoadLibraryEx(string fileName, nint hFileReservedAlwaysZero,
        LoadLibraryFlags dwFlags);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool FreeLibrary(nint hModule);

    [DllImport(LibraryName, CharSet = CharSet.Ansi)]
    public static extern nint GetProcAddress(nint hModule, string procName);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool SetDllDirectory(string fileName);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SetDefaultDllDirectories(LibrarySearchFlags directoryFlags);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint AddDllDirectory(string newDirectory);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool RemoveDllDirectory(nint cookieFromAddDllDirectory);

    #endregion

    #region System Information Functions

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern uint GetSystemDirectory(StringBuilder lpBuffer, uint uSize);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern uint GetWindowsDirectory(StringBuilder lpBuffer, uint uSize);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetVersion();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool IsWow64Process(nint hProcess, out int isWow64Process);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern void GetNativeSystemInfo(out SystemInfo lpSystemInfo);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern void GetSystemInfo(out SystemInfo lpSystemInfo);

    #endregion

    #region Process and Thread Functions

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetCurrentProcessId();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint GetCurrentProcess();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetCurrentThreadId();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint GetCurrentThread();

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetCurrentProcessorNumber();

    #endregion

    #region File Management Functions

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern FileAttributes GetFileAttributes(string lpFileName);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool SetFileAttributes(string lpFileName, FileAttributes dwFileAttributes);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool GetFileAttributesEx(string lpFileName, FileAttributeInfoLevel fInfoLevelId,
        out FileAttributeData lpFileInformation);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint CreateFile(string lpFileName,
        uint dwDesiredAccess,
        FileShareMode dwShareMode,
        nint lpSecurityAttributes,
        FileCreationDisposition dwCreationDisposition,
        uint dwFlagsAndAttributes,
        nint hTemplateFile);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern nint CreateFile(string lpFileName,
        uint dwDesiredAccess,
        FileShareMode dwShareMode,
        ref SecurityAttributes lpSecurityAttributes,
        FileCreationDisposition dwCreationDisposition,
        uint dwFlagsAndAttributes,
        nint hTemplateFile);

    #endregion

    #region Char

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/stringapiset/nf-stringapiset-widechartomultibyte
    /// </summary>
    [DllImport(LibraryName, CharSet = CharSet.Auto)]
    public static extern int WideCharToMultiByte(CodePage codePage, uint dwFlags,
        [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpWideCharStr, int cchWideChar,
        [MarshalAs(UnmanagedType.LPArray)] char[] lpMultiByteStr, int cbMultiByte, nint lpDefaultChar,
        out bool lpUsedDefaultChar);

    #endregion

    [DllImport(LibraryName)]
    public static extern nint GlobalAlloc(uint uFlags, nuint dwBytes);

    [DllImport(LibraryName)]
    public static extern nint GlobalLock(nint hMem);

    [DllImport(LibraryName)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalUnlock(nint hMem);
}