#nullable enable

using Emotion;
using System.Runtime.InteropServices;

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native.Kernel32;

[StructLayout(LayoutKind.Sequential)]
public struct SystemInfo
{
    public ushort ProcessorArchitecture;
    private ushort Reserved;
    public uint PageSize;
    public nint MinimumApplicationAddress;
    public nint MaximumApplicationAddress;
    public nint ActiveProcessorMask;
    public uint NumberOfProcessors;
    public uint ProcessorType;
    public uint AllocationGranularity;
    public ushort ProcessorLevel;
    public ushort ProcessorRevision;

    public uint OemId
    {
        get => (uint) ProcessorArchitecture << 8 | Reserved;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SecurityAttributes
{
    public uint Length;
    public nint SecurityDescriptor;
    public uint IsHandleInheritedValue;

    public bool IsHandleInherited
    {
        get => IsHandleInheritedValue > 0;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct FileTime
{
    public uint Low;
    public uint High;

    public ulong Value
    {
        get => (ulong) High << 32 | Low;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SystemTime
{
    public ushort Year;
    public ushort Month;
    public ushort DayOfWeek;
    public ushort Day;
    public ushort Hour;
    public ushort Minute;
    public ushort Second;
    public ushort Milliseconds;
}

[StructLayout(LayoutKind.Sequential)]
public struct FileAttributeData
{
    public FileAttributes Attributes;
    public FileTime CreationTime;
    public FileTime LastAccessTime;
    public FileTime LastWriteTime;

    public uint FileSizeHigh;
    public uint FileSizeLow;

    public ulong FileSize
    {
        get => (ulong) FileSizeHigh << 32 | FileSizeLow;
    }
}