#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;

//[GeneratedComInterface]
[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IMMDeviceEnumerator
{
    int EnumAudioEndpoints(DataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);

    [PreserveSig]
    int GetDefaultAudioEndpoint(DataFlow dataFlow, Role role, out IMMDevice endpoint);

    int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice deviceName);

    int RegisterEndpointNotificationCallback(IMMNotificationClient client);

    int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
}