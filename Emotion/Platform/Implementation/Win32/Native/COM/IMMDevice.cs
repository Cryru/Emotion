#region Using

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ComBaseApi.COM;

#nullable enable

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