#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.COM;
using Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;

/// <summary>
/// IMMNotificationClient
/// </summary>
[GeneratedComInterface]
[Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IMMNotificationClient
{
    /// <summary>
    /// Device State Changed
    /// </summary>
    void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, DeviceState newState);

    /// <summary>
    /// Device Added
    /// </summary>
    void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);

    /// <summary>
    /// Device Removed
    /// </summary>
    void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

    /// <summary>
    /// Default Device Changed
    /// </summary>
    void OnDefaultDeviceChanged(DataFlow flow, Role role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

    /// <summary>
    /// Property Value Changed
    /// </summary>
    /// <param name="pwstrDeviceId"></param>
    /// <param name="key"></param>
    void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key);
}