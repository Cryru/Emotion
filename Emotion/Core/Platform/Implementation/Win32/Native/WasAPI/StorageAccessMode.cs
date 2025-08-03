#nullable enable

using Emotion;

namespace Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;

/// <summary>
/// MMDevice STGM enumeration
/// </summary>
public enum StorageAccessMode
{
    /// <summary>
    /// Read-only access mode.
    /// </summary>
    Read,

    /// <summary>
    /// Write-only access mode.
    /// </summary>
    Write,

    /// <summary>
    /// Read-write access mode.
    /// </summary>
    ReadWrite
}