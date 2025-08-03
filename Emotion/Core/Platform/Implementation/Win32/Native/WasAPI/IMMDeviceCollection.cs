#nullable enable

#region Using

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;

[GeneratedComInterface]
[Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IMMDeviceCollection
{
    [PreserveSig]
    int GetCount(out int numDevices);

    [PreserveSig]
    int Item(int deviceNumber, out IMMDevice device);
}