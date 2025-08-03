#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.COM;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;

[GeneratedComInterface]
[Guid("D666063F-1587-4E43-81F1-B948E807363F")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IMMDevice
{
    // activationParams is a propvariant
    [PreserveSig]
    int Activate(ref Guid id, ClsCtx clsCtx, IntPtr activationParams, out IAudioClient interfacePointer);

    [PreserveSig]
    int OpenPropertyStore(StorageAccessMode stgmAccess, out IPropertyStore properties);

    [PreserveSig]
    int GetId([MarshalAs(UnmanagedType.LPWStr)] out string? id);

    [PreserveSig]
    int GetState(out DeviceState state);
}