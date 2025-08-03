#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.COM;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Native.COM;

/// <summary>
/// is defined in propsys.h
/// </summary>
[GeneratedComInterface]
[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IPropertyStore
{
    [PreserveSig]
    int GetCount(out int propCount);

    [PreserveSig]
    int GetAt(int property, out PropertyKey key);

    [PreserveSig]
    int GetValue(ref PropertyKey key, out PropVariant value);

    [PreserveSig]
    int SetValue(ref PropertyKey key, ref PropVariant value);

    [PreserveSig]
    int Commit();
}