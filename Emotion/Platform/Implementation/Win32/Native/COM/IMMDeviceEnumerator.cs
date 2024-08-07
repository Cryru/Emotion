﻿#region Using

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ComBaseApi.COM
{
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
}